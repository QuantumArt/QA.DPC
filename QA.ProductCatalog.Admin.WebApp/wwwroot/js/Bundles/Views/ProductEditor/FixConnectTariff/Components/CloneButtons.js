import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { MenuItem, Icon, Intent } from "@blueprintjs/core";
import { ActionController } from "Services/ActionController";
import { EntityController } from "Services/EntityController";
import { RelationController } from "Services/RelationController";
var CloneButtons = /** @class */ (function(_super) {
  tslib_1.__extends(CloneButtons, _super);
  function CloneButtons() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.cloneProduct = function(e) {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var _a, product, marketingProduct, fieldSchema, clonedProduct;
        return tslib_1.__generator(this, function(_b) {
          switch (_b.label) {
            case 0:
              e.stopPropagation();
              (_a = this.props),
                (product = _a.product),
                (marketingProduct = _a.marketingProduct),
                (fieldSchema = _a.fieldSchema);
              return [
                4 /*yield*/,
                this._entityController.cloneRelatedEntity(
                  marketingProduct,
                  fieldSchema,
                  product
                )
              ];
            case 1:
              clonedProduct = _b.sent();
              return [
                4 /*yield*/,
                this._actionController.executeCustomAction(
                  "product_editor",
                  clonedProduct,
                  fieldSchema.RelatedContent
                )
              ];
            case 2:
              _b.sent();
              return [2 /*return*/];
          }
        });
      });
    };
    _this.cloneProductPrototype = function(e) {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var _a, marketingProduct, fieldSchema, clonedProduct;
        return tslib_1.__generator(this, function(_b) {
          switch (_b.label) {
            case 0:
              e.stopPropagation();
              (_a = this.props),
                (marketingProduct = _a.marketingProduct),
                (fieldSchema = _a.fieldSchema);
              return [
                4 /*yield*/,
                this._relationController.cloneProductPrototype(
                  marketingProduct,
                  fieldSchema
                )
              ];
            case 1:
              clonedProduct = _b.sent();
              return [
                4 /*yield*/,
                this._actionController.executeCustomAction(
                  "product_editor",
                  clonedProduct,
                  fieldSchema.RelatedContent
                )
              ];
            case 2:
              _b.sent();
              return [2 /*return*/];
          }
        });
      });
    };
    return _this;
  }
  CloneButtons.prototype.render = function() {
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(MenuItem, {
        labelElement: React.createElement(Icon, { icon: "duplicate" }),
        intent: Intent.SUCCESS,
        onClick: this.cloneProduct,
        text:
          "\u041A\u043B\u043E\u043D\u0438\u0440\u043E\u0432\u0430\u0442\u044C",
        title:
          "\u041A\u043B\u043E\u043D\u0438\u0440\u043E\u0432\u0430\u0442\u044C \u0442\u0435\u043A\u0443\u0449\u0443\u044E \u0441\u0442\u0430\u0442\u044C\u044E"
      }),
      React.createElement(MenuItem, {
        labelElement: React.createElement(Icon, { icon: "add" }),
        intent: Intent.SUCCESS,
        onClick: this.cloneProductPrototype,
        text:
          "\u0421\u043E\u0437\u0434\u0430\u0442\u044C \u043F\u043E \u043E\u0431\u0440\u0430\u0437\u0446\u0443",
        title:
          "\u0421\u043E\u0437\u0434\u0430\u0442\u044C \u0441\u0442\u0430\u0442\u044C\u044E \u043F\u043E \u043E\u0431\u0440\u0430\u0437\u0446\u0443"
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", ActionController)],
    CloneButtons.prototype,
    "_actionController",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    CloneButtons.prototype,
    "_entityController",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", RelationController)],
    CloneButtons.prototype,
    "_relationController",
    void 0
  );
  return CloneButtons;
})(Component);
export { CloneButtons };
//# sourceMappingURL=CloneButtons.js.map
