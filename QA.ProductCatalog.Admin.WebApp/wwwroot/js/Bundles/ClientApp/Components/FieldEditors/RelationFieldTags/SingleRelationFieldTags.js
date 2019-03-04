import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { observer } from "mobx-react";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTags } from "./AbstractRelationFieldTags";
/** Отображение поля-связи в виде списка тегов */
var SingleRelationFieldTags = /** @class */ (function(_super) {
  tslib_1.__extends(SingleRelationFieldTags, _super);
  function SingleRelationFieldTags() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this._isHalfSize = true;
    _this.detachEntity = function(e) {
      e.stopPropagation();
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
      _this.setState({ isSelected: false });
      model[fieldSchema.FieldName] = null;
      model.setTouched(fieldSchema.FieldName, true);
    };
    _this.selectRelation = function() {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var _a, model, fieldSchema;
        return tslib_1.__generator(this, function(_b) {
          switch (_b.label) {
            case 0:
              (_a = this.props),
                (model = _a.model),
                (fieldSchema = _a.fieldSchema);
              return [
                4 /*yield*/,
                this._relationController.selectRelation(model, fieldSchema)
              ];
            case 1:
              _b.sent();
              return [2 /*return*/];
          }
        });
      });
    };
    return _this;
  }
  SingleRelationFieldTags.prototype.renderField = function(model, fieldSchema) {
    var _a = this.props,
      relationActions = _a.relationActions,
      validateItems = _a.validateItems;
    var entity = model[fieldSchema.FieldName];
    var itemError =
      entity &&
      validateItems &&
      this._validationCache.getOrAdd(entity, function() {
        return validateItems(entity);
      });
    return React.createElement(
      Col,
      { md: true, className: "relation-field-list__tags" },
      React.createElement(
        RelationFieldMenu,
        { onSelect: !this._readonly && this.selectRelation },
        relationActions && relationActions()
      ),
      entity &&
        React.createElement(
          "span",
          {
            className: cn("bp3-tag bp3-minimal", {
              "bp3-intent-danger": !!itemError
            }),
            title: itemError
          },
          this.getTitle(entity),
          !this._readonly &&
            React.createElement("button", {
              className: "bp3-tag-remove",
              title: "\u041E\u0442\u0432\u044F\u0437\u0430\u0442\u044C",
              onClick: this.detachEntity
            })
        )
    );
  };
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    SingleRelationFieldTags.prototype,
    "detachEntity",
    void 0
  );
  SingleRelationFieldTags = tslib_1.__decorate(
    [observer],
    SingleRelationFieldTags
  );
  return SingleRelationFieldTags;
})(AbstractRelationFieldTags);
export { SingleRelationFieldTags };
//# sourceMappingURL=SingleRelationFieldTags.js.map
