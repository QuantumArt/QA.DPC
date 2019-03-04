import * as tslib_1 from "tslib";
import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { ComputedCache } from "Utils/WeakCache";
import { AbstractRelationFieldEditor } from "../AbstractFieldEditor";
import "./RelationFieldTable.scss";
var AbstractRelationFieldTable = /** @class */ (function(_super) {
  tslib_1.__extends(AbstractRelationFieldTable, _super);
  function AbstractRelationFieldTable(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this._validationCache = new ComputedCache();
    var fieldSchema = props.fieldSchema;
    _this._displayFields = _this.makeDisplayFieldsSelectors(
      props.displayFields,
      fieldSchema
    );
    return _this;
  }
  AbstractRelationFieldTable.prototype.render = function() {
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
          {
            xl: 2,
            md: 3,
            className: "field-editor__label field-editor__label--small"
          },
          this.renderLabel(model, fieldSchema)
        ),
        this.renderField(model, fieldSchema)
      )
    );
  };
  return AbstractRelationFieldTable;
})(AbstractRelationFieldEditor);
export { AbstractRelationFieldTable };
//# sourceMappingURL=AbstractRelationFieldTable.js.map
