import * as tslib_1 from "tslib";
import React from "react";
import { RadioGroup as PtRadioGroup } from "@blueprintjs/core";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";
var RadioGroup = /** @class */ (function(_super) {
  tslib_1.__extends(RadioGroup, _super);
  function RadioGroup() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  RadioGroup.prototype.handleChange = function(e) {
    _super.prototype.handleChange.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model[name] = e.target.value;
  };
  RadioGroup.prototype.render = function() {
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
      PtRadioGroup,
      tslib_1.__assign(
        { onChange: this.handleChange, selectedValue: model[name] },
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
    RadioGroup.prototype,
    "handleChange",
    null
  );
  RadioGroup = tslib_1.__decorate([observer], RadioGroup);
  return RadioGroup;
})(ValidatableControl);
export { RadioGroup };
//# sourceMappingURL=RadioGroup.js.map
