import * as tslib_1 from "tslib";
import { Component } from "react";
import { transaction } from "mobx";
import { isPlainObject } from "Utils/TypeChecks";
var AbstractControl = /** @class */ (function(_super) {
  tslib_1.__extends(AbstractControl, _super);
  function AbstractControl(props, context) {
    var _this = _super.call(this, props, context) || this;
    var model = props.model,
      name = props.name;
    if (!(name in model)) {
      var modelName = isPlainObject(model)
        ? "Object"
        : Object.getPrototypeOf(model).name;
      throw new TypeError(
        "Object [" + modelName + '] does not have property "' + name + '"'
      );
    }
    _this.handleFocus = _this.handleFocus.bind(_this);
    _this.handleChange = _this.handleChange.bind(_this);
    _this.handleBlur = _this.handleBlur.bind(_this);
    return _this;
  }
  AbstractControl.prototype.handleFocus = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    var _a;
    if (this.props.onFocus) {
      (_a = this.props).onFocus.apply(_a, tslib_1.__spread(args));
    }
  };
  AbstractControl.prototype.handleChange = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    var _a;
    if (this.props.onChange) {
      (_a = this.props).onChange.apply(_a, tslib_1.__spread(args));
    }
  };
  AbstractControl.prototype.handleBlur = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    var _a;
    if (this.props.onBlur) {
      (_a = this.props).onBlur.apply(_a, tslib_1.__spread(args));
    }
  };
  return AbstractControl;
})(Component);
export { AbstractControl };
var ValidatableControl = /** @class */ (function(_super) {
  tslib_1.__extends(ValidatableControl, _super);
  function ValidatableControl() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ValidatableControl.prototype.handleFocus = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    _super.prototype.handleFocus.apply(this, tslib_1.__spread(args));
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    transaction(function() {
      model.setFocus(name, true);
      model.setTouched(name, true);
    });
  };
  ValidatableControl.prototype.handleChange = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    _super.prototype.handleChange.apply(this, tslib_1.__spread(args));
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model.setTouched(name, true);
  };
  ValidatableControl.prototype.handleBlur = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    _super.prototype.handleBlur.apply(this, tslib_1.__spread(args));
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    model.setFocus(name, false);
  };
  return ValidatableControl;
})(AbstractControl);
export { ValidatableControl };
var ValidatableInput = /** @class */ (function(_super) {
  tslib_1.__extends(ValidatableInput, _super);
  function ValidatableInput() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.state = {
      hasFocus: false,
      editValue: ""
    };
    return _this;
  }
  ValidatableInput.prototype.handleFocus = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    _super.prototype.handleFocus.apply(this, tslib_1.__spread(args));
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    var editValue = model[name];
    if (editValue == null) {
      editValue = "";
    }
    this.setState({ hasFocus: true, editValue: editValue });
  };
  ValidatableInput.prototype.handleBlur = function() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
      args[_i] = arguments[_i];
    }
    _super.prototype.handleBlur.apply(this, tslib_1.__spread(args));
    this.setState({ hasFocus: false });
  };
  return ValidatableInput;
})(ValidatableControl);
export { ValidatableInput };
//# sourceMappingURL=AbstractControls.js.map
