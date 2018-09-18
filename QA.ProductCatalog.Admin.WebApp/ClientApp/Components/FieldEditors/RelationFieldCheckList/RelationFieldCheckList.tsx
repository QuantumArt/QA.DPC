import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { Checkbox, Radio } from "@blueprintjs/core";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import {
  RelationFieldSchema,
  isMultiRelationField,
  PreloadingMode,
  PreloadingState
} from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isArray, isObject, isString } from "Utils/TypeChecks";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";
import "./RelationFieldCheckList.scss";

export interface RelationFieldCheckListProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
}

interface Option {
  article: EntityObject;
  fields: string[];
}

const optionsCache = new WeakMap<RelationFieldSchema, Option[]>();

@consumer
@observer
export class RelationFieldCheckList extends AbstractRelationFieldEditor<
  RelationFieldCheckListProps
> {
  private readonly _getOption: (article: EntityObject) => Option;
  private readonly _multiple: boolean;
  private _baseRelation: EntityObject | EntityObject[];
  private _cachedOptions: Option[];
  private _sortedOptions: Option[];

  constructor(props: RelationFieldCheckListProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayFields = props.displayFields || fieldSchema.DisplayFieldNames || [];
    const fieldSelectors = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
    this._getOption = article => ({
      article,
      fields: fieldSelectors.map(field => field(article))
    });
    this._multiple = isMultiRelationField(props.fieldSchema);
  }

  private getCachedOptions(): Option[] {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;

    let options = optionsCache.get(fieldSchema);
    if (options) {
      return options;
    }

    if (
      fieldSchema.PreloadingMode === PreloadingMode.Lazy &&
      fieldSchema.PreloadingState !== PreloadingState.Done
    ) {
      if (fieldSchema.PreloadingState === PreloadingState.NotStarted) {
        this._relationController.preloadRelationArticles(fieldSchema);
      }

      const relation = this.props.model[fieldSchema.FieldName];
      if (isArray(relation)) {
        return relation.map(this._getOption);
      }
      if (isObject(relation)) {
        return [this._getOption(relation)];
      }
      return [];
    }

    options = fieldSchema.PreloadedArticles.map(this._getOption);
    optionsCache.set(fieldSchema, options);

    return options;
  }

  private getSortedOptions(model: ArticleObject, fieldSchema: SingleRelationFieldSchema): Option[] {
    const cachedOptions = this.getCachedOptions();
    const baseRelation = model.getBaseValue(fieldSchema.FieldName) as EntityObject | EntityObject[];

    if (this._baseRelation !== baseRelation || this._cachedOptions !== cachedOptions) {
      // очищаем кеш внутри компонента
      this._baseRelation = baseRelation;
      this._cachedOptions = cachedOptions;
      this._sortedOptions = [];

      if (isArray(baseRelation)) {
        const notSelectedOptons: Option[] = [];
        cachedOptions.forEach(option => {
          if (baseRelation.includes(option.article)) {
            this._sortedOptions.push(option);
          } else {
            notSelectedOptons.push(option);
          }
        });
        this._sortedOptions.push(...notSelectedOptons);
      } else if (isObject(baseRelation)) {
        cachedOptions.forEach(option => {
          if (baseRelation === option.article) {
            this._sortedOptions.unshift(option);
          } else {
            this._sortedOptions.push(option);
          }
        });
      }
    }

    return this._sortedOptions;
  }

  @action
  toggleRelation = (article: EntityObject) => {
    const { model, fieldSchema } = this.props;
    if (this._multiple) {
      const relation: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
      if (relation.includes(article)) {
        relation.remove(article);
      } else {
        relation.push(article);
      }
    } else if (model[fieldSchema.FieldName] !== article) {
      model[fieldSchema.FieldName] = article;
    }
    model.setTouched(fieldSchema.FieldName, true);
  };

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const sortedOptions = this.getSortedOptions(model, fieldSchema);
    const relation = model[fieldSchema.FieldName] as EntityObject | EntityObject[];

    return (
      <Col xl md={6}>
        <div
          className={cn("relation-field-check-list", {
            "relation-field-check-list--scroll": sortedOptions.length > 7
          })}
        >
          <table>
            <tbody>
              {sortedOptions.map(({ article, fields }) => (
                <tr key={article._ClientId}>
                  <td key={-1}>
                    {this._multiple ? (
                      <Checkbox
                        checked={relation.includes(article)}
                        onChange={() => this.toggleRelation(article)}
                      />
                    ) : (
                      <Radio
                        checked={article === relation}
                        onChange={() => this.toggleRelation(article)}
                      />
                    )}
                  </td>
                  {fields.map((field, i) => (
                    <td
                      key={i}
                      className="relation-field-check-list__cell"
                      onClick={() => this.toggleRelation(article)}
                    >
                      {field}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Col>
    );
  }
}
