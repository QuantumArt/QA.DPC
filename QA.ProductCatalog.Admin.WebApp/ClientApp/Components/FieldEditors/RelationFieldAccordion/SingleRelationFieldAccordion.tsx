import React from "react";
import { consumer } from "react-ioc";
import { action, untracked } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { EntityMenu } from "Components/ArticleEditor/EntityMenu";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldAccordion } from "./AbstractRelationFieldAccordion";
import { EntityLink } from "Components/ArticleEditor/EntityLink";

@consumer
@observer
export class SingleRelationFieldAccordion extends AbstractRelationFieldAccordion {
  readonly state = {
    isOpen: false,
    isTouched: false
  };

  private clonePrototype = async () => {
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    await this._cloneController.cloneProductPrototype(model, relationFieldSchema);
    this.setState({
      isOpen: true,
      isTouched: true
    });
  };

  @action
  private createEntity = () => {
    const { model, fieldSchema } = this.props;
    const contentName = (fieldSchema as SingleRelationFieldSchema).RelatedContent.ContentName;
    const entity = this._dataContext.createEntity(contentName);
    this.setState({
      isOpen: true,
      isTouched: true
    });
    model[fieldSchema.FieldName] = entity;
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private detachEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
    this.setState({
      isOpen: false,
      isTouched: false
    });
  };

  private removeEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    if (entity) {
      await this._entityController.removeRelatedEntity(model, relationFieldSchema, entity);
    }
    this.setState({
      isOpen: false,
      isTouched: false
    });
  };

  private cloneEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    if (entity) {
      await this._cloneController.cloneRelatedEntity(model, relationFieldSchema, entity);
    }
  };

  @action
  private saveEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const entity: EntityObject = model[fieldSchema.FieldName];
    if (entity) {
      await this._productController.savePartialProduct(entity, contentSchema);
    }
  };

  @action
  private refreshEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const entity: EntityObject = model[fieldSchema.FieldName];
    if (entity) {
      await this._entityController.refreshEntity(entity, contentSchema);
    }
  };

  @action
  private reloadEntity = async (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const entity: EntityObject = model[fieldSchema.FieldName];
    if (entity) {
      await this._entityController.reloadEntity(entity, contentSchema);
    }
  };

  private publishEntity = (e: any) => {
    e.stopPropagation();
    alert("TODO: публикация");
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
    const {
      canCreateEntity,
      canSelectRelation,
      canClearRelation,
      canReloadRelation,
      canClonePrototype
    } = this.props;
    const entity: EntityObject = model[fieldSchema.FieldName];
    return (
      <RelationFieldMenu
        onCreate={canCreateEntity && !this._readonly && !entity && this.createEntity}
        onSelect={canSelectRelation && !this._readonly && this.selectRelation}
        onClear={canClearRelation && !this._readonly && !!entity && this.detachEntity}
        onReload={canReloadRelation && model._ServerId > 0 && this.reloadRelation}
        onClonePrototype={
          canClonePrototype && model._ServerId > 0 && !entity && this.clonePrototype
        }
      />
    );
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const {
      columnProportions,
      fieldOrders,
      fieldEditors,
      canSaveEntity,
      canRefreshEntity,
      canReloadEntity,
      canDetachEntity,
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity
    } = this.props;
    const { isOpen, isTouched } = this.state;
    const entity: EntityObject = model[fieldSchema.FieldName];
    const hasServerId = entity._ServerId > 0;
    return entity ? (
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
              <EntityLink model={entity} contentSchema={fieldSchema.RelatedContent} />
            </td>
            {this._displayFields.map((displayField, i) => (
              <td
                key={i}
                colSpan={columnProportions ? columnProportions[i] : 1}
                className="relation-field-accordion__cell"
              >
                {displayField(entity)}
              </td>
            ))}
            <td key={-3} className="relation-field-accordion__controls">
              <EntityMenu
                small
                onSave={canSaveEntity && this.saveEntity}
                onDetach={canDetachEntity && !this._readonly && this.detachEntity}
                onRemove={canRemoveEntity && hasServerId && this.removeEntity}
                onRefresh={canRefreshEntity && hasServerId && this.refreshEntity}
                onReload={canReloadEntity && hasServerId && this.reloadEntity}
                onClone={canCloneEntity && hasServerId && this.cloneEntity}
                onPublish={canPublishEntity && hasServerId && this.publishEntity}
              />
            </td>
          </tr>
          <tr className="relation-field-accordion__main">
            <td
              className={cn("relation-field-accordion__body", {
                "relation-field-accordion__body--open": isOpen
              })}
              colSpan={this.getBodyColSpan()}
            >
              {isTouched && (
                <EntityEditor
                  model={entity}
                  contentSchema={fieldSchema.RelatedContent}
                  fieldOrders={fieldOrders}
                  fieldEditors={fieldEditors}
                />
              )}
            </td>
          </tr>
        </tbody>
      </table>
    ) : null;
  }
}
