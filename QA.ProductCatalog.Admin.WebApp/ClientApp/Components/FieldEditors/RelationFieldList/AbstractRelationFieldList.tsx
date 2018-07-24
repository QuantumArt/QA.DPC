import React, { MouseEvent } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { inject } from "react-ioc";
import { ArticleObject } from "Models/EditorDataModels";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationController } from "Services/RelationController";
import { isString } from "Utils/TypeChecks";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldList.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

export interface RelationFieldListProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  selectMultiple?: boolean;
  onClick?: (e: MouseEvent<HTMLElement>, article: ArticleObject) => void;
}

export abstract class AbstractRelationFieldList extends AbstractFieldEditor<
  RelationFieldListProps
> {
  @inject protected _relationController: RelationController;
  protected _displayField: FieldSelector;

  constructor(props: RelationFieldListProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayField = (fieldSchema as RelationFieldSchema).Content.DisplayFieldName || (() => "")
    } = this.props;
    this._displayField = isString(displayField) ? article => article[displayField] : displayField;
  }

  protected getTitle(article: ArticleObject) {
    const title = this._displayField(article);
    return title != null && !/^\s*$/.test(title) ? title : "...";
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
            {this.renderLabel(fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={2} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}
