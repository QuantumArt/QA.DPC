import React from "react";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTabs } from "./AbstractRelationFieldTabs";

@consumer
@observer
export class SingleRelationFieldTabs extends AbstractRelationFieldTabs {
  readonly state = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed
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
  private detachEntity = (_entity: EntityObject) => {
    this.setState({
      isOpen: false,
      isTouched: false
    });
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private removeEntity = async (entity: EntityObject) => {
    this.setState({
      isOpen: false,
      isTouched: false
    });
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    await this._articleController.removeRelatedEntity(model, relationFieldSchema, entity);
  };

  private cloneEntity = async (entity: EntityObject) => {
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    if (entity) {
      await this._cloneController.cloneRelatedEntity(model, relationFieldSchema, entity);
    }
  };

  private toggleRelation = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  private selectRelation = async () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: true,
      isTouched: true
    });
    await this._relationController.selectRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  private reloadRelation = async () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: true,
      isTouched: true
    });
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
    const { isOpen } = this.state;
    const entity: EntityObject = model[fieldSchema.FieldName];
    return (
      <div className="relation-field-tabs__controls">
        <RelationFieldMenu
          onCreate={canCreateEntity && !this._readonly && !entity && this.createEntity}
          onSelect={canSelectRelation && !this._readonly && this.selectRelation}
          onClear={canClearRelation && !this._readonly && !!entity && this.detachEntity}
          onReload={canReloadRelation && model._ServerId > 0 && this.reloadRelation}
          onClonePrototype={
            canClonePrototype && model._ServerId > 0 && !entity && this.clonePrototype
          }
        />
        <Button
          small
          disabled={!entity}
          rightIcon={isOpen ? "chevron-up" : "chevron-down"}
          onClick={this.toggleRelation}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </div>
    );
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const {
      skipOtherFields,
      fieldOrders,
      fieldEditors,
      borderless,
      className,
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
    return isTouched && entity ? (
      <div
        className={cn("single-relation-field-tabs", className, {
          "single-relation-field-tabs--hidden": !isOpen,
          "single-relation-field-tabs--borderless": borderless
        })}
      >
        <EntityEditor
          withHeader
          model={entity}
          contentSchema={fieldSchema.RelatedContent}
          skipOtherFields={skipOtherFields}
          fieldOrders={fieldOrders}
          fieldEditors={fieldEditors}
          onDetach={this.detachEntity}
          onClone={this.cloneEntity}
          onRemove={this.removeEntity}
          canSaveEntity={canSaveEntity}
          canRefreshEntity={canRefreshEntity}
          canReloadEntity={canReloadEntity}
          canDetachEntity={!this._readonly && canDetachEntity}
          canRemoveEntity={canRemoveEntity}
          canPublishEntity={canPublishEntity}
          canCloneEntity={canCloneEntity}
        />
      </div>
    ) : null;
  }
}
