import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import { Button, ButtonGroup, Icon } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { Validate } from "mst-validation-mixin";
import { DataSerializer } from "Services/DataSerializer";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
import { asc } from "Utils/Array/Sort";
import { FieldSelector } from "../AbstractFieldEditor";
import { AbstractRelationFieldTable, RelationFieldTableProps } from "./AbstractRelationFieldTable";

@consumer
@observer
export class MultiRelationFieldTable extends AbstractRelationFieldTable {
  @inject private _dataSerializer: DataSerializer;
  private _orderByField: FieldSelector;

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      orderByField = (fieldSchema as MultiRelationFieldSchema).OrderByFieldName || "Id"
    } = props;
    this._orderByField = isString(orderByField) ? article => article[orderByField] : orderByField;
  }

  @action
  clearRelation = () => {
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  removeRelation(e: any, article: ArticleObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    array.remove(article);
    model.setTouched(fieldSchema.FieldName, true);
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const list: ArticleObject[] = model[fieldSchema.FieldName];
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
            onClick={this.clearRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={[
            fieldSchema.IsRequired && required,
            fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)
          ]}
        />
        {this.renderValidation()}
        {list && (
          <div className="relation-field-table">
            {list
              .slice()
              .sort(asc(this._orderByField))
              .map(article => {
                const serverId = this._dataSerializer.getServerId(article);
                return (
                  <div key={article.Id} className="relation-field-table__row">
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
                          onClick={e => this.removeRelation(e, article)}
                        >
                          Удалить
                        </Button>
                      )}
                    </div>
                  </div>
                );
              })}
          </div>
        )}
      </Col>
    );
  }
}
