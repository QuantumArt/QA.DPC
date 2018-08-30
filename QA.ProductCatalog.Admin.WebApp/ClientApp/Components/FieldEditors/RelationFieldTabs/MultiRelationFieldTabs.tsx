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
    const {
      model,
      fieldSchema,
      orderByField = (fieldSchema as MultiRelationFieldSchema).OrderByFieldName ||
        ArticleObject._ServerId
    } = props;
    this._orderByField = isString(orderByField) ? article => article[orderByField] : orderByField;
    untracked(() => {
      const array = model[fieldSchema.FieldName] as EntityObject[];
      if (array.length > 0) {
        const firstArticle = array[0];
        this.state.activeId = firstArticle._ClientId;
        this.state.touchedIds[firstArticle._ClientId] = true;
      }
    });
  }

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const { touchedIds } = this.state;
    const contentName = (fieldSchema as MultiRelationFieldSchema).RelatedContent.ContentName;
    const article = this._dataContext.createArticle(contentName);
    touchedIds[article._ClientId] = true;
    this.setState({
      activeId: article._ClientId,
      touchedIds,
      isOpen: true,
      isTouched: true
    });
    model[fieldSchema.FieldName].push(article);
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private clearRelation = () => {
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

  @action
  private removeRelation = (article: EntityObject) => {
    const { model, fieldSchema } = this.props;
    const { activeId, touchedIds } = this.state;
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    delete touchedIds[article._ClientId];
    if (activeId === article._ClientId) {
      const index = array.indexOf(article);
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
    if (array) {
      array.remove(article);
      model.setTouched(fieldSchema.FieldName, true);
    }
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

  private handleTabChange = (newTabId: number, _prevTabId: number, _e: any) => {
    const { touchedIds } = this.state;
    touchedIds[newTabId] = true;
    this.setState({
      activeId: newTabId,
      touchedIds
    });
  };

  private toggleFieldEditor = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  renderControls(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const { isOpen } = this.state;
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <div className="relation-field-tabs__controls">
        <RelationFieldMenu
          onCreate={this._canEditRelation && this.createRelation}
          onSelect={this._canEditRelation && this.selectRelations}
          onClear={this._canEditRelation && !isEmpty && this.clearRelation}
          onReload={model._ServerId > 0 && this.reloadRelations}
        />
        <Button
          small
          disabled={isEmpty}
          rightIcon={isOpen ? "chevron-up" : "chevron-down"}
          onClick={this.toggleFieldEditor}
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
      fieldEditors,
      vertical,
      filterItems,
      className,
      children
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
            .map(article => {
              const title = this.getTitle(article);
              return (
                <Tab
                  key={article._ClientId}
                  id={article._ClientId}
                  panel={
                    touchedIds[article._ClientId] && (
                      <EntityEditor
                        model={article}
                        contentSchema={fieldSchema.RelatedContent}
                        skipOtherFields={skipOtherFields}
                        fieldEditors={fieldEditors}
                        header
                        buttons={this._canEditRelation}
                        onRemove={this.removeRelation}
                      >
                        {children}
                      </EntityEditor>
                    )
                  }
                >
                  <div className="multi-relation-field-tabs__title" title={title}>
                    {title}
                  </div>
                </Tab>
              );
            })}
      </Tabs>
    );
  }
}
