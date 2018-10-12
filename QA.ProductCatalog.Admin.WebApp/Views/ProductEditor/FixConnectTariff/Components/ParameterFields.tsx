import React, { Component, Fragment } from "react";
import cn from "classnames";
import { consumer, inject } from "react-ioc";
import { observable, runInAction, autorun, IObservableArray, IReactionDisposer } from "mobx";
import { observer } from "mobx-react";
import { Intent } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { Options } from "react-select";
import { WeakCache, ComputedCache } from "Utils/WeakCache";
import { asc } from "Utils/Array/Sort";
import { DataContext } from "Services/DataContext";
import { InputNumber, Select } from "Components/FormControls/FormControls";
import { FieldEditorProps } from "Components/ArticleEditor/ArticleEditor";
import { RelationFieldSchema, NumericFieldSchema } from "Models/EditorSchemaModels";
import {
  Tables,
  LinkParameter,
  ProductParameter,
  BaseParameter,
  Unit,
  BaseParameterModifier
} from "../TypeScriptSchema";

type Parameter = ProductParameter | LinkParameter;

interface ParameterField {
  Title: string;
  Unit: string;
  BaseParam: string;
  BaseParamModifiers?: string[];
}

interface ParameterFieldsProps extends FieldEditorProps {
  fields?: ParameterField[];
}

const unitOptionsCache = new WeakCache();
const unitByAliasCache = new ComputedCache();
const baseParamsByAliasCache = new ComputedCache();
const baseParamModifiersByAliasCache = new ComputedCache();

@consumer
@observer
export class ParameterFields extends Component<ParameterFieldsProps> {
  @inject private _dataContext: DataContext<Tables>;
  @observable private isMounted = false;
  private reactions: IReactionDisposer[] = [];
  private fieldOrdersByTitile: { [title: string]: number } = {};
  private virtualParameters: Parameter[];

  componentDidMount() {
    const { fields = [] } = this.props;
    const contentName = this.getContentName();
    const unitsByAlias = this.getUnitsByAlias();
    const baseParamsByAlias = this.getBaseParamsByAlias();
    const baseParamModifiersByAlias = this.getBaseParameterModifiersByAlias();

    fields.forEach((field, i) => {
      this.fieldOrdersByTitile[field.Title] = i;
    });

    runInAction("createParameters", () => {
      // create virtual paramters in context table
      this.virtualParameters = fields.map(field =>
        this._dataContext.createEntity(contentName, {
          _IsVirtual: true,
          Title: field.Title,
          Unit: unitsByAlias[field.Unit],
          BaseParameter: baseParamsByAlias[field.BaseParam],
          BaseParameterModifiers:
            field.BaseParamModifiers &&
            field.BaseParamModifiers.map(alias => baseParamModifiersByAlias[alias])
        })
      );
    });

    this.reactions.push(
      autorun(() => {
        const parameters = this.getParameters();
        // find missing virtual parameters then add it to entity field
        const parametersToAdd = this.virtualParameters.filter(
          virtual => !parameters.some(parameter => parameter.Title === virtual.Title)
        );
        if (parametersToAdd.length > 0) {
          runInAction("addParameters", () => {
            parametersToAdd.forEach(parameter => {
              parameter.restoreBaseValues();
              parameter.setUntouched();
              // @ts-ignore
              parameter.clearErrors();
            });
            parameters.push(...parametersToAdd);
          });
        }
      })
    );

    this.reactions.push(
      autorun(() => {
        const editedParameters = this.getParameters().filter(
          parameter => parameter._IsVirtual && parameter.isEdited()
        );
        if (editedParameters.length > 0) {
          runInAction("materializeParameters", () => {
            editedParameters.forEach(parameter => {
              parameter._IsVirtual = false;
            });
          });
        }
      })
    );

    runInAction("componentDidMount", () => {
      this.isMounted = true;
    });
  }

  componentWillUnmount() {
    this.reactions.forEach(disposer => disposer());

    runInAction("componentWillUnmount", () => {
      const parameters = this.getParameters();
      if (parameters.some(parameter => parameter._IsVirtual)) {
        // remove virtual parameters from entity field
        parameters.replace(parameters.filter(parameter => !parameter._IsVirtual));
      }
      // remove virtual parameters from context table
      this.virtualParameters.forEach(virtual => {
        if (virtual._IsVirtual) {
          this._dataContext.deleteEntity(virtual);
        }
      });
    });
  }

