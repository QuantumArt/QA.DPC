import * as tslib_1 from "tslib";
import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { isNullOrWhiteSpace } from "Utils/TypeChecks";
import { ComputedCache } from "Utils/WeakCache";
import { AbstractRelationFieldEditor } from "../AbstractFieldEditor";
import "./RelationFieldTags.scss";
var AbstractRelationFieldTags = /** @class */ (function(_super) {
  tslib_1.__extends(AbstractRelationFieldTags, _super);
  function AbstractRelationFieldTags(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this._isHalfSize = false;
    _this._validationCache = new ComputedCache();
    var fieldSchema = props.fieldSchema;
    _this._displayField = _this.makeDisplayFieldSelector(
      props.displayField,
      fieldSchema
    );
    return _this;
  }
  AbstractRelationFieldTags.prototype.getTitle = function(entity) {
    var title = this._displayField(entity);
    return isNullOrWhiteSpace(title) ? "..." : title;
  };
  AbstractRelationFieldTags.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    return React.createElement(
      Col,
      {
        xl: this._isHalfSize ? 6 : 12,
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
            xl: this._isHalfSize ? 4 : 2,
            md: 3,
            className: "field-editor__label field-editor__label--small"
          },
          this.renderLabel(model, fieldSchema)
        ),
        this.renderField(model, fieldSchema)
      ),
      React.createElement(
        Row,
        null,
        React.createElement(Col, {
          xl: this._isHalfSize ? 4 : 2,
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
  return AbstractRelationFieldTags;
})(AbstractRelationFieldEditor);
export { AbstractRelationFieldTags };
//# sourceMappingURL=AbstractRelationFieldTags.js.map
