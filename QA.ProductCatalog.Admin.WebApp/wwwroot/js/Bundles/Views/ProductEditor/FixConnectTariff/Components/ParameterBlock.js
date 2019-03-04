import * as tslib_1 from "tslib";
import React, { Component } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Intent, Checkbox, Alignment } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { Validate } from "mst-validation-mixin";
import { InputNumber, Select } from "Components/FormControls/FormControls";
import { hasUniqueTariffDirection } from "../Utils/ParameterValidators";
var ParameterBlock = /** @class */ (function(_super) {
  tslib_1.__extends(ParameterBlock, _super);
  function ParameterBlock() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ParameterBlock.prototype.toggleBaseParamModifier = function(modifier) {
    var parameter = this.props.parameter;
    // @ts-ignore
    parameter.setTouched("BaseParameterModifiers");
    if (parameter.BaseParameterModifiers.includes(modifier)) {
      parameter.BaseParameterModifiers.remove(modifier);
    } else {
      parameter.BaseParameterModifiers.push(modifier);
    }
  };
  ParameterBlock.prototype.render = function() {
    var _this = this;
    var _a = this.props,
      allParameters = _a.allParameters,
      parameter = _a.parameter,
      numValueSchema = _a.numValueSchema,
      unitOptions = _a.unitOptions,
      baseParamModifiers = _a.baseParamModifiers;
    return React.createElement(
      Col,
      {
        md: 12,
        className: cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": parameter.hasVisibleErrors()
        })
      },
      React.createElement(
        Row,
        null,
        React.createElement(
          Col,
          { xl: 2, md: 3, className: "field-editor__label" },
          React.createElement(
            "label",
            {
              htmlFor: "param_" + parameter._ClientId,
              title: parameter.BaseParameter && parameter.BaseParameter.Alias
            },
            React.createElement(
              "span",
              {
                className: cn("field-editor__label-text", {
                  "field-editor__label-text--edited": parameter.isEdited(),
                  "field-editor__label-text--invalid": parameter.hasVisibleErrors()
                })
              },
              parameter.Title,
              ":"
            )
          )
        ),
        React.createElement(
          Col,
          { xl: 2, md: 3 },
          React.createElement(InputNumber, {
            id: "param_" + parameter._ClientId,
            model: parameter,
            name: "NumValue",
            isInteger: numValueSchema.IsInteger,
            intent: parameter.isEdited("NumValue")
              ? Intent.PRIMARY
              : Intent.NONE
          })
        ),
        React.createElement(
          Col,
          { xl: 2, md: 3 },
          React.createElement(Select, {
            model: parameter,
            name: "Unit",
            options: unitOptions,
            className: cn({
              "bp3-intent-primary": parameter.isEdited("Unit")
            })
          })
        ),
        baseParamModifiers &&
          React.createElement(
            Col,
            { md: true },
            baseParamModifiers.map(function(modifier) {
              return React.createElement(Checkbox, {
                key: modifier._ClientId,
                inline: true,
                label: modifier.Alias,
                alignIndicator: Alignment.RIGHT,
                className: cn({
                  "parameter-block__modifier--edited": parameter.isEdited(
                    "BaseParameterModifiers"
                  )
                }),
                checked: parameter.BaseParameterModifiers.includes(modifier),
                onChange: function() {
                  return _this.toggleBaseParamModifier(modifier);
                }
              });
            })
          )
      ),
      React.createElement(
        Row,
        null,
        React.createElement(Col, {
          xl: 2,
          md: 3,
          className: "field-editor__label"
        }),
        React.createElement(
          Col,
          { md: true },
          React.createElement(Validate, {
            model: parameter,
            name: "BaseParameter",
            errorClassName: "bp3-form-helper-text",
            rules: hasUniqueTariffDirection(parameter, allParameters)
          })
        )
      )
    );
  };
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    ParameterBlock.prototype,
    "toggleBaseParamModifier",
    null
  );
  ParameterBlock = tslib_1.__decorate([observer], ParameterBlock);
  return ParameterBlock;
})(Component);
export { ParameterBlock };
//# sourceMappingURL=ParameterBlock.js.map
