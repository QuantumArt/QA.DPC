import React, { Fragment } from "react";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { asc } from "Utils/Array/Sort";
import { isString } from "Utils/TypeChecks";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { FieldSelector } from "../AbstractFieldEditor";
import { AbstractRelationFieldTags, RelationFieldTagsProps } from "./AbstractRelationFieldTags";

@consumer
@observer
export class MultiRelationFieldTags extends AbstractRelationFieldTags {
  private _orderByField: FieldSelector;

  constructor(props: RelationFieldTagsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;
    const orderByField =
      props.orderByField || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    this._orderByField = isString(orderByField) ? entity => entity[orderByField] : orderByField;
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
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <Col md className="relation-field-list__tags">
        <RelationFieldMenu
          onSelect={!this._readonly && this.selectRelations}
          onClear={!this._readonly && !isEmpty && this.clearRelations}
        />
        {list &&
          list
            .slice()
            .sort(asc(this._orderByField))
            .map(entity => (
              <Fragment key={entity._ClientId}>
                {" "}
                <span className="bp3-tag bp3-minimal">
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
            ))}
      </Col>
    );
  }
}
