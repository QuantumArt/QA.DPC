import React, { Fragment, ReactNode } from "react";
import cn from "classnames";
import { consumer, inject } from "react-ioc";
import { action, IObservableArray, computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Icon, Button } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { ComputedCache } from "Utils/WeakCache";
import { isString } from "Utils/TypeChecks";
import { asc } from "Utils/Array/Sort";
import { DataContext } from "Services/DataContext";
import { ProductController } from "Services/ProductController";
import { EntityController } from "Services/EntityController";
import { CloneController } from "Services/CloneController";
import { PublicationController } from "Services/PublicationController";
import { EntityMenu } from "Components/ArticleEditor/EntityMenu";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { EntityLink } from "Components/ArticleEditor/EntityLink";
import {
  AbstractRelationFieldEditor,
  ExpandableFieldEditorProps,
  FieldSelector
} from "Components/FieldEditors/AbstractFieldEditor";
import "./RelationFieldAccordion.scss";

export interface RelationFieldAccordionProps extends ExpandableFieldEditorProps {
  filterItems?: (item: EntityObject) => boolean;
  columnProportions?: number[];
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
  collapsed?: boolean;
}

interface RelationFieldAccordionState {
  isOpen: boolean;
  isTouched: boolean;
  activeId: number | null;
}

const defaultRelationHandler = action => action();
const defaultEntityHandler = (_entity, action) => action();

@consumer
@observer
export class RelationFieldAccordion extends AbstractRelationFieldEditor<
  RelationFieldAccordionProps
