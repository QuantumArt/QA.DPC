import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";

import { action, IObservableArray, computed, untracked } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "ProductEditor/Models/EditorDataModels";
import { MultiRelationFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { RelationFieldMenu } from "ProductEditor/Components/FieldEditors/RelationFieldMenu";
import { EntityComparer, HighlightMode } from "../AbstractFieldEditor";
import { AbstractRelationFieldTable, RelationFieldTableProps } from "./AbstractRelationFieldTable";
import { EntityLink } from "ProductEditor/Components/ArticleEditor/EntityLink";

/** Отображение поля-связи в виде таблицы */
@observer
export class MultiRelationFieldTable extends AbstractRelationFieldTable {
  static defaultProps = {
    filterItems: () => true,
    highlightItems: () => HighlightMode.None
  };

  private _entityComparer: EntityComparer;

  @computed
  private get dataSource() {
    const { model, fieldSchema, filterItems, validateItems } = this.props;
    const array: EntityObject[] = model[fieldSchema.FieldName];
    if (!array) {
      return array;
    }
    if (!validateItems) {
      return array.filter(filterItems).sort(this._entityComparer);
    }
    const head: EntityObject[] = [];
    const tail: EntityObject[] = [];

    array.filter(filterItems).forEach(entity => {
      const itemError = untracked(() =>
        this._validationCache.getOrAdd(entity, () => validateItems(entity))
      );
      if (itemError) {
        head.push(entity);
      } else {
        tail.push(entity);
      }
    });

    head.sort(this._entityComparer);
    tail.sort(this._entityComparer);

    return head.concat(tail);
  }

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const { sortItems, sortItemsBy } = props;
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;
    this._entityComparer = this.makeEntityComparer(sortItems || sortItemsBy, fieldSchema);
  }

  @action
  private detachEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(entity);
      model.setTouched(fieldSchema.FieldName, true);
    }
  }

  @action
  private clearRelations = () => {
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName].replace([]);
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const { highlightItems, relationActions } = this.props;
    const dataSource = this.dataSource;
    const isEmpty = !dataSource || dataSource.length === 0;
    return (
      <Col md>
        <RelationFieldMenu
          onSelect={!this._readonly && this.selectRelations}
          onClear={!this._readonly && !isEmpty && this.clearRelations}
        >
          {relationActions && relationActions()}
        </RelationFieldMenu>
        {this.renderValidation(model, fieldSchema)}
        {dataSource && (
          <div className="relation-field-table">
            <div className="relation-field-table__table">
              {dataSource.map(entity => {
                const highlightMode = highlightItems(entity);
                const highlight = highlightMode === HighlightMode.Highlight;
                const shade = highlightMode === HighlightMode.Shade;
                const itemError = this._validationCache.get(entity);
                return (
                  <div
                    key={entity._ClientId}
                    className={cn("relation-field-table__row", {
                      "relation-field-table__row--highlight": highlight,
                      "relation-field-table__row--shade": shade,
                      "relation-field-table__row--invalid": !!itemError
                    })}
                    title={itemError}
                  >
                    <div key={-1} className="relation-field-table__cell">
                      <EntityLink model={entity} contentSchema={fieldSchema.RelatedContent} />
                    </div>
                    {this._displayFields.map((displayField, i) => (
                      <div key={i} className="relation-field-table__cell">
                        {displayField(entity)}
                      </div>
                    ))}
                    <div key={-2} className="relation-field-table__controls">
                      {!this._readonly && (
                        <Button
                          minimal
                          small
                          rightIcon="remove"
                          intent={Intent.DANGER}
                          title="Удалить связь с текущей статьей"
                          onClick={e => this.detachEntity(e, entity)}
                        >
                          Отвязать
                        </Button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </Col>
    );
  }
}
