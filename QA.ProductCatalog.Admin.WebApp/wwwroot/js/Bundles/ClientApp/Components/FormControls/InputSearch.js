import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";
var InputSearch = /** @class */ (function(_super) {
  tslib_1.__extends(InputSearch, _super);
  function InputSearch() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  InputSearch.prototype.handleChange = function(e) {
    _super.prototype.handleChange.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model[name] = e.target.value;
  };
  InputSearch.prototype.render = function() {
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
    var inputValue = model[name] != null ? model[name] : "";
    return React.createElement(
      "div",
      { className: cn("bp3-input-group", className) },
      React.createElement(
        "input",
        tslib_1.__assign(
          {
            type: "search",
            className: cn("bp3-input", className),
            value: inputValue,
            onFocus: this.handleFocus,
            onChange: this.handleChange,
            onBlur: this.handleBlur
          },
          props
        )
      ),
      React.createElement("span", { className: "bp3-icon bp3-icon-search" })
    );
  };
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    InputSearch.prototype,
    "handleChange",
    null
  );
  InputSearch = tslib_1.__decorate([observer], InputSearch);
  return InputSearch;
})(AbstractControl);
export { InputSearch };
//# sourceMappingURL=InputSearch.js.map
