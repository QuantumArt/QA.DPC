import * as tslib_1 from "tslib";
import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import {
  InputText,
  RadioGroup,
  Select,
  InputNumber,
  CheckBox,
  DatePicker
} from "Components/FormControls/FormControls";
import { pattern } from "Utils/Validators";
import { Intent } from "@blueprintjs/core";
import { AbstractFieldEditor } from "./AbstractFieldEditor";
import { Validate } from "mst-validation-mixin";
export { FileFieldEditor } from "./FileFieldEditor";
export { TextFieldEditor } from "./TextFieldEditor";
export {
  RelationFieldCheckList
} from "./RelationFieldCheckList/RelationFieldCheckList";
export { RelationFieldSelect } from "./RelationFieldSelect/RelationFieldSelect";
export { RelationFieldForm } from "./RelationFieldForm/RelationFieldForm";
export { RelationFieldTabs } from "./RelationFieldTabs/RelationFieldTabs";
export {
  RelationFieldAccordion
} from "./RelationFieldAccordion/RelationFieldAccordion";
export {
  SingleRelationFieldTags,
  MultiRelationFieldTags,
  RelationFieldTags
} from "./RelationFieldTags/RelationFieldTags";
export {
  SingleRelationFieldTable,
  MultiRelationFieldTable,
  RelationFieldTable
} from "./RelationFieldTable/RelationFieldTable";
var StringFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(StringFieldEditor, _super);
  function StringFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  StringFieldEditor.prototype.renderValidation = function(model, fieldSchema) {
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(Validate, {
        model: model,
        name: fieldSchema.FieldName,
        silent: true,
        rules: fieldSchema.RegexPattern && pattern(fieldSchema.RegexPattern)
      }),
      _super.prototype.renderValidation.call(this, model, fieldSchema)
    );
  };
  StringFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { xl: true, md: 6 },
      React.createElement(InputText, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        disabled: this._readonly,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  StringFieldEditor = tslib_1.__decorate([observer], StringFieldEditor);
  return StringFieldEditor;
})(AbstractFieldEditor);
export { StringFieldEditor };
var NumericFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(NumericFieldEditor, _super);
  function NumericFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  NumericFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { xl: 4, md: 3 },
      React.createElement(InputNumber, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        isInteger: fieldSchema.IsInteger,
        disabled: this._readonly,
        intent: model.hasVisibleErrors(fieldSchema.FieldName)
          ? Intent.DANGER
          : model.isEdited(fieldSchema.FieldName)
          ? Intent.PRIMARY
          : Intent.NONE
      })
    );
  };
  NumericFieldEditor = tslib_1.__decorate([observer], NumericFieldEditor);
  return NumericFieldEditor;
})(AbstractFieldEditor);
export { NumericFieldEditor };
var BooleanFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(BooleanFieldEditor, _super);
  function BooleanFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  BooleanFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { md: true },
      React.createElement(CheckBox, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        disabled: this._readonly,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  BooleanFieldEditor = tslib_1.__decorate([observer], BooleanFieldEditor);
  return BooleanFieldEditor;
})(AbstractFieldEditor);
export { BooleanFieldEditor };
var DateFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(DateFieldEditor, _super);
  function DateFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  DateFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { xl: 4, md: 3 },
      React.createElement(DatePicker, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        type: "date",
        disabled: this._readonly,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  DateFieldEditor = tslib_1.__decorate([observer], DateFieldEditor);
  return DateFieldEditor;
})(AbstractFieldEditor);
export { DateFieldEditor };
var TimeFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(TimeFieldEditor, _super);
  function TimeFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  TimeFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { xl: 4, md: 3 },
      React.createElement(DatePicker, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        type: "time",
        disabled: this._readonly,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  TimeFieldEditor = tslib_1.__decorate([observer], TimeFieldEditor);
  return TimeFieldEditor;
})(AbstractFieldEditor);
export { TimeFieldEditor };
var DateTimeFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(DateTimeFieldEditor, _super);
  function DateTimeFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  DateTimeFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { xl: 4, md: 3 },
      React.createElement(DatePicker, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        disabled: this._readonly,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  DateTimeFieldEditor = tslib_1.__decorate([observer], DateTimeFieldEditor);
  return DateTimeFieldEditor;
})(AbstractFieldEditor);
export { DateTimeFieldEditor };
var ClassifierFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(ClassifierFieldEditor, _super);
  function ClassifierFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ClassifierFieldEditor.prototype.renderField = function(model, fieldSchema) {
    var value = model[fieldSchema.FieldName];
    var options = value ? [{ value: value, label: value }] : [];
    return React.createElement(
      Col,
      { xl: true, md: 6 },
      React.createElement(Select, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        options: options,
        required: true,
        disabled: true,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  ClassifierFieldEditor = tslib_1.__decorate([observer], ClassifierFieldEditor);
  return ClassifierFieldEditor;
})(AbstractFieldEditor);
export { ClassifierFieldEditor };
var EnumFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(EnumFieldEditor, _super);
  function EnumFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  EnumFieldEditor.prototype.renderField = function(model, fieldSchema) {
    var options = fieldSchema.Items.map(function(item) {
      return { value: item.Value, label: item.Alias };
    });
    return fieldSchema.ShowAsRadioButtons
      ? React.createElement(
          Col,
          { md: true },
          React.createElement(RadioGroup, {
            model: model,
            name: fieldSchema.FieldName,
            options: options,
            disabled: this._readonly,
            className: cn({
              "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
              "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
            })
          })
        )
      : React.createElement(
          Col,
          { xl: true, md: 6 },
          React.createElement(Select, {
            id: this._id,
            model: model,
            name: fieldSchema.FieldName,
            options: options,
            required: fieldSchema.IsRequired,
            disabled: this._readonly,
            className: cn({
              "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
              "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
            })
          })
        );
  };
  EnumFieldEditor = tslib_1.__decorate([observer], EnumFieldEditor);
  return EnumFieldEditor;
})(AbstractFieldEditor);
export { EnumFieldEditor };
var ExtensionFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(ExtensionFieldEditor, _super);
  function ExtensionFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ExtensionFieldEditor.prototype.renderField = function(model, fieldSchema) {
    var options = Object.values(fieldSchema.ExtensionContents).map(function(
      contentSchema
    ) {
      return {
        value: contentSchema.ContentName,
        label: contentSchema.ContentTitle || contentSchema.ContentName
      };
    });
    var disabled =
      this._readonly || (!fieldSchema.Changeable && model._ServerId > 0);
    return React.createElement(
      Col,
      { xl: true, md: 6 },
      React.createElement(Select, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        options: options,
        required: fieldSchema.IsRequired,
        disabled: disabled,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  ExtensionFieldEditor = tslib_1.__decorate([observer], ExtensionFieldEditor);
  return ExtensionFieldEditor;
})(AbstractFieldEditor);
export { ExtensionFieldEditor };
//# sourceMappingURL=FieldEditors.js.map
