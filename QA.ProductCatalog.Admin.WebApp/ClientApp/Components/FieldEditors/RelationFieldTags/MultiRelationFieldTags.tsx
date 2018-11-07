import React, { Fragment } from "react";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";

import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { EntityComparer } from "../AbstractFieldEditor";
import { AbstractRelationFieldTags, RelationFieldTagsProps } from "./AbstractRelationFieldTags";

@observer
export class MultiRelationFieldTags extends AbstractRelationFieldTags {
  private _entityComparer: EntityComparer;

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
            .sort(this._entityComparer)
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
