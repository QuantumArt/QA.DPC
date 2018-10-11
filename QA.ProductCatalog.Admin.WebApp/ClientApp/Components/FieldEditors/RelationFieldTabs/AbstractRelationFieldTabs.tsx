import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { isString, isNullOrWhiteSpace } from "Utils/TypeChecks";
import { CloneController } from "Services/CloneController";
import { EntityController } from "Services/EntityController";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";
import "./RelationFieldTabs.scss";

export interface RelationFieldTabsProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  fieldOrders?: string[];
  fieldEditors?: FieldsConfig;
  skipOtherFields?: boolean;
  filterItems?: (item: EntityObject) => boolean;
  renderOnlyActiveTab?: boolean;
  collapsed?: boolean;
  vertical?: boolean;
  className?: string;
  borderless?: boolean;
  // allowed actions
  canClonePrototype?: boolean;
  canCreateEntity?: boolean;
  canSaveEntity?: boolean;
  canRefreshEntity?: boolean;
  canReloadEntity?: boolean;
  canDetachEntity?: boolean;
  canRemoveEntity?: boolean;
  canPublishEntity?: boolean;
  canCloneEntity?: boolean;
  canSelectRelation?: boolean;
  canClearRelation?: boolean;
  canReloadRelation?: boolean;
  onCreateEntity?(createEntity: () => EntityObject): void;
  onClonePrototype?(clonePrototype: () => Promise<EntityObject>): void;
  onCloneEntity?(entity: EntityObject, cloneEntity: () => Promise<EntityObject>): void;
  onSaveEntity?(entity: EntityObject, saveEntity: () => Promise<void>): void;
  onRefreshEntity?(entity: EntityObject, refreshEntity: () => Promise<void>): void;
  onReloadEntity?(entity: EntityObject, reloadEntity: () => Promise<void>): void;
  onPublishEntity?(entity: EntityObject, publishEntity: () => Promise<void>): void;
  onRemoveEntity?(entity: EntityObject, removeEntity: () => Promise<void>): void;
  onDetachEntity?(entity: EntityObject, detachEntity: () => void): void;
  onSelectRelation?(selectRelation: () => Promise<void>): void;
  onReloadRelation?(relaoadRelation: () => Promise<void>): void;
  onClearRelation?(clearRelation: () => void): void;
}

const defaultRelationHandler = action => action();
const defaultEntityHandler = (_entity, action) => action();

export abstract class AbstractRelationFieldTabs extends AbstractRelationFieldEditor<
  RelationFieldTabsProps
> {
  static defaultProps = {
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

  @inject protected _dataContext: DataContext;
  @inject protected _cloneController: CloneController;
  @inject protected _entityController: EntityController;
  protected _displayField: FieldSelector;

  constructor(props: RelationFieldTabsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayField =
      props.displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");
    this._displayField = isString(displayField) ? entity => entity[displayField] : displayField;
  }

  protected getTitle(entity: EntityObject) {
    const title = this._displayField(entity);
    return isNullOrWhiteSpace(title) ? "..." : title;
  }

  protected abstract renderControls(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

  render() {
    const { model, fieldSchema } = this.props;
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
}
