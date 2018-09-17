import React from "react";
import { Col } from "react-flexbox-grid";
import { Options } from "react-select";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import cn from "classnames";
import {
  RelationFieldSchema,
  isMultiRelationField,
  PreloadingMode,
  PreloadingState
} from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isArray, isObject, isString } from "Utils/TypeChecks";
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

const optionsCache = new WeakMap<RelationFieldSchema, Options>();

@consumer
@observer
export class RelationFieldSelect extends AbstractRelationFieldEditor<RelationFieldSelectProps> {
  private readonly _multiple: boolean;

  constructor(props: RelationFieldSelectProps, context?: any) {
    super(props, context);
    this._multiple = isMultiRelationField(props.fieldSchema);
  }

  private getOptions() {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;

    let options = optionsCache.get(fieldSchema);
    if (options) {
      return options;
    }

    const displayField =
      this.props.displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");

    const getTitle = isString(displayField) ? article => article[displayField] : displayField;

    const getOption = article => {
      const title = getTitle(article);
      return {
        value: article._ClientId,
        label: title != null && !/^\s*$/.test(title) ? title : "..."
      };
    };

    if (
      fieldSchema.PreloadingMode === PreloadingMode.Lazy &&
      fieldSchema.PreloadingState !== PreloadingState.Done
    ) {
      if (fieldSchema.PreloadingState === PreloadingState.NotStarted) {
        this._relationController.preloadRelationArticles(fieldSchema);
      }

      const relation = this.props.model[fieldSchema.FieldName];
      if (isArray(relation)) {
        return relation.map(getOption);
      }
      if (isObject(relation)) {
        return [getOption(relation)];
      }
      return [];
    }

    options = fieldSchema.PreloadedArticles.map(getOption);
    optionsCache.set(fieldSchema, options);

    return options;
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const options = this.getOptions();

    return (
      <Col xl md={6}>
        <Select
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
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
