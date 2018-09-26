import React, { Component } from "react";
import cn from "classnames";
import { IObservableArray } from "mobx";
import { observer } from "mobx-react";
import { Intent } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { Options } from "react-select";
import { asc } from "Utils/Array/Sort";
import { InputNumber, Select } from "Components/FormControls/FormControls";
import { FieldEditorProps } from "Components/FieldEditors/AbstractFieldEditor";
import { RelationFieldSchema, NumericFieldSchema } from "Models/EditorSchemaModels";
import { LinkParameter, ProductParameter, BaseParameter } from "../ProductEditorSchema";

type Parameter = ProductParameter | LinkParameter;

interface ParamenterField {
  Title: string;
  Alias: string;
}

interface ParametersFieldSetProps extends FieldEditorProps {
  fields: ParamenterField[];
}

const optionsCache = new WeakMap<RelationFieldSchema, Options>();

@observer
export class ParametersFieldSet extends Component<ParametersFieldSetProps> {
  private getParameters(): IObservableArray<Parameter> {
    const { model, fieldSchema } = this.props;
    return model[fieldSchema.FieldName];
  }

  componentDidMount() {}

  componentWillUnmount() {}

  // unitFieldSchema.PreloadingMode должно быть PreloadingMode.Eager
  private getCachedOptions(): Options {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    const unitFieldSchema = fieldSchema.RelatedContent.Fields.Unit as RelationFieldSchema;

    let options = optionsCache.get(fieldSchema);
    if (options) {
      return options;
    }

    options = unitFieldSchema.PreloadedArticles.map((baseParamenter: BaseParameter) => ({
      value: baseParamenter._ClientId,
      label: baseParamenter.Title
    }));

    optionsCache.set(fieldSchema, options);

    return options;
  }

  render() {
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    const numValueSchema = fieldSchema.RelatedContent.Fields.NumValue as NumericFieldSchema;

    const parameters = this.getParameters();
    const options = this.getCachedOptions();

    return parameters
      .slice()
      .sort(asc(p => p.Title))
      .map(parameter => (
        <Col md={12} key={parameter._ClientId} className="field-editor__block pt-form-group">
          <Row>
            <Col xl={2} md={3} className="field-editor__label">
              <label
                htmlFor={"p" + parameter._ClientId}
                title={parameter.BaseParameter && parameter.BaseParameter.Alias}
                className={cn("field-editor__label-text", {
                  "field-editor__label-text--edited": parameter.isEdited()
                })}
              >
                {parameter.Title}
              </label>
            </Col>
            <Col xl={2} md={3}>
              <InputNumber
                id={"p" + parameter._ClientId}
                model={parameter}
                name="NumValue"
                isInteger={numValueSchema.IsInteger}
                intent={parameter.isEdited("NumValue") ? Intent.PRIMARY : Intent.NONE}
              />
            </Col>
            <Col xl={2} md={3}>
              <Select
                model={parameter}
                name="Unit"
                options={options}
                className={cn({
                  "pt-intent-primary": parameter.isEdited("Unit")
                })}
              />
            </Col>
          </Row>
        </Col>
      ));
  }
}
