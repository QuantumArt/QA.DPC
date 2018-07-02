import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, ButtonGroup, Icon } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { Validate } from "mst-validation-mixin";
import { DataSerializer } from "Services/DataSerializer";
import { required } from "Utils/Validators";
import { AbstractRelationFieldTable } from "./AbstractRelationFieldTable";

@consumer
@observer
export class SingleRelationFieldTable extends AbstractRelationFieldTable {
  @inject private _dataSerializer: DataSerializer;

  @action
  removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const article: ArticleObject = model[fieldSchema.FieldName];
    const serverId = article && this._dataSerializer.getServerId(article);
    return (
      <Col xl={10} md={9}>
        <ButtonGroup>
          <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
            Выбрать
          </Button>
          <Button
            small
            icon="eraser"
            disabled={fieldSchema.IsReadOnly}
            onClick={this.removeRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={fieldSchema.IsRequired && required}
        />
        {this.renderValidation()}
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
                    small
                    icon={<Icon icon="remove" title={false} />}
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
