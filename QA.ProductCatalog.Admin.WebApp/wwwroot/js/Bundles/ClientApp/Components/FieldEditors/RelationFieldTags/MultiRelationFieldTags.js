import * as tslib_1 from "tslib";
import React, { Fragment } from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action, computed, untracked } from "mobx";
import { observer } from "mobx-react";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTags } from "./AbstractRelationFieldTags";
/** Отображение поля-связи в виде списка тегов */
var MultiRelationFieldTags = /** @class */ (function(_super) {
  tslib_1.__extends(MultiRelationFieldTags, _super);
  function MultiRelationFieldTags(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this.clearRelations = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
      _this.setState({ selectedIds: {} });
      model[fieldSchema.FieldName].replace([]);
      model.setTouched(fieldSchema.FieldName, true);
    };
    _this.selectRelations = function() {
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
                this._relationController.selectRelations(model, fieldSchema)
              ];
            case 1:
              _b.sent();
              return [2 /*return*/];
          }
        });
      });
    };
    var sortItems = props.sortItems,
      sortItemsBy = props.sortItemsBy;
    var fieldSchema = props.fieldSchema;
    _this._entityComparer = _this.makeEntityComparer(
      sortItems || sortItemsBy,
      fieldSchema
    );
    return _this;
  }
  Object.defineProperty(MultiRelationFieldTags.prototype, "dataSource", {
    get: function() {
      var _this = this;
      var _a = this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        filterItems = _a.filterItems,
        validateItems = _a.validateItems;
      var array = model[fieldSchema.FieldName];
      if (!array) {
        return array;
      }
      if (!validateItems) {
        return array.filter(filterItems).sort(this._entityComparer);
      }
      var head = [];
      var tail = [];
      array.filter(filterItems).forEach(function(entity) {
        var itemError = untracked(function() {
          return _this._validationCache.getOrAdd(entity, function() {
            return validateItems(entity);
          });
        });
        if (itemError) {
          head.push(entity);
        } else {
          tail.push(entity);
        }
      });
      head.sort(this._entityComparer);
      tail.sort(this._entityComparer);
      return head.concat(tail);
    },
    enumerable: true,
    configurable: true
  });
  MultiRelationFieldTags.prototype.detachEntity = function(e, entity) {
    e.stopPropagation();
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    var array = model[fieldSchema.FieldName];
    if (array) {
      array.remove(entity);
      model.setTouched(fieldSchema.FieldName, true);
    }
  };
  MultiRelationFieldTags.prototype.renderField = function(
    _model,
    _fieldSchema
  ) {
    var _this = this;
    var relationActions = this.props.relationActions;
    var dataSource = this.dataSource;
    var isEmpty = !dataSource || dataSource.length === 0;
    return React.createElement(
      Col,
      { md: true, className: "relation-field-list__tags" },
      React.createElement(
        RelationFieldMenu,
        {
          onSelect: !this._readonly && this.selectRelations,
          onClear: !this._readonly && !isEmpty && this.clearRelations
        },
        relationActions && relationActions()
      ),
      dataSource &&
        dataSource.map(function(entity) {
          var itemError = _this._validationCache.get(entity);
          return React.createElement(
            Fragment,
            { key: entity._ClientId },
            " ",
            React.createElement(
              "span",
              {
                className: cn("bp3-tag bp3-minimal", {
                  "bp3-intent-danger": !!itemError
                }),
                title: itemError
              },
              _this.getTitle(entity),
              !_this._readonly &&
                React.createElement("button", {
                  className: "bp3-tag-remove",
                  title: "\u041E\u0442\u0432\u044F\u0437\u0430\u0442\u044C",
                  onClick: function(e) {
                    return _this.detachEntity(e, entity);
                  }
                })
            )
          );
        })
    );
  };
  MultiRelationFieldTags.defaultProps = {
    filterItems: function() {
      return true;
    }
  };
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    MultiRelationFieldTags.prototype,
    "dataSource",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    MultiRelationFieldTags.prototype,
    "detachEntity",
    null
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    MultiRelationFieldTags.prototype,
    "clearRelations",
    void 0
  );
  MultiRelationFieldTags = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    MultiRelationFieldTags
  );
  return MultiRelationFieldTags;
})(AbstractRelationFieldTags);
export { MultiRelationFieldTags };
//# sourceMappingURL=MultiRelationFieldTags.js.map
