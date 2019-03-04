import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import TextAreaAutosize from "react-textarea-autosize";
import { ValidatableInput } from "./AbstractControls";
var TextArea = /** @class */ (function(_super) {
  tslib_1.__extends(TextArea, _super);
  function TextArea() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  TextArea.prototype.handleChange = function(e) {
    _super.prototype.handleChange.call(this, e);
    this.setState({ editValue: e.target.value });
  };
  TextArea.prototype.handleBlur = function(e) {
    _super.prototype.handleBlur.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model[name] = this.state.editValue;
  };
  TextArea.prototype.render = function() {
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
    return React.createElement(
      TextAreaAutosize,
      tslib_1.__assign(
        {
          useCacheForDOMMeasurements: true,
          className: cn("bp3-input bp3-fill editor-textarea", className),
          minRows: 2,
          maxRows: 6,
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
    TextArea.prototype,
    "handleBlur",
    null
  );
  TextArea = tslib_1.__decorate([observer], TextArea);
  return TextArea;
})(ValidatableInput);
export { TextArea };
//# sourceMappingURL=TextArea.js.map