  private getParameters(): IObservableArray<Parameter> {
    const { model, fieldSchema } = this.props;
    return model[fieldSchema.FieldName];
  }

  private getContentName(): string {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    return fieldSchema.RelatedContent.ContentName;
  }

  // BaseParameter.PreloadingMode должно быть PreloadingMode.Eager
  private getBaseParamsByAlias(): { [alias: string]: BaseParameter } {
    return baseParamsByAliasCache.getOrAdd(this._dataContext, { keepAlive: true }, () => {
      const byAlias = {};
      for (const baseParameter of this._dataContext.tables.BaseParameter.values()) {
        byAlias[baseParameter.Alias] = baseParameter;
      }
      return byAlias;
    });
  }

  // BaseParameterModifiers.PreloadingMode должно быть PreloadingMode.Eager
  private getBaseParameterModifiersByAlias(): { [alias: string]: BaseParameterModifier } {
    return baseParamModifiersByAliasCache.getOrAdd(this._dataContext, { keepAlive: true }, () => {
      const byAlias = {};
      for (const modifier of this._dataContext.tables.BaseParameterModifier.values()) {
        byAlias[modifier.Alias] = modifier;
      }
      return byAlias;
    });
  }

  // Unit.PreloadingMode должно быть PreloadingMode.Eager
  private getUnitsByAlias(): { [alias: string]: Unit } {
    return unitByAliasCache.getOrAdd(this._dataContext, { keepAlive: true }, () => {
      const byAlias = {};
      for (const unit of this._dataContext.tables.Unit.values()) {
        byAlias[unit.Alias] = unit;
      }
      return byAlias;
    });
  }

  // Unit.PreloadingMode должно быть PreloadingMode.Eager
  private getUnitOptions(): Options {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    const unitFieldSchema = fieldSchema.RelatedContent.Fields.Unit as RelationFieldSchema;

    return unitOptionsCache.getOrAdd(fieldSchema, () =>
      unitFieldSchema.PreloadedArticles.map((unit: Unit) => ({
        value: unit._ClientId,
        label: unit.Title
      }))
    );
  }

  render() {
    if (!this.isMounted) {
      return null;
    }

    const fields = this.props.fields;
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    const numValueSchema = fieldSchema.RelatedContent.Fields.NumValue as NumericFieldSchema;

    const parameters = this.getParameters();
    const unitOptions = this.getUnitOptions();

    return (
      <Fragment>
        {parameters
          .slice()
          .sort(fields ? asc(p => this.fieldOrdersByTitile[p.Title]) : asc(p => p.Title))
          .map(parameter => (
            <Col
              md={12}
              key={parameter._ClientId}
              className={cn("field-editor__block bp3-form-group", {
                "bp3-intent-danger": parameter.hasVisibleErrors("BaseParameter")
              })}
            >
              <Row>
                <Col xl={2} md={3} className="field-editor__label">
                  <label
                    htmlFor={"param_" + parameter._ClientId}
                    title={parameter.BaseParameter && parameter.BaseParameter.Alias}
                    className={cn("field-editor__label-text", {
                      "field-editor__label-text--edited": parameter.isEdited(),
                      "field-editor__label-text--invalid": parameter.hasVisibleErrors()
                    })}
                  >
                    {parameter.Title}
                  </label>
                </Col>
                <Col xl={2} md={3}>
                  <InputNumber
                    id={"param_" + parameter._ClientId}
                    model={parameter}
                    name="NumValue"
                    isInteger={numValueSchema.IsInteger}
                    intent={parameter.isEdited("NumValue") ? Intent.PRIMARY : Intent.NONE}
                  />
                </Col>
                <Col xl={2} md={3}>
                  <Select
                    model={parameter}
                    name="Unit"
                    options={unitOptions}
                    className={cn({
                      "bp3-intent-primary": parameter.isEdited("Unit")
                    })}
                  />
                </Col>
              </Row>
              <Row>
                <Col xl={2} md={3} className="field-editor__label" />
                <Col md>
                  {parameter.hasVisibleErrors("BaseParameter") && (
                    <span className="bp3-form-helper-text">
                      {parameter.getVisibleErrors("BaseParameter")}
                    </span>
                  )}
                </Col>
              </Row>
            </Col>
          ))}
      </Fragment>
    );
  }
}
