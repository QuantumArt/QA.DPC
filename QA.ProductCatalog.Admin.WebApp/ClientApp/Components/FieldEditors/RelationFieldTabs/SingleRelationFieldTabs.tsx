import React from "react";
import { consumer } from "react-ioc";
import { action, untracked } from "mobx";
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

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const contentName = (fieldSchema as SingleRelationFieldSchema).RelatedContent.ContentName;
    const article = this._dataContext.createEntity(contentName);
    this.setState({
      isOpen: true,
      isTouched: true
    });
    model[fieldSchema.FieldName] = article;
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation = () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
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

  private async cloneRelation() {
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    if (entity) {
      await this._cloneController.cloneRelatedEntity(model, relationFieldSchema, entity);
    }
  }

  renderControls(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const { canCreateEntity, canSelectRelation, canClearRelation, canReloadRelation } = this.props;
    const { isOpen } = this.state;
    const article: EntityObject = model[fieldSchema.FieldName];
    return (
      <div className="relation-field-tabs__controls">
        <RelationFieldMenu
          onCreate={canCreateEntity && !this._readonly && !article && this.createRelation}
          onSelect={canSelectRelation && !this._readonly && this.selectRelation}
          onClear={canClearRelation && !this._readonly && !!article && this.removeRelation}
          onReload={canReloadRelation && model._ServerId > 0 && this.reloadRelation}
        />
        <Button
          small
          disabled={!article}
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
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity
    } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: EntityObject = model[fieldSchema.FieldName];
    return isTouched && article ? (
      <div
        className={cn("single-relation-field-tabs", className, {
          "single-relation-field-tabs--hidden": !isOpen,
          "single-relation-field-tabs--borderless": borderless
        })}
      >
        <EntityEditor
          withHeader
          model={article}
          contentSchema={fieldSchema.RelatedContent}
          skipOtherFields={skipOtherFields}
          fieldOrders={fieldOrders}
          fieldEditors={fieldEditors}
          onRemove={this.removeRelation}
          onClone={this.cloneRelation}
          canSaveEntity={canSaveEntity}
          canRefreshEntity={canRefreshEntity}
          canReloadEntity={canReloadEntity}
          canRemoveEntity={!this._readonly && canRemoveEntity}
          canPublishEntity={canPublishEntity}
          canCloneEntity={canCloneEntity}
        />
      </div>
    ) : null;
  }
}
