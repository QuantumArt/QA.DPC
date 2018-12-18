import React from "react";
import cn from "classnames";
import { inject } from "react-ioc";
import { action, untracked, IObservableArray, computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Button, Tab, Tabs } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString, isNullOrWhiteSpace } from "Utils/TypeChecks";
import { DataContext } from "Services/DataContext";
import { EntityController } from "Services/EntityController";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import {
  AbstractRelationFieldEditor,
  FieldSelector,
  ExpandableFieldEditorProps,
  EntityComparer
} from "Components/FieldEditors/AbstractFieldEditor";
import "./RelationFieldTabs.scss";

interface RelationFieldTabsProps extends ExpandableFieldEditorProps {
  /** Предикат фильтрации связанных статей при отображении во вкладках */
  filterItems?: (item: EntityObject) => boolean;
  /** Функция сравнения связанных статей для сортировки */
  sortItems?: EntityComparer;
  /** Селектор поля для сравнения связанных статей для сортировки */
  sortItemsBy?: string | FieldSelector;
  /** Селектор поля для заголовка вкладки */
  displayField?: string | FieldSelector;
  /** Селектор поля для заголовка вутри вложенного @see EntityEditor */
  titleField?: string | FieldSelector;
  /** Свернуты по-умолчанию */
  collapsed?: boolean;
  /** Вертикальные вкладки */
  vertical?: boolean;
  /** Кастомный className, добавляющийся к "relation-field-tabs" */
  className?: string;
  /**
   * Отрисовывать сразу все вкладки, или только открытую.
   * Влияет на производительность и выполнение валидаций, заданных внутри вложенного @see EntityEditor
   */
  renderAllTabs?: boolean;
}

interface RelationFieldTabsState {
  isOpen: boolean;
  isTouched: boolean;
  activeId: number | null;
}

const defaultRelationHandler = action => action();
const defaultEntityHandler = (_entity, action) => action();

/** Отображение множественного поля-связи в виде вкладок (возможно вертикальных) */
@observer
export class RelationFieldTabs extends AbstractRelationFieldEditor<RelationFieldTabsProps> {
  static defaultProps = {
    filterItems: () => true,
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true,
    onClonePrototype: defaultRelationHandler,
    onCreateEntity: defaultRelationHandler,
    onCloneEntity: defaultEntityHandler,
    onRemoveEntity: defaultEntityHandler,
    onDetachEntity: defaultEntityHandler,
    onSelectRelation: defaultRelationHandler,
    onReloadRelation: defaultRelationHandler,
    onClearRelation: defaultRelationHandler
  };

  @inject private _dataContext: DataContext;
  @inject private _entityController: EntityController;

  private _displayField: FieldSelector;
  private _entityComparer: EntityComparer;

