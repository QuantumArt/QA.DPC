import React, { Fragment } from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray, computed } from "mobx";
import { observer } from "mobx-react";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";

import { EntityComparer } from "../AbstractFieldEditor";
import { AbstractRelationFieldTags, RelationFieldTagsProps } from "./AbstractRelationFieldTags";

@observer
export class MultiRelationFieldTags extends AbstractRelationFieldTags {
  static defaultProps = {
    filterItems: () => true
  };

  private _entityComparer: EntityComparer;

  @computed
  private get dataSource() {
    const { model, fieldSchema, filterItems, validateItem } = this.props;
    const array: EntityObject[] = model[fieldSchema.FieldName];
    if (!array) {
      return array;
    }
    if (!validateItem) {
      return array.filter(filterItems).sort(this._entityComparer);
    }
    const head: EntityObject[] = [];
    const tail: EntityObject[] = [];

    array.filter(filterItems).forEach(entity => {
      const error = this._validationCache.getOrAdd(entity, () => validateItem(entity));
      if (error) {
        head.push(entity);
      } else {
        tail.push(entity);
      }
    });

    head.sort(this._entityComparer);
    tail.sort(this._entityComparer);

    return head.concat(tail);
  }

  constructor(props: RelationFieldTagsProps, context?: any) {
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
    this.setState({ selectedIds: {} });
    model[fieldSchema.FieldName].replace([]);
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderField(_model: ArticleObject, _fieldSchema: MultiRelationFieldSchema) {
    const { relationActions } = this.props;
    const dataSource = this.dataSource;
    const isEmpty = !dataSource || dataSource.length === 0;
    return (
      <Col md className="relation-field-list__tags">
        <RelationFieldMenu
          onSelect={!this._readonly && this.selectRelations}
          onClear={!this._readonly && !isEmpty && this.clearRelations}
        >
          {relationActions && relationActions()}
        </RelationFieldMenu>
        {dataSource &&
          dataSource.map(entity => {
            const error = this._validationCache.get(entity);
            return (
              <Fragment key={entity._ClientId}>
                {" "}
                <span
                  className={cn("bp3-tag bp3-minimal", {
                    "bp3-intent-danger": !!error
                  })}
                  title={error}
                >
                  {this.getTitle(entity)}
                  {!this._readonly && (
                    <button
                      className="bp3-tag-remove"
                      title="Отвязать"
                      onClick={e => this.detachEntity(e, entity)}
                    />
                  )}
                </span>
              </Fragment>
            );
          })}
      </Col>
    );
  }
}
