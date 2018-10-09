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

  private clonePrototype = () => {
    const { model, fieldSchema, onClonePrototype } = this.props;
    onClonePrototype(
      action("clonePrototype", async () => {
        const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
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

  private detachEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onDetachEntity } = this.props;
    const entity = untracked(() => model[fieldSchema.FieldName]);
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

  private removeEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onRemoveEntity } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    const entity = untracked(() => model[fieldSchema.FieldName]);
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

  private cloneEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onCloneEntity } = this.props;
    const relationFieldSchema = fieldSchema as SingleRelationFieldSchema;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    onCloneEntity(
      entity,
      action("cloneEntity", async () => {
        return await this._cloneController.cloneRelatedEntity(model, relationFieldSchema, entity);
      })
    );
  };

  private saveEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onSaveEntity } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    onSaveEntity(
      entity,
      action("saveEntity", async () => {
        await this._productController.savePartialProduct(entity, contentSchema);
      })
    );
  };

  private refreshEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onRefreshEntity } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    onRefreshEntity(
      entity,
      action("refreshEntity", async () => {
        await this._entityController.refreshEntity(entity, contentSchema);
      })
    );
  };

  private reloadEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onReloadEntity } = this.props;
    const contentSchema = (fieldSchema as SingleRelationFieldSchema).RelatedContent;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    onReloadEntity(
      entity,
      action("reloadEntity", async () => {
        await this._entityController.reloadEntity(entity, contentSchema);
      })
    );
  };

  private publishEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema, onPublishEntity } = this.props;
    const entity = untracked(() => model[fieldSchema.FieldName]);
    onPublishEntity(
      entity,
      action("publishEntity", async () => {
        alert("TODO: публикация");
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

  private toggleEditor = (e: any) => {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;

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
    const entity: EntityObject = model[fieldSchema.FieldName];
    return (
      <RelationFieldMenu
        onCreate={canCreateEntity && !this._readonly && !entity && this.createEntity}
        onSelect={canSelectRelation && !this._readonly && this.selectRelation}
        onClear={canClearRelation && !this._readonly && !!entity && this.clearRelation}
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
    const contentSchema = fieldSchema.RelatedContent;
    return entity ? (
      <table className="relation-field-accordion" cellSpacing="0" cellPadding="0">
        <tbody>
          <tr
            className={cn("relation-field-accordion__header", {
              "relation-field-accordion__header--open": isOpen,
              "relation-field-accordion__header--edited": contentSchema.isEdited(entity),
              "relation-field-accordion__header--invalid": contentSchema.hasErrors(entity)
            })}
            onClick={this.toggleEditor}
          >
            <td
              key={-1}
              className="relation-field-accordion__expander"
              title={isOpen ? "Свернуть" : "Развернуть"}
            >
              <Icon icon={isOpen ? "caret-down" : "caret-right"} title={false} />
            </td>
            <td key={-2} className="relation-field-accordion__cell">
              <EntityLink model={entity} contentSchema={contentSchema} />
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
