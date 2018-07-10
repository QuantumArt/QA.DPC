import React from "react";
import { consumer } from "react-ioc";
import { action, untracked, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Tab, Tabs, Intent } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema, SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { maxCount } from "Utils/Validators";
import { asc } from "Utils/Array/Sort";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
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
      orderByField = (fieldSchema as MultiRelationFieldSchema).OrderByFieldName || "Id"
    } = props;
    this._orderByField = isString(orderByField) ? article => article[orderByField] : orderByField;
    untracked(() => {
      const array = model[fieldSchema.FieldName];
      if (array.length > 0) {
        const firstArticle = array[0];
        this.state.activeId = firstArticle.Id;
        this.state.touchedIds[firstArticle.Id] = true;
      }
    });
  }

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const { touchedIds } = this.state;
    const contentName = (fieldSchema as MultiRelationFieldSchema).Content.ContentName;
    const article = this._dataContext.createArticle(contentName);
    touchedIds[article.Id] = true;
    this.setState({
      activeId: article.Id,
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
      touchedIds: {}
    });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation = (article: ArticleObject) => {
    const { model, fieldSchema } = this.props;
    const { activeId, touchedIds } = this.state;
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    delete touchedIds[article.Id];
    if (activeId === article.Id) {
      const index = array.indexOf(article);
      const nextArticle = index > 0 ? array[index - 1] : array.length > 1 ? array[1] : null;
      if (nextArticle) {
        touchedIds[nextArticle.Id] = true;
        this.setState({ activeId: nextArticle.Id, touchedIds });
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

  renderControls(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { isOpen } = this.state;
    const list: ArticleObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <div className="relation-field-tabs__controls">
        <ButtonGroup>
          <Button
            minimal
            small
            rightIcon="add"
            intent={Intent.SUCCESS}
            disabled={fieldSchema.IsReadOnly}
            onClick={this.createRelation}
          >
            Создать
          </Button>
          <Button
            minimal
            small
            rightIcon="th-derived"
            intent={Intent.PRIMARY}
            disabled={fieldSchema.IsReadOnly}
          >
            Выбрать
          </Button>
          <Button
            minimal
            small
            rightIcon="eraser"
            intent={Intent.DANGER}
            disabled={fieldSchema.IsReadOnly}
            onClick={this.clearRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
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

  renderValidation(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    return (
      <>
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          silent
          rules={fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)}
        />
        {super.renderValidation(model, fieldSchema)}
      </>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      saveRelations,
      skipOtherFields,
      fieldEditors,
      vertical,
      filterItems,
      children
    } = this.props;
    const { isOpen, isTouched, activeId, touchedIds } = this.state;
    const list: ArticleObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <Tabs
        vertical={vertical}
        id={`${model.Id || ""}.${fieldSchema.FieldName}`}
        className={cn("multi-relation-field-tabs", {
          "multi-relation-field-tabs--hidden": !isOpen,
          "multi-relation-field-tabs--empty": isEmpty,
          "container-md": vertical
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
              let title = this._displayField(article);
              if (title == null) {
                title = "...";
              }
              return (
                <Tab
                  key={article.Id}
                  id={article.Id}
                  panel={
                    touchedIds[article.Id] && (
                      <ArticleEditor
                        model={article}
                        contentSchema={fieldSchema.Content}
                        skipOtherFields={skipOtherFields}
                        fieldEditors={fieldEditors}
                        saveRelations={saveRelations}
                        header
                        buttons={!fieldSchema.IsReadOnly}
                        onRemove={this.removeRelation}
                      >
                        {children}
                      </ArticleEditor>
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
