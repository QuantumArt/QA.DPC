import React, { Component, ReactNode, Fragment } from "react";
import { Col, Row } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Icon } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import {
  MultiRelationFieldSchema,
  SingleRelationFieldSchema,
  isSingleRelationField,
  RelationFieldSchema,
  FieldSchema
} from "Models/EditorSchemaModels";
import { RelationSelection } from "Models/RelationSelection";
import { DataSerializer } from "Services/DataSerializer";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
import { asc } from "Utils/Array/Sort";
import { RenderArticle, FieldsConfig, ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { AbstractFieldEditor, FieldEditorProps } from "./AbstractFieldEditor";
import "./RelationFieldAccordion.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

type FieldSelector = (article: ArticleObject) => any;

interface RelationFieldAccordionProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
  save?: boolean;
  saveRelations?: RelationSelection;
  fields?: FieldsConfig;
  children?: RenderArticle | ReactNode;
}

export class RelationFieldAccordion extends Component<RelationFieldAccordionProps> {
  FieldAccordion = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldAccordion
    : MultiRelationFieldAccordion;

  render() {
    return <this.FieldAccordion {...this.props} />;
  }
}

abstract class AbstractRelationFieldAccordion extends AbstractFieldEditor<
  RelationFieldAccordionProps
> {
  protected _displayFields: FieldSelector[];

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayFields = (fieldSchema as RelationFieldSchema).DisplayFieldNames || []
    } = this.props;
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
  }

  abstract renderControls(fieldSchema: FieldSchema);

  render() {
    const { model, fieldSchema } = this.props;
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
          <Col md>{this.renderControls(fieldSchema)}</Col>
        </Row>
        <Row>
          <Col md xlOffset={2} mdOffset={3}>
            {this.renderValidation()}
          </Col>
        </Row>
        <Row>
          <Col md>{this.renderField(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}

@consumer
@observer
class SingleRelationFieldAccordion extends AbstractRelationFieldAccordion {
  @inject private _dataSerializer: DataSerializer;
  state = {
    isOpen: false,
    isTouched: false
  };

  @action
  removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  toggleRelation = () => {
    const { isOpen, isTouched } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: isTouched || !isOpen
    });
  };

  renderControls(fieldSchema: SingleRelationFieldSchema) {
    return (
      <ButtonGroup>
        <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
          Выбрать
        </Button>
        <Button small icon="eraser" disabled={fieldSchema.IsReadOnly} onClick={this.removeRelation}>
          Очистить
        </Button>
      </ButtonGroup>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { save, fields, children } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: ArticleObject = model[fieldSchema.FieldName];
    const serverId = article && this._dataSerializer.getServerId(article);
    return (
      <>
        {article && (
          <table className="relation-field-accordion" cellSpacing="0" cellPadding="0">
            <tbody>
              <tr
                className={cn("relation-field-accordion__header", {
                  "relation-field-accordion__header--open": isOpen
                })}
                onClick={this.toggleRelation}
              >
                <td
                  key={-1}
                  className="relation-field-accordion__expander"
                  title={isOpen ? "Свернуть" : "Развернуть"}
                >
                  <Icon icon={isOpen ? "caret-down" : "caret-right"} title={false} />
                </td>
                <td key={-2} className="relation-field-accordion__cell">
                  {serverId > 0 && `(${serverId})`}
                </td>
                {this._displayFields.map((displayField, i) => (
                  <td key={i} className="relation-field-accordion__cell">
                    {displayField(article)}
                  </td>
                ))}
                <td key={-3} className="relation-field-accordion__controls">
                  {!fieldSchema.IsReadOnly && (
                    <ButtonGroup>
                      {save && (
                        <Button small icon="floppy-disk">
                          Сохранить
                        </Button>
                      )}
                      <Button
                        small
                        icon={<Icon icon="small-cross" title={false} />}
                        disabled={fieldSchema.IsReadOnly}
                        onClick={this.removeRelation}
                      />
                    </ButtonGroup>
                  )}
                </td>
              </tr>
              <tr className="relation-field-accordion__main">
                <td
                  className={cn("relation-field-accordion__body", {
                    "relation-field-accordion__body--open": isOpen
                  })}
                  colSpan={this._displayFields.length + 3}
                >
                  {isTouched && (
                    <Col md>
                      <ArticleEditor
                        model={article}
                        contentSchema={fieldSchema.Content}
                        fields={fields}
                      >
                        {children}
                      </ArticleEditor>
                    </Col>
                  )}
                </td>
              </tr>
            </tbody>
          </table>
        )}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={fieldSchema.IsRequired && required}
        />
      </>
    );
  }
}

@consumer
@observer
class MultiRelationFieldAccordion extends AbstractRelationFieldAccordion {
  @inject private _dataSerializer: DataSerializer;
  private _orderByField: FieldSelector;
  state = {
    activeId: null,
    touchedIds: {}
  };

  constructor(props: RelationFieldAccordionProps, context?: any) {
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
    this.setState({
      activeId: null,
      touchedIds: {}
    });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  removeRelation(e: any, article: ArticleObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const { activeId, touchedIds } = this.state;
    delete touchedIds[article.Id];
    if (activeId === article.Id) {
      this.setState({ activeId: null, touchedIds });
    } else {
      this.setState({ touchedIds });
    }
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    array.remove(article);
    model.setTouched(fieldSchema.FieldName, true);
  }

  toggleRelation(_e: any, article: ArticleObject) {
    const { activeId, touchedIds } = this.state;
    if (activeId === article.Id) {
      this.setState({ activeId: null });
    } else {
      touchedIds[article.Id] = true;
      this.setState({ activeId: article.Id, touchedIds });
    }
  }

  renderControls(fieldSchema: SingleRelationFieldSchema) {
    return (
      <ButtonGroup>
        <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
          Выбрать
        </Button>
        <Button small icon="eraser" disabled={fieldSchema.IsReadOnly} onClick={this.clearRelation}>
          Очистить
        </Button>
      </ButtonGroup>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const { save, fields, children } = this.props;
    const { activeId, touchedIds } = this.state;
    const list: ArticleObject[] = model[fieldSchema.FieldName];
    return (
      <>
        {list && (
          <table className="relation-field-accordion" cellSpacing="0" cellPadding="0">
            <tbody>
              {list
                .slice()
                .sort(asc(this._orderByField))
                .map(article => {
                  const serverId = this._dataSerializer.getServerId(article);
                  const isOpen = article.Id === activeId;
                  return (
                    <Fragment key={article.Id}>
                      <tr
                        className={cn("relation-field-accordion__header", {
                          "relation-field-accordion__header--open": isOpen
                        })}
                        onClick={e => this.toggleRelation(e, article)}
                      >
                        <td
                          key={-1}
                          className="relation-field-accordion__expander"
                          title={isOpen ? "Свернуть" : "Развернуть"}
                        >
                          <Icon icon={isOpen ? "caret-down" : "caret-right"} title={false} />
                        </td>
                        <td key={-2} className="relation-field-accordion__cell">
                          {serverId > 0 && `(${serverId})`}
                        </td>
                        {this._displayFields.map((displayField, i) => (
                          <td key={i} className="relation-field-accordion__cell">
                            {displayField(article)}
                          </td>
                        ))}
                        <td key={-3} className="relation-field-accordion__controls">
                          {!fieldSchema.IsReadOnly && (
                            <ButtonGroup>
                              {save && (
                                <Button small icon="floppy-disk">
                                  Сохранить
                                </Button>
                              )}
                              <Button
                                small
                                icon={<Icon icon="small-cross" title={false} />}
                                disabled={fieldSchema.IsReadOnly}
                                onClick={e => this.removeRelation(e, article)}
                              />
                            </ButtonGroup>
                          )}
                        </td>
                      </tr>
                      <tr className="relation-field-accordion__main">
                        <td
                          className={cn("relation-field-accordion__body", {
                            "relation-field-accordion__body--open": isOpen
                          })}
                          colSpan={this._displayFields.length + 3}
                        >
                          {touchedIds[article.Id] && (
                            <Col md>
                              <ArticleEditor
                                model={article}
                                contentSchema={fieldSchema.Content}
                                fields={fields}
                              >
                                {children}
                              </ArticleEditor>
                            </Col>
                          )}
                        </td>
                      </tr>
                    </Fragment>
                  );
                })}
            </tbody>
          </table>
        )}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={[
            fieldSchema.IsRequired && required,
            fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)
          ]}
        />
      </>
    );
  }
}
