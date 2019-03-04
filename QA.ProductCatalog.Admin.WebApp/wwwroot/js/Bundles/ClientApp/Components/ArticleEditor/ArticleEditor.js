import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import {
  FieldExactTypes,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  isRelationField,
  PreloadingMode
} from "Models/EditorSchemaModels";
import {
  ExtensionFieldEditor,
  StringFieldEditor,
  BooleanFieldEditor,
  NumericFieldEditor,
  DateFieldEditor,
  TimeFieldEditor,
  DateTimeFieldEditor,
  FileFieldEditor,
  TextFieldEditor,
  ClassifierFieldEditor,
  EnumFieldEditor,
  RelationFieldSelect,
  RelationFieldAccordion,
  RelationFieldCheckList,
  RelationFieldForm
} from "Components/FieldEditors/FieldEditors";
import { asc } from "Utils/Array";
import { isFunction, isObject } from "Utils/TypeChecks";
import "./ArticleEditor.scss";
/**
 * Настройка компонентов-редакторов для полей-связей по имени контента связи.
 * Переопределяются с помощью @see ArticleEditorProps.fieldEditors
 */
var RelationsConfig = /** @class */ (function() {
  function RelationsConfig() {}
  return RelationsConfig;
})();
export { RelationsConfig };
function isContentsConfig(field) {
  return isObject(field) && Object.values(field).every(isObject);
}
/** Не отображать редактор поля */
export var IGNORE = Symbol("IGNORE");
var AbstractEditor = /** @class */ (function(_super) {
  tslib_1.__extends(AbstractEditor, _super);
  function AbstractEditor(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this._editorBlocks = [];
    _this.prepareFields();
    return _this;
  }
  AbstractEditor.prototype.prepareFields = function() {
    var _this = this;
    var _a = this.props,
      contentSchema = _a.contentSchema,
      fieldOrders = _a.fieldOrders,
      children = _a.children;
    if (isFunction(children) && children.length === 0) {
      return;
    }
    var fieldOrderByName = {};
    if (fieldOrders) {
      fieldOrders
        .slice()
        .reverse()
        .forEach(function(fieldName, i) {
          fieldOrderByName[fieldName] = -(i + 1);
        });
    }
    // TODO: cache by contentSchema and memoize by props.fieldEditors
    Object.values(contentSchema.Fields)
      .sort(
        asc(function(field) {
          return fieldOrderByName[field.FieldName] || field.FieldOrder;
        })
      )
      .forEach(function(fieldSchema) {
        if (_this.shouldIncludeField(fieldSchema)) {
          _this.prepareFieldBlock(fieldSchema);
          if (isExtensionField(fieldSchema)) {
            _this.prepareContentsBlock(fieldSchema);
          }
        }
      });
  };
  AbstractEditor.prototype.shouldIncludeField = function(fieldSchema) {
    var _a = this.props,
      skipOtherFields = _a.skipOtherFields,
      fieldEditors = _a.fieldEditors;
    return (
      !skipOtherFields ||
      (fieldEditors && fieldEditors.hasOwnProperty(fieldSchema.FieldName))
    );
  };
  AbstractEditor.prototype.prepareFieldBlock = function(fieldSchema) {
    var _a = this.props,
      model = _a.model,
      fieldEditors = _a.fieldEditors;
    var fieldName = fieldSchema.FieldName;
    if (fieldEditors && fieldEditors.hasOwnProperty(fieldName)) {
      var field = fieldEditors[fieldName];
      if (isFunction(field)) {
        this._editorBlocks.push({
          fieldSchema: fieldSchema,
          FieldEditor: field
        });
      } else if (field !== IGNORE) {
        model[fieldName] = field;
      }
      return;
    }
    if (fieldSchema.IsReadOnly) {
      return;
    }
    if (isRelationField(fieldSchema)) {
      var contentName = fieldSchema.RelatedContent.ContentName;
      if (this._relationsConfig.hasOwnProperty(contentName)) {
        var field = this._relationsConfig[contentName];
        if (isFunction(field)) {
          this._editorBlocks.push({
            fieldSchema: fieldSchema,
            FieldEditor: field
          });
        } else if (field !== IGNORE) {
          model[fieldName] = field;
        }
        return;
      }
    }
    this._editorBlocks.push({
      fieldSchema: fieldSchema,
      FieldEditor: this.getDefaultFieldEditor(fieldSchema)
    });
  };
  AbstractEditor.prototype.prepareContentsBlock = function(fieldSchema) {
    var fieldEditors = this.props.fieldEditors;
    var contentsConfig =
      fieldEditors && fieldEditors[fieldSchema.FieldName + "_Extension"];
    if (isContentsConfig(contentsConfig)) {
      this._editorBlocks.push({
        fieldSchema: fieldSchema,
        contentsConfig: contentsConfig
      });
    } else if (contentsConfig !== IGNORE) {
      this._editorBlocks.push({ fieldSchema: fieldSchema });
    }
  };
  AbstractEditor.prototype.getDefaultFieldEditor = function(fieldSchema) {
    if (isSingleRelationField(fieldSchema)) {
      if (fieldSchema.PreloadingMode !== PreloadingMode.None) {
        return RelationFieldSelect;
      }
      return RelationFieldForm;
    }
    if (isMultiRelationField(fieldSchema)) {
      if (fieldSchema.PreloadingMode !== PreloadingMode.None) {
        return RelationFieldCheckList;
      }
      return RelationFieldAccordion;
    }
    if (isExtensionField(fieldSchema)) {
      return ExtensionFieldEditor;
    }
    switch (fieldSchema.FieldType) {
      case FieldExactTypes.String:
        return StringFieldEditor;
      case FieldExactTypes.Numeric:
        return NumericFieldEditor;
      case FieldExactTypes.Boolean:
        return BooleanFieldEditor;
      case FieldExactTypes.Date:
        return DateFieldEditor;
      case FieldExactTypes.Time:
        return TimeFieldEditor;
      case FieldExactTypes.DateTime:
        return DateTimeFieldEditor;
      case FieldExactTypes.File:
      case FieldExactTypes.Image:
        return FileFieldEditor;
      case FieldExactTypes.Textbox:
      case FieldExactTypes.VisualEdit:
        return TextFieldEditor;
      case FieldExactTypes.Classifier:
        return ClassifierFieldEditor;
      case FieldExactTypes.StringEnum:
        return EnumFieldEditor;
    }
    throw new Error(
      "Unsupported field type FieldExactTypes." + fieldSchema.FieldType
    );
  };
  AbstractEditor.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      fieldOrders = _a.fieldOrders;
    return this._editorBlocks
      .map(function(_a) {
        var fieldSchema = _a.fieldSchema,
          FieldEditor = _a.FieldEditor,
          contentsConfig = _a.contentsConfig;
        var fieldName = fieldSchema.FieldName;
        if (FieldEditor) {
          return React.createElement(FieldEditor, {
            key: fieldName,
            model: model,
            fieldSchema: fieldSchema
          });
        }
        var contentName = model[fieldName];
        if (contentName) {
          var extensionModel =
            model["" + fieldName + ArticleObject._Extension][contentName];
          var extensionSchema = fieldSchema.ExtensionContents[contentName];
          var extensionFields = contentsConfig && contentsConfig[contentName];
          return React.createElement(ArticleEditor, {
            key: fieldName + "_" + contentName,
            model: extensionModel,
            fieldOrders: fieldOrders,
            contentSchema: extensionSchema,
            fieldEditors: extensionFields
          });
        }
        return null;
      })
      .filter(Boolean);
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", RelationsConfig)],
    AbstractEditor.prototype,
    "_relationsConfig",
    void 0
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", []),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    AbstractEditor.prototype,
    "prepareFields",
    null
  );
  return AbstractEditor;
})(Component);
export { AbstractEditor };
/** Компонент для отображения и редактирования произвольной статьи */
var ArticleEditor = /** @class */ (function(_super) {
  tslib_1.__extends(ArticleEditor, _super);
  function ArticleEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ArticleEditor = tslib_1.__decorate([observer], ArticleEditor);
  return ArticleEditor;
})(AbstractEditor);
export { ArticleEditor };
//# sourceMappingURL=ArticleEditor.js.map
