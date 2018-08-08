import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { RelationController } from "Services/RelationController";
import { isString } from "Utils/TypeChecks";
import { RenderEntity, FieldsConfig } from "Components/ArticleEditor/EntityEditor";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldTabs.scss";

export interface RelationFieldTabsProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  fieldEditors?: FieldsConfig;
  skipOtherFields?: boolean;
  filterItems?: (item: EntityObject) => boolean;
  collapsed?: boolean;
  vertical?: boolean;
  borderless?: boolean;
  children?: RenderEntity | ReactNode;
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
      displayField = (fieldSchema as RelationFieldSchema).RelatedContent.DisplayFieldName ||
        (() => "")
    } = this.props;
    this._displayField = isString(displayField) ? article => article[displayField] : displayField;
  }

  protected getTitle(article: EntityObject) {
    const title = this._displayField(article);
    return title != null && !/^\s*$/.test(title) ? title : "...";
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
