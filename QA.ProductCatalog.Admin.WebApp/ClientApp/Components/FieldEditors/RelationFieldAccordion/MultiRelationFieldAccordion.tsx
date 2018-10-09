import React, { Fragment } from "react";
import { consumer } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Icon, Button } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema, SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { asc } from "Utils/Array/Sort";
import { EntityMenu } from "Components/ArticleEditor/EntityMenu";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { FieldSelector } from "../AbstractFieldEditor";
import {
  AbstractRelationFieldAccordion,
  RelationFieldAccordionProps
} from "./AbstractRelationFieldAccordion";
import { EntityLink } from "Components/ArticleEditor/EntityLink";

interface MultiRelationFieldAccordionState {
  isOpen: boolean;
  isTouched: boolean;
  activeId: number | null;
  touchedIds: {
    [articleId: number]: boolean;
  };
}

@consumer
@observer
export class MultiRelationFieldAccordion extends AbstractRelationFieldAccordion {
  static defaultProps = {
    ...AbstractRelationFieldAccordion.defaultProps,
    filterItems: () => true
  };

  private _orderByField: FieldSelector;
  readonly state: MultiRelationFieldAccordionState = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed,
    activeId: null,
    touchedIds: {}
  };

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;
    const orderByField =
      props.orderByField || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    this._orderByField = isString(orderByField) ? entity => entity[orderByField] : orderByField;
  }

  private clonePrototype = () => {
    const { model, fieldSchema, onClonePrototype } = this.props;
    const relationFieldSchema = fieldSchema as MultiRelationFieldSchema;
    onClonePrototype(
      action("clonePrototype", async () => {
        const clone = await this._cloneController.cloneProductPrototype(model, relationFieldSchema);
        this.toggleScreen(clone);
        return clone;
      })
    );
  };

  private createEntity = () => {
    const { model, fieldSchema, onCreateEntity } = this.props;
    const contentName = (fieldSchema as MultiRelationFieldSchema).RelatedContent.ContentName;
    onCreateEntity(
      action("createEntity", () => {
        const entity = this._dataContext.createEntity(contentName);
        model[fieldSchema.FieldName].push(entity);
        model.setTouched(fieldSchema.FieldName, true);
        this.toggleScreen(entity);
        return entity;
      })
    );
  };

  private detachEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema, onDetachEntity } = this.props;
    onDetachEntity(
      entity,
      action("detachEntity", () => {
        const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
        if (array) {
          array.remove(entity);
          model.setTouched(fieldSchema.FieldName, true);
        }
        this.deactivateScreen(entity);
      })
    );
  }

  private removeEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema, onRemoveEntity } = this.props;
    const relationFieldSchema = fieldSchema as MultiRelationFieldSchema;
    onRemoveEntity(
      entity,
      action("removeEntity", async () => {
        await this._entityController.removeRelatedEntity(model, relationFieldSchema, entity);
        this.deactivateScreen(entity);
      })
    );
  }

  private saveEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { fieldSchema, onSaveEntity } = this.props;
    const contentSchema = (fieldSchema as MultiRelationFieldSchema).RelatedContent;
    onSaveEntity(
      entity,
      action("saveEntity", async () => {
        await this._productController.savePartialProduct(entity, contentSchema);
      })
    );
  }

  private refreshEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { fieldSchema, onRefreshEntity } = this.props;
    const contentSchema = (fieldSchema as MultiRelationFieldSchema).RelatedContent;
    onRefreshEntity(
      entity,
      action("refreshEntity", async () => {
        await this._entityController.refreshEntity(entity, contentSchema);
      })
    );
  }

  private reloadEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { fieldSchema, onReloadEntity } = this.props;
    const contentSchema = (fieldSchema as MultiRelationFieldSchema).RelatedContent;
    onReloadEntity(
      entity,
      action("reloadEntity", async () => {
        await this._entityController.reloadEntity(entity, contentSchema);
      })
    );
  }

  private cloneEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema, onCloneEntity } = this.props;
    const relationFieldSchema = fieldSchema as MultiRelationFieldSchema;
    onCloneEntity(
      entity,
      action("cloneEntity", async () => {
        const clone = await this._cloneController.cloneRelatedEntity(
          model,
          relationFieldSchema,
          entity
        );
        this.toggleScreen(clone);
        return clone;
      })
    );
  }

  private publishEntity = (e: any, entity: EntityObject) => {
    e.stopPropagation();
    const { onPublishEntity } = this.props;
    onPublishEntity(
      entity,
      action("publishEntity", async () => {
        alert("TODO: публикация");
      })
    );
  };

  private clearRelations = () => {
    const { model, fieldSchema, onClearRelation } = this.props;
    onClearRelation(
      action("clearRelations", () => {
        model[fieldSchema.FieldName] = [];
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          activeId: null,
          touchedIds: {},
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private selectRelations = () => {
    const { model, fieldSchema, onSelectRelation } = this.props;
    onSelectRelation(
      action("selectRelations", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.selectRelations(
          model,
          fieldSchema as MultiRelationFieldSchema
        );
      })
    );
  };

  private reloadRelations = () => {
    const { model, fieldSchema, onReloadRelation } = this.props;
    onReloadRelation(
      action("reloadRelations", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.reloadRelations(
          model,
          fieldSchema as MultiRelationFieldSchema
        );
      })
    );
  };

  private handleToggle(e: any, entity: EntityObject) {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;

    this.toggleScreen(entity);
  }

  private toggleScreen(entity: EntityObject) {
    const { activeId, touchedIds } = this.state;
    if (activeId === entity._ClientId) {
      this.setState({
        activeId: null,
        isOpen: true,
        isTouched: true
      });
    } else {
      touchedIds[entity._ClientId] = true;
      this.setState({
        activeId: entity._ClientId,
        touchedIds,
        isOpen: true,
        isTouched: true
      });
    }
  }

  private deactivateScreen(entity: EntityObject) {
    const { activeId, touchedIds } = this.state;
    delete touchedIds[entity._ClientId];
    if (activeId === entity._ClientId) {
      this.setState({ activeId: null, touchedIds });
    } else {
      this.setState({ touchedIds });
    }
  }

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
      canClonePrototype,
      canSelectRelation,
      canClearRelation,
      canReloadRelation
    } = this.props;
    const { isOpen } = this.state;
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <div className="relation-field-tabs__controls">
        <RelationFieldMenu
          onCreate={canCreateEntity && !this._readonly && this.createEntity}
          onSelect={canSelectRelation && !this._readonly && this.selectRelations}
          onClear={canClearRelation && !this._readonly && !isEmpty && this.clearRelations}
          onReload={canReloadRelation && model._ServerId > 0 && this.reloadRelations}
          onClonePrototype={canClonePrototype && model._ServerId > 0 && this.clonePrototype}
        />
        <Button
          small
          disabled={isEmpty}
          rightIcon={isOpen ? "chevron-up" : "chevron-down"}
          onClick={this.toggleEditor}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </div>
    );
  }

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      columnProportions,
      fieldOrders,
      fieldEditors,
      filterItems,
      renderOnlyActiveSection,
      canSaveEntity,
      canRefreshEntity,
      canReloadEntity,
      canDetachEntity,
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity
    } = this.props;
    const { isOpen, isTouched, activeId, touchedIds } = this.state;
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const contentSchema = fieldSchema.RelatedContent;
    return isTouched && list ? (
      <table
        className={cn("relation-field-accordion", {
          "relation-field-accordion--hidden": !isOpen
        })}
        cellSpacing="0"
        cellPadding="0"
      >
        <tbody>
          {list
            .filter(filterItems)
            .sort(asc(this._orderByField))
            .map(entity => {
              const isOpen = entity._ClientId === activeId;
              const hasServerId = entity._ServerId > 0;
              return (
                <Fragment key={entity._ClientId}>
                  <tr
                    className={cn("relation-field-accordion__header", {
                      "relation-field-accordion__header--open": isOpen,
                      "relation-field-accordion__header--edited": contentSchema.isEdited(entity),
                      "relation-field-accordion__header--invalid": contentSchema.hasErrors(entity)
                    })}
                    onClick={e => this.handleToggle(e, entity)}
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
                        onSave={canSaveEntity && (e => this.saveEntity(e, entity))}
                        onDetach={
                          canDetachEntity && !this._readonly && (e => this.detachEntity(e, entity))
                        }
                        onRemove={
                          canRemoveEntity && hasServerId && (e => this.removeEntity(e, entity))
                        }
                        onRefresh={
                          canRefreshEntity && hasServerId && (e => this.refreshEntity(e, entity))
                        }
                        onReload={
                          canReloadEntity && hasServerId && (e => this.reloadEntity(e, entity))
                        }
                        onClone={
                          canCloneEntity && hasServerId && (e => this.cloneEntity(e, entity))
                        }
                        onPublish={
                          canPublishEntity && hasServerId && (e => this.publishEntity(e, entity))
                        }
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
                      {(isOpen || !renderOnlyActiveSection) &&
                        touchedIds[entity._ClientId] && (
                          <EntityEditor
                            model={entity}
                            contentSchema={fieldSchema.RelatedContent}
                            fieldOrders={fieldOrders}
                            fieldEditors={fieldEditors}
                          />
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
