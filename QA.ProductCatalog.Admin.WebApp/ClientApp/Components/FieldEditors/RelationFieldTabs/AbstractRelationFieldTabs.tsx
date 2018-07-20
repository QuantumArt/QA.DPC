import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { RelationController } from "Services/RelationController";
import { isString } from "Utils/TypeChecks";
import { RenderArticle, FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldTabs.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

export interface RelationFieldTabsProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  fieldEditors?: FieldsConfig;
  skipOtherFields?: boolean;
  filterItems?: (item: ArticleObject) => boolean;
  collapsed?: boolean;
  vertical?: boolean;
  borderless?: boolean;
  children?: RenderArticle | ReactNode;
}

export abstract class AbstractRelationFieldTabs extends AbstractFieldEditor<
  RelationFieldTabsProps
> {
  @inject protected _dataContext: DataContext;
  @inject protected _relationController: RelationController;
  protected _displayField: FieldSelector;

  constructor(props: RelationFieldTabsProps, context?: any) {
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

  protected abstract renderControls(
    model: ArticleObject | ExtensionObject,
    fieldSchema: FieldSchema
  ): ReactNode;

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
            <label htmlFor={this.id} title={fieldSchema.FieldDescription || fieldSchema.FieldName}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && (
                <span className="field-editor__label-required">&nbsp;*</span>
              )}
            </label>
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
