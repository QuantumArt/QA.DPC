import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { EditorController } from "Services/EditorController";
import { ArticleController } from "Services/ArticleController";
import { CloneController } from "Services/CloneController";
import { isString } from "Utils/TypeChecks";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";
import "./RelationFieldAccordion.scss";

export interface RelationFieldAccordionProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  columnProportions?: number[];
  orderByField?: string | FieldSelector;
  fieldOrders?: string[];
  fieldEditors?: FieldsConfig;
  filterItems?: (item: EntityObject) => boolean;
  renderOnlyActiveSection?: boolean;
  collapsed?: boolean;
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

export abstract class AbstractRelationFieldAccordion extends AbstractRelationFieldEditor<
  RelationFieldAccordionProps
> {
  static defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true
  };

  @inject protected _dataContext: DataContext;
  @inject protected _articleController: ArticleController;
  @inject protected _cloneController: CloneController;
  @inject protected _editorController: EditorController;
  protected _displayFields: FieldSelector[];

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayFields = props.displayFields || fieldSchema.DisplayFieldNames || [];
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
  }

  protected abstract renderControls(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

  protected getBodyColSpan() {
    const { columnProportions } = this.props;
    return columnProportions
      ? columnProportions.reduce((sum, n) => sum + n, 0) + 3
      : this._displayFields.length + 3;
  }

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
