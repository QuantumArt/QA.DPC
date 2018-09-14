import React from "react";
import { Col } from "react-flexbox-grid";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import cn from "classnames";
import { RelationFieldSchema, isMultiRelationField } from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { Select } from "Components/FormControls/FormControls";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";

export interface RelationFieldSelectProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
}

@consumer
@observer
export class RelationFieldSelect extends AbstractRelationFieldEditor<RelationFieldSelectProps> {
  readonly _options: { value: number; label: string }[];
  readonly _multiple: boolean;

  constructor(props: RelationFieldSelectProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayField = (fieldSchema as RelationFieldSchema).RelatedContent.DisplayFieldName ||
        (() => "")
    } = this.props;

    const getDisplayField = isString(displayField)
      ? article => article[displayField]
      : displayField;

    this._options = (fieldSchema as RelationFieldSchema).PreloadedArticles.map(article => {
      const title = getDisplayField(article);
      return {
        value: article._ClientId,
        label: title != null && !/^\s*$/.test(title) ? title : "..."
      };
    });

    this._multiple = isMultiRelationField(fieldSchema);
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    return (
      <Col xl md={6}>
        <Select
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          options={this._options}
          required={fieldSchema.IsRequired}
          multiple={this._multiple}
          disabled={fieldSchema.IsReadOnly}
          className={cn({
            "pt-intent-primary": model.isEdited(fieldSchema.FieldName),
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}
