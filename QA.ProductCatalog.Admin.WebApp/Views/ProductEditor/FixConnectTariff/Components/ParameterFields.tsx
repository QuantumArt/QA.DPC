import React, { Component } from "react";
import { consumer, inject } from "react-ioc";
import { observable, runInAction, autorun, IObservableArray, IReactionDisposer } from "mobx";
import { observer } from "mobx-react";
import { Options } from "react-select";
import { WeakCache, ComputedCache } from "Utils/WeakCache";
import { asc } from "Utils/Array";
import { setEquals } from "Utils/Array";
import { DataContext } from "Services/DataContext";
import { FieldEditorProps } from "Components/ArticleEditor/ArticleEditor";
import { RelationFieldSchema, NumericFieldSchema } from "Models/EditorSchemaModels";
import {
  Tables,
  LinkParameter,
  ProductParameter,
  BaseParameter,
  Unit,
  BaseParameterModifier,
  ParameterModifier,
  Direction
} from "../TypeScriptSchema";
import { ParameterBlock } from "./ParameterBlock";

type Parameter = ProductParameter | LinkParameter;

interface ParameterField {
  Title: string;
  Unit?: string;
  BaseParam?: string;
  Direction?: string;
  BaseParamModifiers?: string[];
  Modifiers?: string[];
}

interface ParameterFieldsProps extends FieldEditorProps {
  fields?: ParameterField[];
}

const unitOptionsCache = new WeakCache();
const unitByAliasCache = new ComputedCache();
const baseParamsByAliasCache = new ComputedCache();
const directionByAliasCache = new ComputedCache();
const baseParamModifiersByAliasCache = new ComputedCache();
const paramModifiersByAliasCache = new ComputedCache();

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
    const directionByAlias = this.getDirectionsByAlias();
    const baseParamModifiersByAlias = this.getBaseParameterModifiersByAlias();
    const paramModifiersByAlias = this.getParameterModifiersByAlias();

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
          Direction: directionByAlias[field.Direction],
          BaseParameterModifiers:
            field.BaseParamModifiers &&
            field.BaseParamModifiers.map(alias => baseParamModifiersByAlias[alias]),
          Modifiers: field.Modifiers && field.Modifiers.map(alias => paramModifiersByAlias[alias])
        })
      );
    });

    this.reactions.push(
      autorun(() => {
        const parameters = this.getParameters();
        // find missing virtual parameters then add it to entity field
        const parametersToAdd = this.virtualParameters.filter(
          // if virtual BaseParameter search for parameter with same Title
          // else search for parameter with same TariffDirection and Modifiers
          virtual =>
            !parameters.some(
              virtual.BaseParameter
                ? parameter =>
                    parameter.BaseParameter === virtual.BaseParameter &&
                    parameter.Zone === virtual.Zone &&
                    parameter.Direction === virtual.Direction &&
                    setEquals(parameter.BaseParameterModifiers, virtual.BaseParameterModifiers) &&
                    setEquals(parameter.Modifiers, virtual.Modifiers)
                : parameter => parameter.Title === virtual.Title
            )
        );
        runInAction("addParameters", () => {
          if (parametersToAdd.length > 0) {
            parametersToAdd.forEach(parameter => {
              parameter.restoreBaseValues();
              parameter.setUntouched();
              // @ts-ignore
              parameter.clearErrors();
            });
            parameters.push(...parametersToAdd);
          }
          parameters.forEach(parameter => {
            // @ts-ignore
            parameter.setTouched("BaseParameter");
          });
        });
      })
    );

    this.reactions.push(
      autorun(() => {
        const parameters = this.getParameters();
        // touch parameter NumValue and Value to capture reaction dependencies
        parameters.forEach(parameter => (parameter.NumValue, parameter.Value));
        runInAction("synchronizeParameters", () => {
          parameters.forEach(parameter => {
            const isVirtual = parameter.NumValue === null && !parameter.Value;
            if (parameter._IsVirtual !== isVirtual) {
              parameter._IsVirtual = isVirtual;
            }
          });
        });
      })
    );

    runInAction("componentDidMount", () => {
      this.isMounted = true;
    });
  }

  async componentWillUnmount() {
    this.reactions.forEach(disposer => disposer());

    // remove virtual parameters from entity field
    runInAction("removeParameters", () => {
      const parameters = this.getParameters();
      if (parameters.some(parameter => parameter._IsVirtual)) {
        parameters.replace(parameters.filter(parameter => !parameter._IsVirtual));
      }
    });

    // wait until all <ParameterBlock> are unmounted
    await Promise.resolve();

    // remove virtual parameters from context table
    runInAction("deleteParameters", () => {
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

  // Direction.PreloadingMode должно быть PreloadingMode.Eager
  private getDirectionsByAlias(): { [alias: string]: Direction } {
    return directionByAliasCache.getOrAdd(this._dataContext, { keepAlive: true }, () => {
      const byAlias = {};
      for (const direction of this._dataContext.tables.Direction.values()) {
        byAlias[direction.Alias] = direction;
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

  // Modifiers.PreloadingMode должно быть PreloadingMode.Eager
  private getParameterModifiersByAlias(): { [alias: string]: ParameterModifier } {
    return paramModifiersByAliasCache.getOrAdd(this._dataContext, { keepAlive: true }, () => {
      const byAlias = {};
      for (const modifier of this._dataContext.tables.ParameterModifier.values()) {
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

    return parameters
      .slice()
      .sort(fields ? asc(p => this.fieldOrdersByTitile[p.Title]) : asc(p => p.Title))
      .map(parameter => (
        <ParameterBlock
          key={parameter._ClientId}
          parameter={parameter}
          allParameters={parameters}
          numValueSchema={numValueSchema}
          unitOptions={unitOptions}
        />
      ));
  }
}
