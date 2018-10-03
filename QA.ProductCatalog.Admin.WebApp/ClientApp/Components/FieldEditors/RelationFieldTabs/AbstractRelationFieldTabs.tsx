import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { isString, isNullOrWhiteSpace } from "Utils/TypeChecks";
import { CloneController } from "Services/CloneController";
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
  canCreateEntity?: boolean;
  canClonePrototype?: boolean;
  canSaveEntity?: boolean;
  canRefreshEntity?: boolean;
  canReloadEntity?: boolean;
  canRemoveEntity?: boolean;
  canPublishEntity?: boolean;
  canCloneEntity?: boolean;
  canSelectRelation?: boolean;
  canClearRelation?: boolean;
  canReloadRelation?: boolean;
}

export abstract class AbstractRelationFieldTabs extends AbstractRelationFieldEditor<
  RelationFieldTabsProps
> {
  static defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true
  };

  @inject protected _dataContext: DataContext;
  @inject protected _cloneController: CloneController;
  protected _displayField: FieldSelector;

  constructor(props: RelationFieldTabsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayField =
      props.displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");
    this._displayField = isString(displayField) ? article => article[displayField] : displayField;
  }

  protected getTitle(article: EntityObject) {
    const title = this._displayField(article);
    return isNullOrWhiteSpace(title) ? "..." : title;
  }

  protected abstract renderControls(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        md={12}
        className={cn("field-editor__block pt-form-group", {
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
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
