import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { Validator } from "ProductEditor/Packages/mst-validation-mixin";
import { EntityObject } from "ProductEditor/Models/EditorDataModels";
import { RelationFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { isNullOrWhiteSpace } from "ProductEditor/Utils/TypeChecks";
import { ComputedCache } from "ProductEditor/Utils/WeakCache";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector,
  EntityComparer
} from "../AbstractFieldEditor";
import "./RelationFieldTags.scss";

export interface RelationFieldTagsProps extends FieldEditorProps {
  /** Предикат фильтрации связанных статей при отображении в списке */
  filterItems?: (item: EntityObject) => boolean;
  /** Функция realtime валидации каждой связанной статьи */
  validateItems?: Validator;
  /** Функция сравнения связанных статей для сортировки */
  sortItems?: EntityComparer;
  /** Селектор поля для сравнения связанных статей для сортировки */
  sortItemsBy?: string | FieldSelector;
  /** Селектор поля для отображения текста */
  displayField?: string | FieldSelector;
  /** Render Callback для добавления дополнительных кнопок-действий в редактор поля-связи */
  relationActions?: () => ReactNode;
}

export abstract class AbstractRelationFieldTags extends AbstractRelationFieldEditor<
  RelationFieldTagsProps
> {
  protected _displayField: FieldSelector;
  protected _isHalfSize = false;
  protected _validationCache = new ComputedCache<EntityObject, string>();

  constructor(props: RelationFieldTagsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    this._displayField = this.makeDisplayFieldSelector(props.displayField, fieldSchema);
  }

  protected getTitle(entity: EntityObject) {
    const title = this._displayField(entity);
    return isNullOrWhiteSpace(title) ? "..." : title;
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        xl={this._isHalfSize ? 6 : 12}
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col
            xl={this._isHalfSize ? 4 : 2}
            md={3}
            className="field-editor__label field-editor__label--small"
          >
            {this.renderLabel(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={this._isHalfSize ? 4 : 2} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}
