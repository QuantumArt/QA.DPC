import React from "react";
import { Col } from "react-flexbox-grid";
import { Options, Option } from "react-select";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import cn from "classnames";
import {
  RelationFieldSchema,
  isMultiRelationField,
  PreloadingMode,
  PreloadingState
} from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { isArray, isObject, isString, isNullOrWhiteSpace } from "Utils/TypeChecks";
import { Select } from "Components/FormControls/FormControls";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";

export interface RelationFieldSelectProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
}

const optionsCache = new WeakMap<RelationFieldSchema, Options>();

@consumer
@observer
export class RelationFieldSelect extends AbstractRelationFieldEditor<RelationFieldSelectProps> {
  private readonly _getOption: (article: EntityObject) => Option;
  private readonly _multiple: boolean;

  constructor(props: RelationFieldSelectProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayField =
      props.displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");
    const getTitle = isString(displayField) ? article => article[displayField] : displayField;
    this._getOption = article => {
      const title = getTitle(article);
      return {
        value: article._ClientId,
        label: isNullOrWhiteSpace(title) ? "..." : title
      };
    };
    this._multiple = isMultiRelationField(props.fieldSchema);
  }

  private getCachedOptions(): Options {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;

    let options = optionsCache.get(fieldSchema);
    if (options) {
      return options;
    }

    if (
      fieldSchema.PreloadingMode === PreloadingMode.Lazy &&
      fieldSchema.PreloadingState !== PreloadingState.Done
    ) {
      if (fieldSchema.PreloadingState === PreloadingState.NotStarted) {
        this._relationController.preloadRelationArticles(fieldSchema);
      }

      const relation = this.props.model[fieldSchema.FieldName];
      if (isArray(relation)) {
        return relation.map(this._getOption);
      }
      if (isObject(relation)) {
        return [this._getOption(relation)];
      }
      return [];
    }

    options = fieldSchema.PreloadedArticles.map(this._getOption);
    optionsCache.set(fieldSchema, options);

    return options;
  }

  render() {
    return this.getCachedOptions().length > 0 ? super.render() : null;
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const options = this.getCachedOptions();

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