  readonly state: RelationFieldTabsState = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed,
    activeId: null
  };

  @computed
  private get dataSource() {
    const { model, fieldSchema, filterItems } = this.props;
    const array: EntityObject[] = model[fieldSchema.FieldName];
    return array && array.filter(filterItems).sort(this._entityComparer);
  }

  constructor(props: RelationFieldTabsProps, context?: any) {
    super(props, context);
    const { fieldSchema, sortItems, sortItemsBy, displayField } = props as PrivateProps;
    this._displayField = this.makeDisplayFieldSelector(displayField, fieldSchema);
    this._entityComparer = this.makeEntityComparer(sortItems || sortItemsBy, fieldSchema);

    untracked(() => {
      const array = props.model[fieldSchema.FieldName] as EntityObject[];
      if (array.length > 0) {
        const firstArticle = array[0];
        this.state.activeId = firstArticle._ClientId;
      }
    });
  }

  private clonePrototype = () => {
    const { model, fieldSchema, onClonePrototype } = this.props as PrivateProps;
    onClonePrototype(
      action("clonePrototype", async () => {
        const clone = await this._relationController.cloneProductPrototype(model, fieldSchema);
        this.setState({
          activeId: clone._ClientId,
          isOpen: true,
          isTouched: true
        });
        return clone;
      })
    );
  };

  private createEntity = () => {
    const { model, fieldSchema, onCreateEntity } = this.props as PrivateProps;
    const contentName = fieldSchema.RelatedContent.ContentName;
    onCreateEntity(
      action("createEntity", () => {
        const entity = this._dataContext.createEntity(contentName);
        model[fieldSchema.FieldName].push(entity);
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          activeId: entity._ClientId,
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
        const nextEntity = this.getNextTab(entity);
        const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
        if (array) {
          array.remove(entity);
          model.setTouched(fieldSchema.FieldName, true);
        }
        this.deactivateTab(entity, nextEntity);
      })
    );
  };

  private removeEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onRemoveEntity } = this.props as PrivateProps;
    onRemoveEntity(
      entity,
      action("removeEntity", async () => {
        const nextEntity = this.getNextTab(entity);
        const isRemoved = await this._entityController.removeRelatedEntity(
          model,
          fieldSchema,
          entity
        );
        if (isRemoved) {
          this.deactivateTab(entity, nextEntity);
        }
        return isRemoved;
      })
    );
  };

  private async cloneEntity(entity: EntityObject) {
    const { model, fieldSchema, onCloneEntity } = this.props as PrivateProps;
    onCloneEntity(
      entity,
      action("cloneEntity", async () => {
        const clone = await this._entityController.cloneRelatedEntity(model, fieldSchema, entity);

        this.setState({
          activeId: clone._ClientId,
          isOpen: true,
          isTouched: true
        });

        return clone;
      })
    );
  }

  private clearRelations = () => {
    const { model, fieldSchema, onClearRelation } = this.props;
    onClearRelation(
      action("clearRelations", () => {
        model[fieldSchema.FieldName].replace([]);
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          activeId: null,
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private selectRelations = () => {
    const { model, fieldSchema, onSelectRelation } = this.props as PrivateProps;
    onSelectRelation(
      action("selectRelations", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.selectRelations(model, fieldSchema);
      })
    );
  };

  private reloadRelations = () => {
    const { model, fieldSchema, onReloadRelation } = this.props as PrivateProps;
    onReloadRelation(
      action("reloadRelations", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.reloadRelations(model, fieldSchema);
      })
    );
  };

  private getNextTab(entity: EntityObject) {
    const { model, fieldSchema } = this.props;
    return untracked(() => {
      const array = model[fieldSchema.FieldName] as EntityObject[];
      const index = array.indexOf(entity);
      return index > 0 ? array[index - 1] : array.length > 1 ? array[1] : null;
    });
  }

  private deactivateTab(entity: EntityObject, nextEntity?: EntityObject) {
    const { activeId } = this.state;
    if (activeId === entity._ClientId) {
      if (nextEntity) {
        this.setState({ activeId: nextEntity._ClientId });
      } else {
        this.setState({ activeId: null });
      }
    }
  }

  private toggleEditor = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  private handleTabChange = (newTabId: number, _prevTabId: number, _e: any) => {
    this.setState({
      activeId: newTabId
    });
  };

  private getTitle(entity: EntityObject) {
    const title = this._displayField(entity);
    return isNullOrWhiteSpace(title) ? "..." : title;
  }

  render() {
    const { model, fieldSchema } = this.props as PrivateProps;
    return (
      <Col
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={2} md={3} className="field-editor__label field-editor__label--small">
            {this.renderLabel(model, fieldSchema)}
          </Col>
          <Col md>
            {this.renderControls(model, fieldSchema)}
            {this.renderValidation(model, fieldSchema)}
          </Col>
        </Row>
        <Row>
          <Col md>{this.renderField(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }

  renderControls(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      relationActions,
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
        >
          {relationActions && relationActions()}
        </RelationFieldMenu>
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

  private static _nextTabId = 0;
  private static _tabIdsByModel = new WeakMap<ArticleObject, number>();

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      skipOtherFields,
      fieldOrders,
      fieldEditors,
      vertical,
      renderAllTabs,
      className,
      titleField,
      entityActions,
      onMountEntity,
      onUnmountEntity,
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
      canCloneEntity,
      children
    } = this.props;
    const { isOpen, isTouched, activeId } = this.state;
    const dataSource = this.dataSource;
    const contentSchema = fieldSchema.RelatedContent;
    const isEmpty = !dataSource || dataSource.length === 0;
    const isSingle = dataSource && dataSource.length === 1;
    let tabId = RelationFieldTabs._tabIdsByModel.get(model);
    if (!tabId) {
      tabId = RelationFieldTabs._nextTabId++;
      RelationFieldTabs._tabIdsByModel.set(model, tabId);
    }
    return (
      <Tabs
        renderActiveTabPanelOnly={!renderAllTabs}
        vertical={vertical}
        id={`${tabId}_${fieldSchema.FieldName}`}
        className={cn("relation-field-tabs", className, {
          "relation-field-tabs--hidden": !isOpen,
          "relation-field-tabs--empty": isEmpty,
          "relation-field-tabs--single": isSingle,
          "container-md": vertical && !className
        })}
        selectedTabId={activeId}
        onChange={this.handleTabChange}
      >
        {isTouched &&
          dataSource &&
          dataSource.map(entity => {
            const title = this.getTitle(entity);
            const shouldRender = renderAllTabs || activeId === entity._ClientId;
            const isEdited = contentSchema.isEdited(entity);
            const hasVisibleErrors = contentSchema.hasVisibleErrors(entity);
            return (
              <Tab
                key={entity._ClientId}
                id={entity._ClientId}
                panel={
                  shouldRender && (
                    <EntityEditor
                      model={entity}
                      contentSchema={fieldSchema.RelatedContent}
                      skipOtherFields={skipOtherFields}
                      fieldOrders={fieldOrders}
                      fieldEditors={fieldEditors}
                      titleField={titleField}
                      withHeader
                      onMountEntity={onMountEntity}
                      onUnmountEntity={onUnmountEntity}
                      onSaveEntity={onSaveEntity}
                      onRefreshEntity={onRefreshEntity}
                      onReloadEntity={onReloadEntity}
                      onPublishEntity={onPublishEntity}
                      onCloneEntity={this.cloneEntity}
                      onDetachEntity={this.detachEntity}
                      onRemoveEntity={this.removeEntity}
                      canSaveEntity={canSaveEntity}
                      canRefreshEntity={canRefreshEntity}
                      canReloadEntity={canReloadEntity}
                      canDetachEntity={!this._readonly && canDetachEntity}
                      canRemoveEntity={canRemoveEntity}
                      canPublishEntity={canPublishEntity}
                      canCloneEntity={canCloneEntity}
                      customActions={entityActions}
                    >
                      {children}
                    </EntityEditor>
                  )
                }
              >
                <div
                  className={cn("relation-field-tabs__title", {
                    "relation-field-tabs__title--edited": isEdited,
                    "relation-field-tabs__title--invalid": hasVisibleErrors
                  })}
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

interface PrivateProps extends RelationFieldTabsProps {
  fieldSchema: MultiRelationFieldSchema;
}
