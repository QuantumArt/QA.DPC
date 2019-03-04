import * as tslib_1 from "tslib";
import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { TextArea } from "Components/FormControls/FormControls";
import { AbstractFieldEditor } from "./AbstractFieldEditor";
var TextFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(TextFieldEditor, _super);
  function TextFieldEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  TextFieldEditor.prototype.renderField = function(model, fieldSchema) {
    return React.createElement(
      Col,
      { md: true },
      React.createElement(TextArea, {
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
  TextFieldEditor.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    return React.createElement(
      Col,
      {
        md: 12,
        className: cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      },
      React.createElement(
        Row,
        null,
        React.createElement(
          Col,
          { xl: 2, md: 3, className: "field-editor__label" },
          this.renderLabel(model, fieldSchema)
        ),
        this.renderField(model, fieldSchema)
      ),
      React.createElement(
        Row,
        null,
        React.createElement(Col, {
          xl: 2,
          md: 3,
          className: "field-editor__label"
        }),
        React.createElement(
          Col,
          { md: true },
          this.renderValidation(model, fieldSchema)
        )
      )
    );
  };
  TextFieldEditor = tslib_1.__decorate([observer], TextFieldEditor);
  return TextFieldEditor;
})(AbstractFieldEditor);
export { TextFieldEditor };
//# sourceMappingURL=TextFieldEditor.js.map
