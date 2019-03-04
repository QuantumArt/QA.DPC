import * as tslib_1 from "tslib";
import "react-select/dist/react-select.css";
import React from "react";
import ReactSelect from "react-select";
import { action, isObservableArray } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";
import { isStateTreeNode, getIdentifier, getSnapshot } from "mobx-state-tree";
var Select = /** @class */ (function(_super) {
  tslib_1.__extends(Select, _super);
  function Select() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  Select.prototype.handleChange = function(selection) {
    _super.prototype.handleChange.call(this, selection);
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      required = _a.required,
      clearable = _a.clearable;
    if (Array.isArray(selection)) {
      if (!required || clearable || selection.length > 0) {
        model[name] = selection.map(function(option) {
          return option.value;
        });
      }
    } else if (selection) {
      model[name] = selection.value;
    } else if (!required || clearable) {
      model[name] = null;
    }
  };
  Select.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      onFocus = _a.onFocus,
      onChange = _a.onChange,
      onBlur = _a.onBlur,
      required = _a.required,
      multiple = _a.multiple,
      _b = _a.placeholder,
      placeholder = _b === void 0 ? "" : _b,
      props = tslib_1.__rest(_a, [
        "model",
        "name",
        "onFocus",
        "onChange",
        "onBlur",
        "required",
        "multiple",
        "placeholder"
      ]);
    var value = model[name];
    if ((multiple || props.multi) && isObservableArray(value)) {
      if (isStateTreeNode(value)) {
        value = getSnapshot(value);
      } else {
        value = value.peek();
      }
    } else {
      if (isStateTreeNode(value)) {
        value = getIdentifier(value);
      }
    }
    return React.createElement(
      ReactSelect,
      tslib_1.__assign(
        {
          value: value,
          onFocus: this.handleFocus,
          onChange: this.handleChange,
          onBlur: this.handleBlur,
          placeholder: placeholder,
          clearable: !required,
          multi: multiple
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
    Select.prototype,
    "handleChange",
    null
  );
  Select = tslib_1.__decorate([observer], Select);
  return Select;
})(ValidatableControl);
export { Select };
//# sourceMappingURL=Select.js.map
