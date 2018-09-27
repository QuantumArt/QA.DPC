import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTable } from "./AbstractRelationFieldTable";
import { ArticleLink } from "Components/ArticleEditor/ArticleLink";

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

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const article: EntityObject = model[fieldSchema.FieldName];
    return (
      <Col md>
        <RelationFieldMenu
          onSelect={!this._readonly && this.selectRelation}
          onClear={!this._readonly && !!article && this.removeRelation}
        />
        {this.renderValidation(model, fieldSchema)}
        {article && (
          <div className="relation-field-table">
            <div className="relation-field-table__table">
              <div className="relation-field-table__row">
                <div key={-1} className="relation-field-table__cell">
                  <ArticleLink model={article} contentSchema={fieldSchema.RelatedContent} />
                </div>
                {this._displayFields.map((displayField, i) => (
                  <div key={i} className="relation-field-table__cell">
                    {displayField(article)}
                  </div>
                ))}
                <div key={-2} className="relation-field-table__controls">
                  {!this._readonly && (
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
          </div>
        )}
      </Col>
    );
  }
}
