import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { Validator } from "mst-validation-mixin";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { EntityObject } from "Models/EditorDataModels";
import { ComputedCache } from "Utils/WeakCache";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector,
  EntityComparer,
  HighlightMode
} from "../AbstractFieldEditor";
import "./RelationFieldTable.scss";

export interface RelationFieldTableProps extends FieldEditorProps {
  /** Предикат фильтрации связанных статей при отображении в таблице */
  filterItems?: (item: EntityObject) => boolean;
  /** Функция подсветки каждой связанной статьи */
  highlightItems?: (item: EntityObject) => HighlightMode;
  /** Функция realtime валидации каждой связанной статьи */
  validateItems?: Validator;
  /** Функция сравнения связанных статей для сортировки */
  sortItems?: EntityComparer;
  /** Селектор поля для сравнения связанных статей для сортировки */
  sortItemsBy?: string | FieldSelector;
  /** Селектор полей для отображения в таблице */
  displayFields?: (string | FieldSelector)[];
  /** Render Callback для добавления дополнительных кнопок-действий в редактор поля-связи */
  relationActions?: () => ReactNode;
}

export abstract class AbstractRelationFieldTable extends AbstractRelationFieldEditor<
  RelationFieldTableProps
> {
  protected _displayFields: FieldSelector[];
  protected _validationCache = new ComputedCache<EntityObject, string>();

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    this._displayFields = this.makeDisplayFieldsSelectors(props.displayFields, fieldSchema);
  }

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
          {this.renderField(model, fieldSchema)}
        </Row>
      </Col>
    );
  }
}
