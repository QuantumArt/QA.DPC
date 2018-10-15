import React, { Component } from "react";
import cn from "classnames";
import { observer } from "mobx-react";
import { Intent } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { Options } from "react-select";
import { Validate } from "mst-validation-mixin";
import { InputNumber, Select } from "Components/FormControls/FormControls";
import { NumericFieldSchema } from "Models/EditorSchemaModels";
import { LinkParameter, ProductParameter } from "../TypeScriptSchema";
import { hasUniqueTariffDirection } from "../Utils/Validators";

type Parameter = ProductParameter | LinkParameter;

interface ParameterBlockProps {
  parameter: Parameter;
  allParameters: Parameter[];
  numValueSchema: NumericFieldSchema;
  unitOptions: Options;
}

@observer
export class ParameterBlock extends Component<ParameterBlockProps> {
  render() {
    const { allParameters, parameter, numValueSchema, unitOptions } = this.props;
    return (
      <Col
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": parameter.hasVisibleErrors()
        })}
      >
        <Row>
          <Col xl={2} md={3} className="field-editor__label">
            <label
              htmlFor={"param_" + parameter._ClientId}
              title={parameter.BaseParameter && parameter.BaseParameter.Alias}
            >
              <span
                className={cn("field-editor__label-text", {
                  "field-editor__label-text--edited": parameter.isEdited(),
                  "field-editor__label-text--invalid": parameter.hasVisibleErrors()
                })}
              >
                {parameter.Title}:
              </span>
            </label>
          </Col>
          <Col xl={2} md={3}>
            <InputNumber
              id={"param_" + parameter._ClientId}
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
              options={unitOptions}
              className={cn({
                "bp3-intent-primary": parameter.isEdited("Unit")
              })}
            />
          </Col>
        </Row>
        <Row>
          <Col xl={2} md={3} className="field-editor__label" />
          <Col md>
            <Validate
              model={parameter}
              name="BaseParameter"
              errorClassName="bp3-form-helper-text"
              rules={hasUniqueTariffDirection(parameter, allParameters)}
            />
          </Col>
        </Row>
      </Col>
    );
  }
}
