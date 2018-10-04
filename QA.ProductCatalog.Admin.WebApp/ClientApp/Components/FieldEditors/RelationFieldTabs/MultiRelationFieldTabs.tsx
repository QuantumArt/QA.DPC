import React from "react";
import { consumer } from "react-ioc";
import { action, untracked, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, Tab, Tabs } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { asc } from "Utils/Array/Sort";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { FieldSelector } from "../AbstractFieldEditor";
import { AbstractRelationFieldTabs, RelationFieldTabsProps } from "./AbstractRelationFieldTabs";

interface MultiRelationFieldTabsState {
  isOpen: boolean;
  isTouched: boolean;
  activeId: number | null;
  touchedIds: {
    [articleId: number]: boolean;
  };
}

@consumer
@observer
export class MultiRelationFieldTabs extends AbstractRelationFieldTabs {
  static defaultProps = {
    ...AbstractRelationFieldTabs.defaultProps,
    filterItems: () => true
  };

  private _orderByField: FieldSelector;
  readonly state: MultiRelationFieldTabsState = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed,
    activeId: null,
    touchedIds: {}
  };

  constructor(props: RelationFieldTabsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;
    const orderByField =
      props.orderByField || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    this._orderByField = isString(orderByField) ? entity => entity[orderByField] : orderByField;
    untracked(() => {
      const array = props.model[fieldSchema.FieldName] as EntityObject[];
      if (array.length > 0) {
        const firstArticle = array[0];
        this.state.activeId = firstArticle._ClientId;
        this.state.touchedIds[firstArticle._ClientId] = true;
      }
    });
  }

  private clonePrototype = async () => {
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as MultiRelationFieldSchema;
    const clone = await this._cloneController.cloneProductPrototype(model, relationFieldSchema);
    const { touchedIds } = this.state;
    touchedIds[clone._ClientId] = true;
    this.setState({
      activeId: clone._ClientId,
      touchedIds,
      isOpen: true,
      isTouched: true
    });
  };

  @action
  private createEntity = () => {
    const { model, fieldSchema } = this.props;
    const { touchedIds } = this.state;
    const contentName = (fieldSchema as MultiRelationFieldSchema).RelatedContent.ContentName;
    const entity = this._dataContext.createEntity(contentName);
    touchedIds[entity._ClientId] = true;
    this.setState({
      activeId: entity._ClientId,
      touchedIds,
      isOpen: true,
      isTouched: true
    });
    model[fieldSchema.FieldName].push(entity);
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private detachEntity = (entity: EntityObject) => {
    this.deactivateEntity(entity);
    const { model, fieldSchema } = this.props;
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(entity);
      model.setTouched(fieldSchema.FieldName, true);
    }
  };

  private removeEntity = async (entity: EntityObject) => {
    this.deactivateEntity(entity);
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as MultiRelationFieldSchema;
    await this._articleController.removeRelatedEntity(model, relationFieldSchema, entity);
  };

  private deactivateEntity(entity: EntityObject) {
    const { model, fieldSchema } = this.props;
    const array = untracked(() => model[fieldSchema.FieldName]) as EntityObject[];
    const { activeId, touchedIds } = this.state;
    delete touchedIds[entity._ClientId];
    if (activeId === entity._ClientId) {
      const index = array.indexOf(entity);
      const nextArticle = index > 0 ? array[index - 1] : array.length > 1 ? array[1] : null;
      if (nextArticle) {
        touchedIds[nextArticle._ClientId] = true;
        this.setState({ activeId: nextArticle._ClientId, touchedIds });
      } else {
        this.setState({ activeId: null, touchedIds });
      }
    } else {
      this.setState({ touchedIds });
    }
  }

  private async cloneEntity(entity: EntityObject) {
    const { model, fieldSchema } = this.props;
    const relationFieldSchema = fieldSchema as MultiRelationFieldSchema;
    const clone = await this._cloneController.cloneRelatedEntity(
      model,
      relationFieldSchema,
      entity
    );

    const { touchedIds } = this.state;
    touchedIds[clone._ClientId] = true;
    this.setState({
      activeId: clone._ClientId,
      touchedIds,
      isOpen: true,
      isTouched: true
    });
  }

  @action
  private clearRelations = () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      activeId: null,
      touchedIds: {},
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: true,
      isTouched: true
    });
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  private reloadRelations = async () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: true,
      isTouched: true
    });
    await this._relationController.reloadRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  private toggleRelation = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  private handleTabChange = (newTabId: number, _prevTabId: number, _e: any) => {
    const { touchedIds } = this.state;
    touchedIds[newTabId] = true;
    this.setState({
      activeId: newTabId,
      touchedIds
    });
  };

  renderControls(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      canCreateEntity,
      canSelectRelation,
      canClearRelation,
      canReloadRelation,
      canClonePrototype
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
          onClick={this.toggleRelation}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </div>
    );
  }

  private static _nextTabId = 0;
  private static _tabIdsByModel = new WeakMap<ArticleObject, number>();

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      skipOtherFields,
      fieldOrders,
      fieldEditors,
      renderOnlyActiveTab,
      vertical,
      filterItems,
      className,
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
    const isEmpty = !list || list.length === 0;
    const isSingle = list && list.length === 1;
    let tabId = MultiRelationFieldTabs._tabIdsByModel.get(model);
    if (!tabId) {
      tabId = MultiRelationFieldTabs._nextTabId++;
      MultiRelationFieldTabs._tabIdsByModel.set(model, tabId);
    }
    return (
      <Tabs
        renderActiveTabPanelOnly={renderOnlyActiveTab}
        vertical={vertical}
        id={`${tabId}_${fieldSchema.FieldName}`}
        className={cn("multi-relation-field-tabs", className, {
          "multi-relation-field-tabs--hidden": !isOpen,
          "multi-relation-field-tabs--empty": isEmpty,
          "multi-relation-field-tabs--single": isSingle,
          "container-md": vertical && !className
        })}
        selectedTabId={activeId}
        onChange={this.handleTabChange}
      >
        {isTouched &&
          list &&
          list
            .filter(filterItems)
            .sort(asc(this._orderByField))
            .map(entity => {
              const title = this.getTitle(entity);
              return (
                <Tab
                  key={entity._ClientId}
                  id={entity._ClientId}
                  panel={
                    touchedIds[entity._ClientId] && (
                      <EntityEditor
                        model={entity}
                        contentSchema={fieldSchema.RelatedContent}
                        skipOtherFields={skipOtherFields}
                        fieldOrders={fieldOrders}
                        fieldEditors={fieldEditors}
                        withHeader
                        onClone={this.cloneEntity}
                        onDetach={this.detachEntity}
                        onRemove={this.removeEntity}
                        canSaveEntity={canSaveEntity}
                        canRefreshEntity={canRefreshEntity}
                        canReloadEntity={canReloadEntity}
                        canDetachEntity={!this._readonly && canDetachEntity}
                        canRemoveEntity={canRemoveEntity}
                        canPublishEntity={canPublishEntity}
                        canCloneEntity={canCloneEntity}
                      />
                    )
                  }
                >
                  <div
                    className="multi-relation-field-tabs__title"
                    title={isString(title) ? title : ""}
                  >
                    {title}
                  </div>
                </Tab>
              );
            })}
      </Tabs>
    );
  }
}
