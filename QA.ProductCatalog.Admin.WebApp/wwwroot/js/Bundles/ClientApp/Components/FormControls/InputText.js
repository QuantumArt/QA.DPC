import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import MaskedInput from "react-text-mask";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableInput } from "./AbstractControls";
var InputText = /** @class */ (function(_super) {
  tslib_1.__extends(InputText, _super);
  function InputText() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  InputText.prototype.handleChange = function(e) {
    _super.prototype.handleChange.call(this, e);
    this.setState({ editValue: e.target.value });
  };
  InputText.prototype.handleBlur = function(e) {
    _super.prototype.handleBlur.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model[name] = this.state.editValue;
  };
  InputText.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      className = _a.className,
      onFocus = _a.onFocus,
      onChange = _a.onChange,
      onBlur = _a.onBlur,
      props = tslib_1.__rest(_a, [
        "model",
        "name",
        "className",
        "onFocus",
        "onChange",
        "onBlur"
      ]);
    var _b = this.state,
      hasFocus = _b.hasFocus,
      editValue = _b.editValue;
    var inputValue = hasFocus
      ? editValue
      : model[name] != null
      ? model[name]
      : "";
    return props.mask
      ? React.createElement(
          MaskedInput,
          tslib_1.__assign(
            {
              className: cn("bp3-input bp3-fill", className),
              value: inputValue,
              onFocus: this.handleFocus,
              onChange: this.handleChange,
              onBlur: this.handleBlur
            },
            props
          )
        )
      : React.createElement(
          "input",
          tslib_1.__assign(
            {
              type: "text",
              className: cn("bp3-input bp3-fill", className),
              value: inputValue,
              onFocus: this.handleFocus,
              onChange: this.handleChange,
              onBlur: this.handleBlur
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
    InputText.prototype,
    "handleBlur",
    null
  );
  InputText = tslib_1.__decorate([observer], InputText);
  return InputText;
})(ValidatableInput);
export { InputText };
//# sourceMappingURL=InputText.js.map
