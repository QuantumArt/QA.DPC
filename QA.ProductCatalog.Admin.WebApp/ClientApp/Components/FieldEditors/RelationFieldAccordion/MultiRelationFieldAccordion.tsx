import React, { Fragment } from "react";
import { consumer } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema, SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { asc } from "Utils/Array/Sort";
import { ArticleMenu } from "Components/ArticleEditor/ArticleMenu";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { FieldSelector } from "../AbstractFieldEditor";
import {
  AbstractRelationFieldAccordion,
  RelationFieldAccordionProps
} from "./AbstractRelationFieldAccordion";
import { ArticleLink } from "Components/ArticleEditor/ArticleLink";

interface MultiRelationFieldAccordionState {
  activeId: number | null;
  touchedIds: {
    [articleId: number]: boolean;
  };
}

@consumer
@observer
export class MultiRelationFieldAccordion extends AbstractRelationFieldAccordion {
  static defaultProps = {
    filterItems: () => true
  };

  private _orderByField: FieldSelector;
  readonly state: MultiRelationFieldAccordionState = {
    activeId: null,
    touchedIds: {}
  };

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      orderByField = (fieldSchema as MultiRelationFieldSchema).OrderByFieldName ||
        ArticleObject._ServerId
    } = props;
    this._orderByField = isString(orderByField) ? article => article[orderByField] : orderByField;
  }

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const contentName = (fieldSchema as MultiRelationFieldSchema).RelatedContent.ContentName;
    const article = this._dataContext.createArticle(contentName);
    this.toggleRelation(article);
    model[fieldSchema.FieldName].push(article);
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private clearRelation = () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      activeId: null,
      touchedIds: {}
    });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation(e: any, article: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const { activeId, touchedIds } = this.state;
    delete touchedIds[article._ClientId];
    if (activeId === article._ClientId) {
      this.setState({ activeId: null, touchedIds });
    } else {
      this.setState({ touchedIds });
    }
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(article);
      model.setTouched(fieldSchema.FieldName, true);
    }
  }

  private async savePartialProduct(e: any, article: EntityObject) {
    e.stopPropagation();
    const { fieldSchema } = this.props;
    const contentSchema = (fieldSchema as MultiRelationFieldSchema).RelatedContent;
    await this._editorController.savePartialProduct(article, contentSchema);
  }

  private async refreshEntity(e: any, article: EntityObject) {
    e.stopPropagation();
    const { fieldSchema } = this.props;
    const contentSchema = (fieldSchema as MultiRelationFieldSchema).RelatedContent;
    await this._articleController.refreshEntity(article, contentSchema);
  }

  private async reloadEntity(e: any, article: EntityObject) {
    e.stopPropagation();
    const { fieldSchema } = this.props;
    const contentSchema = (fieldSchema as MultiRelationFieldSchema).RelatedContent;
    await this._articleController.reloadEntity(article, contentSchema);
  }

  private handleToggle(e: any, article: EntityObject) {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;

    this.toggleRelation(article);
  }

  private toggleRelation(article: EntityObject) {
    const { activeId, touchedIds } = this.state;
    if (activeId === article._ClientId) {
      this.setState({ activeId: null });
    } else {
      touchedIds[article._ClientId] = true;
      this.setState({ activeId: article._ClientId, touchedIds });
    }
  }

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  private reloadRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.reloadRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderControls(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <RelationFieldMenu
        onCreate={this._canEditRelation && this.createRelation}
        onSelect={this._canEditRelation && this.selectRelations}
        onClear={this._canEditRelation && !isEmpty && this.clearRelation}
        onReload={model._ServerId > 0 && this.reloadRelations}
      />
    );
  }

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const { fieldEditors, filterItems, children } = this.props;
    const { activeId, touchedIds } = this.state;
    const list: EntityObject[] = model[fieldSchema.FieldName];
    return list ? (
      <table className="relation-field-accordion" cellSpacing="0" cellPadding="0">
        <tbody>
          {list
            .filter(filterItems)
            .sort(asc(this._orderByField))
            .map(article => {
              const isOpen = article._ClientId === activeId;
              const hasServerId = article._ServerId > 0;
              return (
                <Fragment key={article._ClientId}>
                  <tr
                    className={cn("relation-field-accordion__header", {
                      "relation-field-accordion__header--open": isOpen
                    })}
                    onClick={e => this.handleToggle(e, article)}
                  >
                    <td
                      key={-1}
                      className="relation-field-accordion__expander"
                      title={isOpen ? "Свернуть" : "Развернуть"}
                    >
                      <Icon icon={isOpen ? "caret-down" : "caret-right"} title={false} />
                    </td>
                    <td key={-2} className="relation-field-accordion__cell">
                      <ArticleLink model={article} contentSchema={fieldSchema.RelatedContent} />
                    </td>
                    {this._displayFields.map((displayField, i) => (
                      <td key={i} className="relation-field-accordion__cell">
                        {displayField(article)}
                      </td>
                    ))}
                    <td key={-3} className="relation-field-accordion__controls">
                      <ArticleMenu
                        small
                        onSave={e => this.savePartialProduct(e, article)}
                        onRemove={e => this.removeRelation(e, article)}
                        onRefresh={hasServerId && (e => this.refreshEntity(e, article))}
                        onReload={hasServerId && (e => this.reloadEntity(e, article))}
                        onClone={() => {}} // TODO: clone PartialProduct
                        onPublish={() => {}} // TODO: publish PartialProduct
                      />
                    </td>
                  </tr>
                  <tr className="relation-field-accordion__main">
                    <td
                      className={cn("relation-field-accordion__body", {
                        "relation-field-accordion__body--open": isOpen
                      })}
                      colSpan={this._displayFields.length + 3}
                    >
                      {touchedIds[article._ClientId] && (
                        <EntityEditor
                          model={article}
                          contentSchema={fieldSchema.RelatedContent}
                          fieldEditors={fieldEditors}
                        >
                          {children}
                        </EntityEditor>
                      )}
                    </td>
                  </tr>
                </Fragment>
              );
            })}
        </tbody>
      </table>
    ) : null;
  }
}
