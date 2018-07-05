import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, ButtonGroup, Intent } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
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

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const article: ArticleObject = model[fieldSchema.FieldName];
    const serverId = article && this._dataSerializer.getServerId(article);
    return (
      <Col md>
        <ButtonGroup>
          <Button
            minimal
            small
            icon="th-derived"
            intent={Intent.PRIMARY}
            disabled={fieldSchema.IsReadOnly}
          >
            Выбрать
          </Button>
          <Button
            minimal
            small
            icon="eraser"
            intent={Intent.DANGER}
            disabled={fieldSchema.IsReadOnly}
            onClick={this.removeRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
        {this.renderValidation(model, fieldSchema)}
        {article && (
          <div className="relation-field-table">
            <div className="relation-field-table__row">
              <div key={-1} className="relation-field-table__cell">
                {serverId > 0 && `(${serverId})`}
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
                    icon="remove"
                    intent={Intent.DANGER}
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
