import React from "react";
import { consumer } from "react-ioc";
import { action, untracked, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Tab, Tabs } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema, SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
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

  renderControls(_model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { isOpen } = this.state;
    return (
      <ButtonGroup>
        <Button small icon="add" disabled={fieldSchema.IsReadOnly} onClick={this.createRelation}>
          Создать
        </Button>
        <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
          Выбрать
        </Button>
        <Button small icon="eraser" disabled={fieldSchema.IsReadOnly} onClick={this.clearRelation}>
          Очистить
        </Button>
        <Button
          small
          icon={isOpen ? "collapse-all" : "expand-all"}
          onClick={this.toggleFieldEditor}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </ButtonGroup>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const { saveRelations, fieldEditors, children } = this.props;
    const { isOpen, isTouched, activeId, touchedIds } = this.state;
    const list: ArticleObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <Tabs
        id={`${model.Id || ""}.${fieldSchema.FieldName}`}
        className={cn("relation-field-tabs", {
          "relation-field-tabs--hidden": !isOpen,
          "relation-field-tabs--empty": isEmpty
        })}
        selectedTabId={activeId}
        onChange={this.handleTabChange}
      >
        {isTouched &&
          list &&
          list
            .slice()
            .sort(asc(this._orderByField))
            .map(article => {
              return (
                <Tab
                  key={article.Id}
                  id={article.Id}
                  panel={
                    touchedIds[article.Id] && (
                      <div className="relation-field-tabs__panel relation-field-tabs__panel--multi">
                        <ArticleEditor
                          model={article}
                          contentSchema={fieldSchema.Content}
                          fieldEditors={fieldEditors}
                          saveRelations={saveRelations}
                          header
                          buttons={!fieldSchema.IsReadOnly}
                          onRemove={this.removeRelation}
                        >
                          {children}
                        </ArticleEditor>
                      </div>
                    )
                  }
                >
                  <div className="relation-field-tabs__title">{this._displayField(article)}</div>
                </Tab>
              );
            })}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={[
            fieldSchema.IsRequired && required,
            fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)
          ]}
        />
      </Tabs>
    );
  }
}
