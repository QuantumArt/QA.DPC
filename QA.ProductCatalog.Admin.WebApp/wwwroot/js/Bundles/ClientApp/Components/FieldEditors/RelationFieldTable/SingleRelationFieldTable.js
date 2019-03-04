import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTable } from "./AbstractRelationFieldTable";
import { EntityLink } from "Components/ArticleEditor/EntityLink";
/** Отображение поля-связи в виде таблицы */
var SingleRelationFieldTable = /** @class */ (function(_super) {
  tslib_1.__extends(SingleRelationFieldTable, _super);
  function SingleRelationFieldTable() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.detachEntity = function(e) {
      e.stopPropagation();
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
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
  SingleRelationFieldTable.prototype.renderField = function(
    model,
    fieldSchema
  ) {
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
      { md: true },
      React.createElement(
        RelationFieldMenu,
        { onSelect: !this._readonly && this.selectRelation },
        relationActions && relationActions()
      ),
      this.renderValidation(model, fieldSchema),
      entity &&
        React.createElement(
          "div",
          { className: "relation-field-table" },
          React.createElement(
            "div",
            { className: "relation-field-table__table" },
            React.createElement(
              "div",
              {
                className: cn("relation-field-table__row", {
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
              this._displayFields.map(function(displayField, i) {
                return React.createElement(
                  "div",
                  { key: i, className: "relation-field-table__cell" },
                  displayField(entity)
                );
              }),
              React.createElement(
                "div",
                { key: -2, className: "relation-field-table__controls" },
                !this._readonly &&
                  React.createElement(
                    Button,
                    {
                      minimal: true,
                      small: true,
                      rightIcon: "remove",
                      intent: Intent.DANGER,
                      title:
                        "\u0423\u0434\u0430\u043B\u0438\u0442\u044C \u0441\u0432\u044F\u0437\u044C \u0441 \u0442\u0435\u043A\u0443\u0449\u0435\u0439 \u0441\u0442\u0430\u0442\u044C\u0435\u0439",
                      onClick: this.detachEntity
                    },
                    "\u041E\u0442\u0432\u044F\u0437\u0430\u0442\u044C"
                  )
              )
            )
          )
        )
    );
  };
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    SingleRelationFieldTable.prototype,
    "detachEntity",
    void 0
  );
  SingleRelationFieldTable = tslib_1.__decorate(
    [observer],
    SingleRelationFieldTable
  );
  return SingleRelationFieldTable;
})(AbstractRelationFieldTable);
export { SingleRelationFieldTable };
//# sourceMappingURL=SingleRelationFieldTable.js.map
