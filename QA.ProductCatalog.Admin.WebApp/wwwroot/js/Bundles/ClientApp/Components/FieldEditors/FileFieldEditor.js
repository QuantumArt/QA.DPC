import * as tslib_1 from "tslib";
import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { FieldExactTypes } from "Models/EditorSchemaModels";
import { InputFile } from "Components/FormControls/FormControls";
import { AbstractFieldEditor } from "./AbstractFieldEditor";
import { inject } from "react-ioc";
import { FileController } from "Services/FileController";
import { action } from "mobx";
var FileFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(FileFieldEditor, _super);
  function FileFieldEditor() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.previewImage = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
      var fileName = model[fieldSchema.FieldName];
      if (fileName) {
        _this._fileController.previewImage(model, fieldSchema);
      }
    };
    _this.downloadFile = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
      var fileName = model[fieldSchema.FieldName];
      if (fileName) {
        _this._fileController.downloadFile(model, fieldSchema);
      }
    };
    _this.selectFile = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        readonly = _a.readonly,
        customSubFolder = _a.customSubFolder;
      if (!readonly) {
        _this._fileController.selectFile(model, fieldSchema, customSubFolder);
      }
    };
    return _this;
  }
  FileFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { xl: true, md: 6, className: "file-field-editor" },
      React.createElement(
        "div",
        { className: "bp3-control-group bp3-fill" },
        React.createElement(InputFile, {
          id: this._id,
          model: model,
          name: fieldSchema.FieldName,
          placeholder: this.getPlaceholder(),
          disabled: this._readonly,
          readOnly: true,
          accept:
            fieldSchema.FieldType === FieldExactTypes.Image ? "image/*" : "",
          className: cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })
        }),
        fieldSchema.FieldType === FieldExactTypes.Image &&
          React.createElement("button", {
            className: "bp3-button bp3-icon-media",
            title: "\u041F\u0440\u043E\u0441\u043C\u043E\u0442\u0440",
            onClick: this.previewImage
          }),
        React.createElement("button", {
          className: "bp3-button bp3-icon-cloud-download",
          title: "\u0421\u043A\u0430\u0447\u0430\u0442\u044C",
          onClick: this.downloadFile
        }),
        React.createElement("button", {
          className: "bp3-button bp3-icon-folder-close",
          title: "\u0411\u0438\u0431\u043B\u0438\u043E\u0442\u0435\u043A\u0430",
          onClick: this.selectFile
        })
      )
    );
  };
  FileFieldEditor.prototype.getPlaceholder = function() {
    var customSubFolder = this.props.customSubFolder;
    return customSubFolder
      ? customSubFolder.endsWith("/")
        ? customSubFolder
        : customSubFolder + "/"
      : "";
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", FileController)],
    FileFieldEditor.prototype,
    "_fileController",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    FileFieldEditor.prototype,
    "previewImage",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    FileFieldEditor.prototype,
    "downloadFile",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    FileFieldEditor.prototype,
    "selectFile",
    void 0
  );
  FileFieldEditor = tslib_1.__decorate([observer], FileFieldEditor);
  return FileFieldEditor;
})(AbstractFieldEditor);
export { FileFieldEditor };
//# sourceMappingURL=FileFieldEditor.js.map
