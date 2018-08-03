import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTable } from "./AbstractRelationFieldTable";

@consumer
@observer
export class SingleRelationFieldTable extends AbstractRelationFieldTable {
  @action
  private removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelation = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const article: ArticleObject = model[fieldSchema.FieldName];
    return (
      <Col md>
        <RelationFieldMenu
          onSelect={this.selectRelation}
          onClear={!!article && this.removeRelation}
        />
        {this.renderValidation(model, fieldSchema)}
        {article && (
          <div className="relation-field-table">
            <div className="relation-field-table__row">
              <div key={-1} className="relation-field-table__cell">
                {article._ServerId > 0 && `(${article._ServerId})`}
              </div>
              {this._displayFields.map((displayField, i) => (
                <div key={i} className="relation-field-table__cell">
                  {displayField(article)}
                </div>
              ))}
              <div key={-2} className="relation-field-table__controls">
                {!fieldSchema.IsReadOnly && (
                  <Button
                    minimal
                    small
                    rightIcon="remove"
                    intent={Intent.DANGER}
                    title="Удалить связь"
                    onClick={this.removeRelation}
                  >
                    Удалить
                  </Button>
                )}
              </div>
            </div>
          </div>
        )}
      </Col>
    );
  }
}