> {
  static defaultProps = {
    filterItems: () => true,
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true,
    onClonePrototype: defaultRelationHandler,
    onCreateEntity: defaultRelationHandler,
    onCloneEntity: defaultEntityHandler,
    onSaveEntity: defaultEntityHandler,
    onRefreshEntity: defaultEntityHandler,
    onReloadEntity: defaultEntityHandler,
    onRemoveEntity: defaultEntityHandler,
    onPublishEntity: defaultEntityHandler,
    onDetachEntity: defaultEntityHandler,
    onSelectRelation: defaultRelationHandler,
    onReloadRelation: defaultRelationHandler,
    onClearRelation: defaultRelationHandler
  };

  @inject private _dataContext: DataContext;
  @inject private _entityController: EntityController;
  @inject private _cloneController: CloneController;
  @inject private _publicationController: PublicationController;
  @inject private _productController: ProductController;
  private _columnProportions?: number[];
  private _displayFields: FieldSelector[];
  private _orderByField: FieldSelector;

  readonly state: RelationFieldAccordionState = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed,
    activeId: null
  };

  @computed
  private get dataSource() {
    const { model, fieldSchema, filterItems } = this.props;
    const array: EntityObject[] = model[fieldSchema.FieldName];
    return array && array.filter(filterItems).sort(asc(this._orderByField));
  }

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;

    const displayFields = props.displayFields || fieldSchema.DisplayFieldNames || [];
    this._displayFields = displayFields.map(
      field => (isString(field) ? entity => entity[field] : field)
    );

    const orderByField =
      props.orderByField || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    this._orderByField = isString(orderByField) ? entity => entity[orderByField] : orderByField;

    this._columnProportions = props.columnProportions;
  }

  private clonePrototype = () => {
    const { model, fieldSchema, onClonePrototype } = this.props as PrivateProps;
    onClonePrototype(
      action("clonePrototype", async () => {
        const clone = await this._cloneController.cloneProductPrototype(model, fieldSchema);
        this.toggleScreen(clone);
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
    const { model, fieldSchema, onRemoveEntity } = this.props as PrivateProps;
    onRemoveEntity(
      entity,
      action("removeEntity", async () => {
        await this._entityController.removeRelatedEntity(model, fieldSchema, entity);
        this.deactivateScreen(entity);
      })
    );
  }

  private saveEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { fieldSchema, onSaveEntity } = this.props as PrivateProps;
    const contentSchema = fieldSchema.RelatedContent;
    onSaveEntity(
      entity,
      action("saveEntity", async () => {
        await this._productController.savePartialProduct(entity, contentSchema);
      })
    );
  }

  private refreshEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { fieldSchema, onRefreshEntity } = this.props as PrivateProps;
    const contentSchema = fieldSchema.RelatedContent;
    onRefreshEntity(
      entity,
      action("refreshEntity", async () => {
        await this._entityController.refreshEntity(entity, contentSchema);
      })
    );
  }

  private reloadEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { fieldSchema, onReloadEntity } = this.props as PrivateProps;
    const contentSchema = fieldSchema.RelatedContent;
    onReloadEntity(
      entity,
      action("reloadEntity", async () => {
        await this._entityController.reloadEntity(entity, contentSchema);
      })
    );
  }

  private cloneEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema, onCloneEntity } = this.props as PrivateProps;
    onCloneEntity(
      entity,
      action("cloneEntity", async () => {
        const clone = await this._cloneController.cloneRelatedEntity(model, fieldSchema, entity);
        this.toggleScreen(clone);
        return clone;
      })
    );
  }

  private publishEntity = (e: any, entity: EntityObject) => {
    e.stopPropagation();
    const { fieldSchema, onPublishEntity } = this.props as PrivateProps;
    onPublishEntity(
      entity,
      action("publishEntity", async () => {
        await this._publicationController.publishEntity(entity, fieldSchema.RelatedContent);
      })
    );
  };

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

  private handleToggle(e: any, entity: EntityObject) {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;

    this.toggleScreen(entity);
  }

  private toggleScreen(entity: EntityObject) {
    const { activeId } = this.state;
    if (activeId === entity._ClientId) {
      this.setState({
        activeId: null,
        isOpen: true,
        isTouched: true
      });
    } else {
      this.setState({
        activeId: entity._ClientId,
        isOpen: true,
        isTouched: true
      });
    }
  }

  private deactivateScreen(entity: EntityObject) {
    const { activeId } = this.state;
    if (activeId === entity._ClientId) {
      this.setState({ activeId: null });
    }
  }

  private toggleEditor = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  private getBodyColSpan() {
    const { columnProportions } = this.props;
    return columnProportions
      ? columnProportions.reduce((sum, n) => sum + n, 0) + 3
      : this._displayFields.length + 3;
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

  private _displayFieldsNodeCache = new ComputedCache<EntityObject, ReactNode>();

  renderField(_model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const {
      fieldOrders,
      fieldEditors,
      skipOtherFields,
      onShowEntity,
      onHideEntity,
      canSaveEntity,
      canRefreshEntity,
      canReloadEntity,
      canDetachEntity,
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity
    } = this.props;
    const { isOpen, isTouched, activeId } = this.state;
    const dataSource = this.dataSource;
    const contentSchema = fieldSchema.RelatedContent;
    return isTouched && dataSource ? (
      <table
        className={cn("relation-field-accordion", {
          "relation-field-accordion--hidden": !isOpen
        })}
        cellSpacing="0"
        cellPadding="0"
      >
        <tbody>
          {dataSource.map(entity => {
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
                    className="relation-field-accordion__expander"
                    title={isOpen ? "Свернуть" : "Развернуть"}
                  >
                    <Icon icon={isOpen ? "caret-down" : "caret-right"} title={false} />
                  </td>
                  {this._displayFieldsNodeCache.getOrAdd(entity, () => (
                    <>
                      <td key={-1} className="relation-field-accordion__cell">
                        <EntityLink model={entity} contentSchema={contentSchema} />
                      </td>
                      {this._displayFields.map((displayField, i) => (
                        <td
                          key={i}
                          colSpan={this._columnProportions ? this._columnProportions[i] : 1}
                          className="relation-field-accordion__cell"
                        >
                          {displayField(entity)}
                        </td>
                      ))}
                    </>
                  ))}
                  <td className="relation-field-accordion__controls">
                    {isOpen && (
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
                    )}
                  </td>
                </tr>
                <tr className="relation-field-accordion__main">
                  <td
                    className={cn("relation-field-accordion__body", {
                      "relation-field-accordion__body--open": isOpen
                    })}
                    colSpan={this.getBodyColSpan()}
                  >
                    {isOpen && (
                      <EntityEditor
                        model={entity}
                        contentSchema={fieldSchema.RelatedContent}
                        fieldOrders={fieldOrders}
                        fieldEditors={fieldEditors}
                        skipOtherFields={skipOtherFields}
                        onShowEntity={onShowEntity}
                        onHideEntity={onHideEntity}
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

interface PrivateProps extends RelationFieldAccordionProps {
  fieldSchema: MultiRelationFieldSchema;
}
