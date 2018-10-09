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

  private clonePrototype = () => {
    const { model, fieldSchema, onClonePrototype } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    onClonePrototype(
      action("clonePrototype", async () => {
        const clone = await this._cloneController.cloneProductPrototype(model, relationFieldSchema);
        this.setState({
          isOpen: true,
          isTouched: true
        });
        return clone;
      })
    );
  };

  private createEntity = () => {
    const { model, fieldSchema, onCreateEntity } = this.props;
    const contentName = (fieldSchema as SingleRelationFieldSchema).RelatedContent.ContentName;
    onCreateEntity(
      action("createEntity", () => {
        const entity = this._dataContext.createEntity(contentName);

        model[fieldSchema.FieldName] = entity;
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          isOpen: true,
          isTouched: true
        });
        return entity;
      })
    );
  };

  private detachEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onDetachEntity } = this.props;
    onDetachEntity(
      entity,
      action("detachEntity", () => {
        model[fieldSchema.FieldName] = null;
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private removeEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onRemoveEntity } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    onRemoveEntity(
      entity,
      action("removeEntity", async () => {
        await this._entityController.removeRelatedEntity(model, relationFieldSchema, entity);
        this.setState({
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private cloneEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onCloneEntity } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    onCloneEntity(
      entity,
      action("cloneEntity", async () => {
        return await this._cloneController.cloneRelatedEntity(model, relationFieldSchema, entity);
      })
    );
  };

  private clearRelation = () => {
    const { model, fieldSchema, onClearRelation } = this.props;
    onClearRelation(
      action("clearRelation", () => {
        model[fieldSchema.FieldName] = null;
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private selectRelation = () => {
    const { model, fieldSchema, onSelectRelation } = this.props;
    onSelectRelation(
      action("selectRelation", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.selectRelation(
          model,
          fieldSchema as SingleRelationFieldSchema
        );
      })
    );
  };

  private reloadRelation = () => {
    const { model, fieldSchema, onReloadRelation } = this.props;
    onReloadRelation(
      action("reloadRelation", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.reloadRelation(
          model,
          fieldSchema as SingleRelationFieldSchema
        );
      })
    );
  };

  private toggleEditor = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
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
          onClear={canClearRelation && !this._readonly && !!entity && this.clearRelation}
          onReload={canReloadRelation && model._ServerId > 0 && this.reloadRelation}
          onClonePrototype={
            canClonePrototype && model._ServerId > 0 && !entity && this.clonePrototype
          }
        />
        <Button
          small
          disabled={!entity}
          rightIcon={isOpen ? "chevron-up" : "chevron-down"}
          onClick={this.toggleEditor}
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
      onSaveEntity,
      onRefreshEntity,
      onReloadEntity,
      onPublishEntity,
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
          onSaveEntity={onSaveEntity}
          onRefreshEntity={onRefreshEntity}
          onReloadEntity={onReloadEntity}
          onPublishEntity={onPublishEntity}
          onDetachEntity={this.detachEntity}
          onCloneEntity={this.cloneEntity}
          onRemoveEntity={this.removeEntity}
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
