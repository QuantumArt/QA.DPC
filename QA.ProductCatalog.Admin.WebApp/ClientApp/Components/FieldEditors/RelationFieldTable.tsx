import React, { Component } from "react";
import { Col, Row } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Icon } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import {
  MultiRelationFieldSchema,
  SingleRelationFieldSchema,
  isSingleRelationField,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { Validate } from "Models/ValidatableMixin";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
import { asc } from "Utils/Array/Sort";
import { AbstractFieldEditor, FieldEditorProps } from "./AbstractFieldEditor";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

type FieldSelector = (article: ArticleObject) => any;

interface RelationFieldTableProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
}

export class RelationFieldTable extends Component<RelationFieldTableProps> {
  FieldTable = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldTable
    : MultiRelationFieldTable;

  render() {
    return <this.FieldTable {...this.props} />;
  }
}

abstract class AbstractRelationFieldTable extends AbstractFieldEditor<RelationFieldTableProps> {
  _displayFields: FieldSelector[];

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayFields = (fieldSchema as RelationFieldSchema).DisplayFieldNames || []
    } = this.props;
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
  }

  render() {
    const { model, fieldSchema, validate } = this.props;
    return (
      <Col
        md={12}
        className={cn("field-editor__block pt-form-group", {
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={2} md={3} className="field-editor__label field-editor__label--small">
            <label htmlFor={this.id} title={fieldSchema.FieldDescription}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && <span className="field-editor__label-required"> *</span>}
            </label>
          </Col>
          {this.renderField(model, fieldSchema)}
          {validate && <Validate model={model} name={fieldSchema.FieldName} rules={validate} />}
        </Row>
      </Col>
    );
  }
}

@observer
class SingleRelationFieldTable extends AbstractRelationFieldTable {
  @action
  removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const article: ArticleObject = model[fieldSchema.FieldName];
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
        {article && (
          <div className="relation-field-table">
            <div className="relation-field-table__row">
              <div className="relation-field-table__cell" key={-1}>
                ({article.Id})
              </div>
              {this._displayFields.map((displayField, i) => (
                <div className="relation-field-table__cell" key={i}>
                  {displayField(article)}
                </div>
              ))}
              {!fieldSchema.IsReadOnly && (
                <div key={-2} className="relation-field-table__controls">
                  <Button
                    small
                    icon={<Icon icon="small-cross" title={false} />}
                    disabled={fieldSchema.IsReadOnly}
                    onClick={this.removeRelation}
                  />
                </div>
              )}
            </div>
          </div>
        )}
        {this.renderErrors(model, fieldSchema)}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={fieldSchema.IsRequired && required}
        />
      </Col>
    );
  }
}

@observer
class MultiRelationFieldTable extends AbstractRelationFieldTable {
  _orderByField: FieldSelector;

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
        {list && (
          <div className="relation-field-table">
            {list
              .slice()
              .sort(asc(this._orderByField))
              .map(article => (
                <div className="relation-field-table__row" key={article.Id}>
                  <div className="relation-field-table__cell" key={-1}>
                    ({article.Id})
                  </div>
                  {this._displayFields.map((displayField, i) => (
                    <div className="relation-field-table__cell" key={i}>
                      {displayField(article)}
                    </div>
                  ))}
                  {!fieldSchema.IsReadOnly && (
                    <div key={-2} className="relation-field-table__controls">
                      <Button
                        small
                        icon={<Icon icon="small-cross" title={false} />}
                        disabled={fieldSchema.IsReadOnly}
                        onClick={e => this.removeRelation(e, article)}
                      />
                    </div>
                  )}
                </div>
              ))}
          </div>
        )}
        {this.renderErrors(model, fieldSchema)}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={[
            fieldSchema.IsRequired && required,
            fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)
          ]}
        />
      </Col>
    );
  }
}
