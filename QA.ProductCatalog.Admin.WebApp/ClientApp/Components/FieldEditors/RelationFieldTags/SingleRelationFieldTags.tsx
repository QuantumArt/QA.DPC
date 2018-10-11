import React from "react";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import cn from "classnames";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTags } from "./AbstractRelationFieldTags";

@consumer
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
    const entity: EntityObject = model[fieldSchema.FieldName];
    return (
      <Col md className="relation-field-list__tags">
        <RelationFieldMenu onSelect={!this._readonly && this.selectRelation} />
        {entity && (
          <span
            className={cn("bp3-tag bp3-minimal", {
              "bp3-tag-removable": !this._readonly
            })}
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
