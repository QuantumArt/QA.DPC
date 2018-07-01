import React, { Fragment } from "react";
import { Col } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Icon } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema, SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { DataSerializer } from "Services/DataSerializer";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
import { asc } from "Utils/Array/Sort";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { FieldSelector } from "../AbstractFieldEditor";
import {
  AbstractRelationFieldAccordion,
  RelationFieldAccordionProps
} from "./AbstractRelationFieldAccordion";

interface MultiRelationFieldAccordionState {
  activeId: number | null;
  touchedIds: {
    [articleId: number]: boolean;
  };
}

@consumer
@observer
export class MultiRelationFieldAccordion extends AbstractRelationFieldAccordion {
  @inject private _dataSerializer: DataSerializer;
  private _orderByField: FieldSelector;
  state: MultiRelationFieldAccordionState = {
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
