import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ArticleObject, EntityObject } from "ProductEditor/Models/EditorDataModels";
import { SingleRelationFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { RelationFieldMenu } from "ProductEditor/Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTags } from "./AbstractRelationFieldTags";

/** Отображение поля-связи в виде списка тегов */
@observer
export class SingleRelationFieldTags extends AbstractRelationFieldTags {
  protected _isHalfSize = true;

  @action
  private detachEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({ isSelected: false });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelation = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const { relationActions, validateItems } = this.props;
    const entity: EntityObject = model[fieldSchema.FieldName];
    const itemError =
      entity &&
      validateItems &&
      this._validationCache.getOrAdd(entity, () => validateItems(entity));
    return (
      <Col md className="relation-field-list__tags">
        <RelationFieldMenu onSelect={!this._readonly && this.selectRelation}>
          {relationActions && relationActions()}
        </RelationFieldMenu>
        {entity && (
          <span
            className={cn("bp3-tag bp3-minimal", {
              "bp3-intent-danger": !!itemError
            })}
            title={itemError}
          >
            {this.getTitle(entity)}
            {!this._readonly && (
              <button className="bp3-tag-remove" title="Отвязать" onClick={this.detachEntity} />
            )}
          </span>
        )}
      </Col>
    );
  }
}
