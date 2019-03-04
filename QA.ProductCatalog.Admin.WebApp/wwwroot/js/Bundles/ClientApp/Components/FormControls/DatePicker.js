import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { LocaleContext } from "react-lazy-i18n";
import DateTime from "react-datetime";
import moment from "moment";
import "react-datetime/css/react-datetime.css";
import { ValidatableInput } from "./AbstractControls";
var DatePicker = /** @class */ (function(_super) {
  tslib_1.__extends(DatePicker, _super);
  function DatePicker() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  DatePicker.prototype.handleChange = function(editValue) {
    _super.prototype.handleChange.call(this, editValue);
    this.setState({ editValue: editValue });
  };
  DatePicker.prototype.handleBlur = function(e) {
    _super.prototype.handleBlur.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    var editValue = this.state.editValue;
    if (moment.isMoment(editValue)) {
      model[name] = editValue.toDate();
    } else if (editValue === "") {
      model[name] = null;
    }
  };
  DatePicker.prototype.render = function() {
    var _this = this;
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      className = _a.className,
      onFocus = _a.onFocus,
      onChange = _a.onChange,
      onBlur = _a.onBlur,
      id = _a.id,
      type = _a.type,
      placeholder = _a.placeholder,
      disabled = _a.disabled,
      readOnly = _a.readOnly,
      props = tslib_1.__rest(_a, [
        "model",
        "name",
        "className",
        "onFocus",
        "onChange",
        "onBlur",
        "id",
        "type",
        "placeholder",
        "disabled",
        "readOnly"
      ]);
    var _b = this.state,
      hasFocus = _b.hasFocus,
      editValue = _b.editValue;
    var inputValue = hasFocus
      ? editValue
      : model[name] != null
      ? model[name]
      : null;
    return React.createElement(
      "div",
      {
        className: cn(
          "bp3-input-group",
          { "bp3-fill": type !== "time" },
          className
        )
      },
      React.createElement(LocaleContext.Consumer, null, function(locale) {
        return React.createElement(
          DateTime,
          tslib_1.__assign(
            {
              className: "editor-datepicker",
              inputProps: {
                className: "bp3-input",
                id: id,
                placeholder: placeholder,
                disabled: disabled,
                readOnly: readOnly
              },
              locale: (locale && locale.slice(0, 2).toLowerCase()) || "en",
              dateFormat: type !== "time",
              timeFormat: type !== "date",
              value: inputValue,
              onFocus: _this.handleFocus,
              onChange: _this.handleChange,
              onBlur: _this.handleBlur
            },
            props
          )
        );
      }),
      React.createElement("span", {
        className: cn(
          "bp3-icon",
          type === "time" ? "bp3-icon-time" : "bp3-icon-calendar"
        )
      })
    );
  };
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    DatePicker.prototype,
    "handleBlur",
    null
  );
  DatePicker = tslib_1.__decorate([observer], DatePicker);
  return DatePicker;
})(ValidatableInput);
export { DatePicker };
//# sourceMappingURL=DatePicker.js.map
