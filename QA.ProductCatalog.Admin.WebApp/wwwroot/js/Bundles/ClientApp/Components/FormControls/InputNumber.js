import * as tslib_1 from "tslib";
import React from "react";
import { NumericInput } from "@blueprintjs/core";
import { action, runInAction } from "mobx";
import { observer } from "mobx-react";
import { ValidatableInput } from "./AbstractControls";
var InputNumber = /** @class */ (function(_super) {
  tslib_1.__extends(InputNumber, _super);
  function InputNumber() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.handleValueChange = function(valueAsNumber, valueAsString) {
      _super.prototype.handleChange.call(_this, valueAsNumber, valueAsString);
      var _a = _this.props,
        model = _a.model,
        name = _a.name,
        isInteger = _a.isInteger;
      var _b = _this.state,
        hasFocus = _b.hasFocus,
        editValue = _b.editValue;
      if (
        valueAsString === "" ||
        valueAsString === "-" ||
        (isInteger
          ? Number.isSafeInteger(valueAsNumber)
          : Number.isFinite(valueAsNumber))
      ) {
        if (hasFocus) {
          _this.setState({ editValue: valueAsString });
        } else {
          runInAction(function() {
            model[name] = valueAsNumber;
          });
        }
      } else {
        _this.setState({ editValue: editValue });
      }
    };
    return _this;
  }
  InputNumber.prototype.handleBlur = function(e) {
    _super.prototype.handleBlur.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    var editValue = this.state.editValue;
    if (editValue === "") {
      model[name] = null;
    } else if (editValue !== "-") {
      model[name] = Number(editValue);
    }
  };
  InputNumber.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      onFocus = _a.onFocus,
      onChange = _a.onChange,
      onBlur = _a.onBlur,
      isInteger = _a.isInteger,
      props = tslib_1.__rest(_a, [
        "model",
        "name",
        "onFocus",
        "onChange",
        "onBlur",
        "isInteger"
      ]);
    var _b = this.state,
      hasFocus = _b.hasFocus,
      editValue = _b.editValue;
    var inputValue = hasFocus
      ? editValue
      : model[name] != null
      ? model[name]
      : "";
    return React.createElement(
      NumericInput,
      tslib_1.__assign(
        {
          value: inputValue,
          onFocus: this.handleFocus,
          onValueChange: this.handleValueChange,
          onBlur: this.handleBlur,
          fill: true
        },
        props
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
    InputNumber.prototype,
    "handleBlur",
    null
  );
  InputNumber = tslib_1.__decorate([observer], InputNumber);
  return InputNumber;
})(ValidatableInput);
export { InputNumber };
//# sourceMappingURL=InputNumber.js.map
