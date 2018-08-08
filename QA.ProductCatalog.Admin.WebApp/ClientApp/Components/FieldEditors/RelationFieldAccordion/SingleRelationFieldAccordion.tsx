import React from "react";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { ArticleMenu } from "Components/ArticleEditor/ArticleMenu";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldAccordion } from "./AbstractRelationFieldAccordion";

@consumer
@observer
export class SingleRelationFieldAccordion extends AbstractRelationFieldAccordion {
  readonly state = {
    isOpen: false,
    isTouched: false
  };

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const contentName = (fieldSchema as SingleRelationFieldSchema).RelatedContent.ContentName;
    const article = this._dataContext.createArticle(contentName);
    this.setState({
      isOpen: true,
      isTouched: true
    });
    model[fieldSchema.FieldName] = article;
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private saveMinimalProduct = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const article: EntityObject = model[fieldSchema.FieldName];
    if (article) {
      await this._editorController.saveMinimalProduct(article, contentSchema);
    }
  };

  @action
  private savePartialProduct = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const article: EntityObject = model[fieldSchema.FieldName];
    if (article) {
      await this._editorController.savePartialProduct(article, contentSchema);
    }
  };

  @action
  private refreshEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const article: EntityObject = model[fieldSchema.FieldName];
    if (article) {
      await this._articleController.refreshEntity(article, contentSchema);
    }
  };

  @action
  private reloadEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const article: EntityObject = model[fieldSchema.FieldName];
    if (article) {
      await this._articleController.reloadEntity(article, contentSchema);
    }
  };

  private toggleRelation = (e: any) => {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;

    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  private selectRelation = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  private reloadRelation = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.reloadRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  renderControls(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const article: EntityObject = model[fieldSchema.FieldName];
    return (
      <RelationFieldMenu
        onCreate={!article && this.createRelation}
        onSelect={this.selectRelation}
        onClear={!!article && this.removeRelation}
        onReload={model._ServerId > 0 && this.reloadRelation}
      />
    );
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const { fieldEditors, children } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: EntityObject = model[fieldSchema.FieldName];
    const hasServerId = article._ServerId > 0;
    const showSaveButton = this.showSaveButton(article);
    return article ? (
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
              {hasServerId && `(${article._ServerId})`}
            </td>
            {this._displayFields.map((displayField, i) => (
              <td key={i} className="relation-field-accordion__cell">
                {displayField(article)}
              </td>
            ))}
            <td key={-3} className="relation-field-accordion__controls">
              {!fieldSchema.IsReadOnly && (
                <ArticleMenu
                  small
                  onSave={showSaveButton && this.saveMinimalProduct}
                  onSaveAll={showSaveButton && this.savePartialProduct}
                  onRemove={this.removeRelation}
                  onRefresh={hasServerId && this.refreshEntity}
                  onReload={hasServerId && this.reloadEntity}
                  onClone={() => {}} // TODO: clone PartialProduct
                  onPublish={() => {}} // TODO: publish PartialProduct
                />
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
        </tbody>
      </table>
    ) : null;
  }
}
