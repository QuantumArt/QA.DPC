import React, { Component } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Intent, Checkbox, Alignment } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { Options } from "react-select";
import { Validate } from "ProductEditor/Packages/mst-validation-mixin";
import { InputNumber, Select } from "ProductEditor/Components/FormControls";
import { NumericFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { LinkParameter, ProductParameter, BaseParameterModifier } from "../TypeScriptSchema";
import { hasUniqueTariffDirection } from "../Utils/ParameterValidators";

type Parameter = ProductParameter | LinkParameter;

interface ParameterBlockProps {
  parameter: Parameter;
  allParameters: Parameter[];
  numValueSchema: NumericFieldSchema;
  unitOptions: Options;
  baseParamModifiers?: BaseParameterModifier[];
}

@observer
export class ParameterBlock extends Component<ParameterBlockProps> {
  @action
  toggleBaseParamModifier(modifier: BaseParameterModifier) {
    const { parameter } = this.props;
    // @ts-ignore
    parameter.setTouched("BaseParameterModifiers");
    if (parameter.BaseParameterModifiers.includes(modifier)) {
      parameter.BaseParameterModifiers.remove(modifier);
    } else {
      parameter.BaseParameterModifiers.push(modifier);
    }
  }

  render() {
    const {
      allParameters,
      parameter,
      numValueSchema,
      unitOptions,
      baseParamModifiers
    } = this.props;
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
          {baseParamModifiers && (
            <Col md>
              {baseParamModifiers.map(modifier => (
                <Checkbox
                  key={modifier._ClientId}
                  inline
                  label={modifier.Alias}
                  alignIndicator={Alignment.RIGHT}
                  className={cn({
                    "parameter-block__modifier--edited": parameter.isEdited(
                      "BaseParameterModifiers"
                    )
                  })}
                  checked={parameter.BaseParameterModifiers.includes(modifier)}
                  onChange={() => this.toggleBaseParamModifier(modifier)}
                />
              ))}
            </Col>
          )}
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
