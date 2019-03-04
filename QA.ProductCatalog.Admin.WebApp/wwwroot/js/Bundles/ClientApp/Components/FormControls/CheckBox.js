import * as tslib_1 from "tslib";
import React from "react";
import { Checkbox } from "@blueprintjs/core";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";
var CheckBox = /** @class */ (function(_super) {
  tslib_1.__extends(CheckBox, _super);
  function CheckBox() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  CheckBox.prototype.handleChange = function(e) {
    _super.prototype.handleChange.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model[name] = !!e.target.checked;
  };
  CheckBox.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      onFocus = _a.onFocus,
      onChange = _a.onChange,
      onBlur = _a.onBlur,
      props = tslib_1.__rest(_a, [
        "model",
        "name",
        "onFocus",
        "onChange",
        "onBlur"
      ]);
    return React.createElement(
      Checkbox,
      tslib_1.__assign(
        {
          checked: !!model[name],
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
    CheckBox.prototype,
    "handleChange",
    null
  );
  CheckBox = tslib_1.__decorate([observer], CheckBox);
  return CheckBox;
})(ValidatableControl);
export { CheckBox };
//# sourceMappingURL=CheckBox.js.map
