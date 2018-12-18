import React, { Component } from "react";
import { inject } from "react-ioc";
import {
  observable,
  runInAction,
  autorun,
  IObservableArray,
  IReactionDisposer,
  action
} from "mobx";
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
  optionalBaseParamModifiers?: string[];
  showBaseParamModifiers?: boolean;
}

const unitOptionsCache = new WeakCache();
const unitByAliasCache = new ComputedCache();
const baseParamsByAliasCache = new ComputedCache();
const directionByAliasCache = new ComputedCache();
const baseParamModifiersByAliasCache = new ComputedCache();
const paramModifiersByAliasCache = new ComputedCache();

@observer
export class ParameterFields extends Component<ParameterFieldsProps> {
  static defaultProps = {
    optionalBaseParamModifiers: ["LowerBound"]
  };

  @inject private _dataContext: DataContext<Tables>;
  @observable private isMounted = false;
  private reactions: IReactionDisposer[] = [];
  private fieldOrdersByTitile: { [title: string]: number } = {};
  private virtualParameters: Parameter[];

  componentDidMount() {
    this.createVirtualParameters();

    this.reactions.push(
      autorun(() => {
        const parameters = this.getParameters();
        // find missing virtual parameters then add it to entity field
        const parametersToAdd = this.virtualParameters.filter(
          virtual =>
            virtual.BaseParameter
              ? !parameters.some(parameter => this.hasSameTariffDerection(parameter, virtual))
              : !parameters.some(parameter => parameter.Title === virtual.Title)
        );
        runInAction("addVirtualParameters", () => {
          if (parametersToAdd.length > 0) {
            this.resetParameters(parametersToAdd);
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

    // wait until all <ParameterBlock> are unmounted
    await Promise.resolve();

    this.deleteVirtualParameters();
  }

  @action
  private createVirtualParameters() {
    const { fields = [], optionalBaseParamModifiers } = this.props;
    if (DEBUG) {
      const invalidField = fields.find(
        field =>
          field.BaseParamModifiers &&
          field.BaseParamModifiers.some(modifier => optionalBaseParamModifiers.includes(modifier))
      );
      if (invalidField) {
        throw new Error(
          `Field "${invalidField.Title}" is invalid: BaseParameterModifiers ${JSON.stringify(
            invalidField.BaseParamModifiers
          )} intersects with optionalBaseParamModifiers ${JSON.stringify(
            optionalBaseParamModifiers
          )}`
        );
      }
    }

    const contentName = this.getContentName();
    const unitsByAlias = this.getUnitsByAlias();
    const baseParamsByAlias = this.getBaseParamsByAlias();
    const directionsByAlias = this.getDirectionsByAlias();
    const baseParamModifiersByAlias = this.getBaseParameterModifiersByAlias();
    const paramModifiersByAlias = this.getParameterModifiersByAlias();

    fields.forEach((field, i) => {
      this.fieldOrdersByTitile[field.Title] = i;
    });

    // create virtual paramters in context table
    this.virtualParameters = fields.map(field =>
      this._dataContext.createEntity(contentName, {
        _IsVirtual: true,
        Title: field.Title,
        Unit: unitsByAlias[field.Unit],
        BaseParameter: baseParamsByAlias[field.BaseParam],
        Direction: directionsByAlias[field.Direction],
        BaseParameterModifiers:
          field.BaseParamModifiers &&
          field.BaseParamModifiers.map(alias => baseParamModifiersByAlias[alias]),
        Modifiers: field.Modifiers && field.Modifiers.map(alias => paramModifiersByAlias[alias])
      })
    );
  }

  @action
  private deleteVirtualParameters() {
    const parameters = this.getParameters();

    // remove virtual parameters from context table
    this.virtualParameters.forEach(virtual => {
      if (!parameters.includes(virtual)) {
        this._dataContext.deleteEntity(virtual);
      }
    });
  }

  private hasSameTariffDerection(parameter: Parameter, virtual: Parameter) {
    const { optionalBaseParamModifiers } = this.props;
    return (
      parameter.BaseParameter === virtual.BaseParameter &&
      parameter.Zone === virtual.Zone &&
      parameter.Direction === virtual.Direction &&
      setEquals(
        parameter.BaseParameterModifiers.filter(
          alias => !optionalBaseParamModifiers.includes(alias.Alias)
        ),
        virtual.BaseParameterModifiers.filter(
          alias => !optionalBaseParamModifiers.includes(alias.Alias)
        )
      ) &&
      setEquals(parameter.Modifiers, virtual.Modifiers)
    );
  }

  @action
  private resetParameters(parameters: Parameter[]) {
    const { optionalBaseParamModifiers } = this.props;
    parameters.forEach(parameter => {
      parameter.NumValue = null;
      parameter.Value = null;
      // remove optional BaseParameterModifiers
      parameter.BaseParameterModifiers.replace(
        parameter.BaseParameterModifiers.filter(
          alias => !optionalBaseParamModifiers.includes(alias.Alias)
        )
      );
      parameter.setUnchanged();
      parameter.setUntouched();
      // @ts-ignore
      parameter.clearErrors();
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

  private getOptionalBaseParameterModifiers(): BaseParameterModifier[] {
    const { optionalBaseParamModifiers, showBaseParamModifiers } = this.props;
    if (!showBaseParamModifiers) {
      return null;
    }
    const baseParamModifiersByAlias = this.getBaseParameterModifiersByAlias();
    return optionalBaseParamModifiers.map(alias => baseParamModifiersByAlias[alias]);
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
    const optionalBaseParamModifiers = this.getOptionalBaseParameterModifiers();

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
          baseParamModifiers={optionalBaseParamModifiers}
        />
      ));
  }
}
