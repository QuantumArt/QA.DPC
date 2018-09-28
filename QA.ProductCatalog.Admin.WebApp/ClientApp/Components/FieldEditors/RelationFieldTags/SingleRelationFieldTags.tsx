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
  private removeRelation = (e: any) => {
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
    const article: EntityObject = model[fieldSchema.FieldName];
    return (
      <Col md className="relation-field-list__tags">
        <RelationFieldMenu onSelect={!this._readonly && this.selectRelation} />
        {article && (
          <span
            className={cn("pt-tag pt-minimal", {
              "pt-tag-removable": !this._readonly
            })}
          >
            {this.getTitle(article)}
            {!this._readonly && (
              <button className="pt-tag-remove" title="Удалить" onClick={this.removeRelation} />
            )}
          </span>
        )}
      </Col>
    );
  }
}
