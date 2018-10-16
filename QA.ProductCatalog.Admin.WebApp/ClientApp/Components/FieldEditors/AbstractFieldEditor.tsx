import React, { Component, ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { inject } from "react-ioc";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { Validator, Validate } from "mst-validation-mixin";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import {
  FieldSchema,
  RelationFieldSchema,
  UpdatingMode,
  FieldExactTypes,
  MultiRelationFieldSchema
} from "Models/EditorSchemaModels";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { RelationController } from "Services/RelationController";
import { isArray, isString } from "Utils/TypeChecks";
import { required } from "Utils/Validators";
import { newUid } from "Utils/Common";
import "./FieldEditors.scss";
import { asc } from "ClientApp/Utils/Array/Sort";

export interface FieldEditorProps {
  model: ArticleObject;
  fieldSchema: FieldSchema;
  validate?: Validator | Validator[];
  readonly?: boolean;
}

export abstract class AbstractFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends Component<P> {
  protected _id = `_${newUid()}`;
  protected _readonly = this.props.readonly || this.props.fieldSchema.IsReadOnly;

  protected abstract renderField(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

  protected renderValidation(model: ArticleObject, fieldSchema: FieldSchema): ReactNode {
    const { validate } = this.props;
    const rules = [];
    if (validate) {
      if (isArray(validate)) {
        rules.push(...validate);
      } else {
        rules.push(validate);
      }
    }
    if (fieldSchema.IsRequired) {
      rules.push(required);
    }
    return (
      <Validate
        model={model}
        name={fieldSchema.FieldName}
        errorClassName="bp3-form-helper-text"
        rules={rules}
      />
    );
  }

  protected renderLabel(model: ArticleObject, fieldSchema: FieldSchema) {
    return (
      <label htmlFor={this._id} title={fieldSchema.FieldName}>
        {fieldSchema.IsRequired && <span className="field-editor__label-required">*&nbsp;</span>}
        <span
          className={cn("field-editor__label-text", {
            "field-editor__label-text--edited": model.isEdited(fieldSchema.FieldName),
            "field-editor__label-text--invalid": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        >
          {fieldSchema.FieldTitle || fieldSchema.FieldName}:
        </span>
        {fieldSchema.FieldDescription && (
          <>
            &nbsp;<Icon icon="help" title={fieldSchema.FieldDescription} />
          </>
        )}
      </label>
    );
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        xl={6}
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={4} md={3} className="field-editor__label">
            {this.renderLabel(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={4} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}

export type FieldSelector<T = ReactNode> = (model: ArticleObject) => T;
export type EntityComparer = (first: EntityObject, second: EntityObject) => -1 | 0 | 1;

export abstract class AbstractRelationFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends AbstractFieldEditor<P> {
  @inject protected _relationController: RelationController;

  constructor(props: P, context?: any) {
    super(props, context);
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    this._readonly =
      this._readonly ||
      (fieldSchema.FieldType === FieldExactTypes.M2ORelation &&
        fieldSchema.UpdatingMode === UpdatingMode.Ignore);
  }

  protected makeDisplayFieldSelector<T = ReactNode>(
    displayField: string | FieldSelector<T>,
    fieldSchema: RelationFieldSchema
  ): FieldSelector<T> {
    const expression = displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");
    return isString(expression) ? entity => entity[expression] : expression;
  }

  protected makeDisplayFieldsSelectors<T = ReactNode>(
    displayFields: (string | FieldSelector<T>)[],
    fieldSchema: RelationFieldSchema
  ): FieldSelector<T>[] {
    const expressions = displayFields || fieldSchema.DisplayFieldNames || [];
    return expressions.map(field => (isString(field) ? entity => entity[field] : field));
  }

  protected makeEntityComparer(
    sortItems: string | FieldSelector | EntityComparer,
    fieldSchema: MultiRelationFieldSchema
  ): EntityComparer {
    const expression = sortItems || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    if (isString(expression)) {
      return asc(entity => entity[expression]);
    }
    if (expression.length === 1) {
      return asc(expression as FieldSelector);
    }
    if (expression.length === 2) {
      return expression as EntityComparer;
    }
    throw new Error("Invalid `sortItems` parameter");
  }
}

export interface ExpandableFieldEditorProps extends FieldEditorProps {
  // ArticleEditor props
  fieldOrders?: string[];
  fieldEditors?: FieldsConfig;
  skipOtherFields?: boolean;
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
  onShowEntity?(entity: EntityObject): void;
  onHideEntity?(entity: EntityObject): void;
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
