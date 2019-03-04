import * as tslib_1 from "tslib";
import React, { createRef } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";
var InputFile = /** @class */ (function(_super) {
  tslib_1.__extends(InputFile, _super);
  function InputFile() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.inputRef = createRef();
    _this.handleClear = function(e) {
      e.preventDefault();
      var _a = _this.props,
        model = _a.model,
        name = _a.name;
      model.setTouched(name, true);
      _this.inputRef.current.value = null;
      model[name] = null;
    };
    return _this;
  }
  InputFile.prototype.handleChange = function(e) {
    _super.prototype.handleChange.call(this, e);
    var _a = this.props,
      model = _a.model,
      name = _a.name;
    var files = e.target.files;
    if (files.length > 0) {
      model[name] = files[0].name;
    } else {
      model[name] = null;
    }
  };
  InputFile.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      className = _a.className,
      readOnly = _a.readOnly,
      onFocus = _a.onFocus,
      onChange = _a.onChange,
      onBlur = _a.onBlur,
      placeholder = _a.placeholder,
      props = tslib_1.__rest(_a, [
        "model",
        "name",
        "className",
        "readOnly",
        "onFocus",
        "onChange",
        "onBlur",
        "placeholder"
      ]);
    var fileName = model[name];
    return React.createElement(
      "label",
      {
        className: cn("bp3-file-input bp3-fill editor-input-file", className),
        title: fileName
      },
      React.createElement(
        "input",
        tslib_1.__assign(
          {
            ref: this.inputRef,
            type: readOnly ? "text" : "file",
            readOnly: readOnly,
            onFocus: this.handleFocus,
            onChange: this.handleChange,
            onBlur: this.handleBlur
          },
          props
        )
      ),
      React.createElement(
        "span",
        {
          className: cn("bp3-file-upload-input", {
            "editor-input-file__placeholder": !fileName
          })
        },
        fileName ? fileName : placeholder,
        React.createElement("span", {
          className: "editor-input-file__clear bp3-icon bp3-icon-cross",
          title: "\u041E\u0447\u0438\u0441\u0442\u0438\u0442\u044C",
          onClick: this.handleClear
        })
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
    InputFile.prototype,
    "handleChange",
    null
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    InputFile.prototype,
    "handleClear",
    void 0
  );
  InputFile = tslib_1.__decorate([observer], InputFile);
  return InputFile;
})(ValidatableControl);
export { InputFile };
//# sourceMappingURL=InputFile.js.map
