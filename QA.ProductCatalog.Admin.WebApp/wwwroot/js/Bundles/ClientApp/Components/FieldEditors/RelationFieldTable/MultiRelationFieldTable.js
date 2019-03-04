import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action, computed, untracked } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTable } from "./AbstractRelationFieldTable";
import { EntityLink } from "Components/ArticleEditor/EntityLink";
/** Отображение поля-связи в виде таблицы */
var MultiRelationFieldTable = /** @class */ (function(_super) {
  tslib_1.__extends(MultiRelationFieldTable, _super);
  function MultiRelationFieldTable(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this.clearRelations = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
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
  Object.defineProperty(MultiRelationFieldTable.prototype, "dataSource", {
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
  MultiRelationFieldTable.prototype.detachEntity = function(e, entity) {
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
  MultiRelationFieldTable.prototype.renderField = function(model, fieldSchema) {
    var _this = this;
    var _a = this.props,
      highlightItems = _a.highlightItems,
      relationActions = _a.relationActions;
    var dataSource = this.dataSource;
    var isEmpty = !dataSource || dataSource.length === 0;
    return React.createElement(
      Col,
      { md: true },
      React.createElement(
        RelationFieldMenu,
        {
          onSelect: !this._readonly && this.selectRelations,
          onClear: !this._readonly && !isEmpty && this.clearRelations
        },
        relationActions && relationActions()
      ),
      this.renderValidation(model, fieldSchema),
      dataSource &&
        React.createElement(
          "div",
          { className: "relation-field-table" },
          React.createElement(
            "div",
            { className: "relation-field-table__table" },
            dataSource.map(function(entity) {
              var highlightMode = highlightItems(entity);
              var highlight = highlightMode === 1; /* Highlight */
              var shade = highlightMode === 2; /* Shade */
              var itemError = _this._validationCache.get(entity);
              return React.createElement(
                "div",
                {
                  key: entity._ClientId,
                  className: cn("relation-field-table__row", {
                    "relation-field-table__row--highlight": highlight,
                    "relation-field-table__row--shade": shade,
                    "relation-field-table__row--invalid": !!itemError
                  }),
                  title: itemError
                },
                React.createElement(
                  "div",
                  { key: -1, className: "relation-field-table__cell" },
                  React.createElement(EntityLink, {
                    model: entity,
                    contentSchema: fieldSchema.RelatedContent
                  })
                ),
                _this._displayFields.map(function(displayField, i) {
                  return React.createElement(
                    "div",
                    { key: i, className: "relation-field-table__cell" },
                    displayField(entity)
                  );
                }),
                React.createElement(
                  "div",
                  { key: -2, className: "relation-field-table__controls" },
                  !_this._readonly &&
                    React.createElement(
                      Button,
                      {
                        minimal: true,
                        small: true,
                        rightIcon: "remove",
                        intent: Intent.DANGER,
                        title:
                          "\u0423\u0434\u0430\u043B\u0438\u0442\u044C \u0441\u0432\u044F\u0437\u044C \u0441 \u0442\u0435\u043A\u0443\u0449\u0435\u0439 \u0441\u0442\u0430\u0442\u044C\u0435\u0439",
                        onClick: function(e) {
                          return _this.detachEntity(e, entity);
                        }
                      },
                      "\u041E\u0442\u0432\u044F\u0437\u0430\u0442\u044C"
                    )
                )
              );
            })
          )
        )
    );
  };
  MultiRelationFieldTable.defaultProps = {
    filterItems: function() {
      return true;
    },
    highlightItems: function() {
      return 0 /* None */;
    }
  };
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    MultiRelationFieldTable.prototype,
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
    MultiRelationFieldTable.prototype,
    "detachEntity",
    null
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    MultiRelationFieldTable.prototype,
    "clearRelations",
    void 0
  );
  MultiRelationFieldTable = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    MultiRelationFieldTable
  );
  return MultiRelationFieldTable;
})(AbstractRelationFieldTable);
export { MultiRelationFieldTable };
//# sourceMappingURL=MultiRelationFieldTable.js.map
