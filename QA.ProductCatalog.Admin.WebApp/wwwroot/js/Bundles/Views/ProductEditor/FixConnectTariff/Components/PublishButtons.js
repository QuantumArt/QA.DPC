import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { MenuItem, Icon, Intent } from "@blueprintjs/core";
import { ValidationSummay } from "Components/ValidationSummary/ValidationSummary";
import { DataValidator } from "Services/DataValidator";
import { OverlayPresenter } from "Services/OverlayPresenter";
import { ActionController } from "Services/ActionController";
var PublishButtons = /** @class */ (function(_super) {
  tslib_1.__extends(PublishButtons, _super);
  function PublishButtons() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.publishProduct = function(e) {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var _a, model, contentSchema, errors;
        return tslib_1.__generator(this, function(_b) {
          switch (_b.label) {
            case 0:
              e.stopPropagation();
              (_a = this.props),
                (model = _a.model),
                (contentSchema = _a.contentSchema);
              errors = this._dataValidator.collectErrors(
                model,
                contentSchema,
                false
              );
              if (!(errors.length > 0)) return [3 /*break*/, 2];
              return [
                4 /*yield*/,
                this._overlayPresenter.alert(
                  React.createElement(ValidationSummay, { errors: errors }),
                  "OK"
                )
              ];
            case 1:
              _b.sent();
              return [2 /*return*/];
            case 2:
              return [
                4 /*yield*/,
                this._actionController.executeCustomAction(
                  "publish_product",
                  model,
                  contentSchema
                )
              ];
            case 3:
              _b.sent();
              return [2 /*return*/];
          }
        });
      });
    };
    _this.stageProduct = function(e) {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var _a, model, contentSchema, errors;
        return tslib_1.__generator(this, function(_b) {
          switch (_b.label) {
            case 0:
              e.stopPropagation();
              (_a = this.props),
                (model = _a.model),
                (contentSchema = _a.contentSchema);
              errors = this._dataValidator.collectErrors(
                model,
                contentSchema,
                false
              );
              if (!(errors.length > 0)) return [3 /*break*/, 2];
              return [
                4 /*yield*/,
                this._overlayPresenter.alert(
                  React.createElement(ValidationSummay, { errors: errors }),
                  "OK"
                )
              ];
            case 1:
              _b.sent();
              return [2 /*return*/];
            case 2:
              return [
                4 /*yield*/,
                this._actionController.executeCustomAction(
                  "publish_to_stage",
                  model,
                  contentSchema
                )
              ];
            case 3:
              _b.sent();
              return [2 /*return*/];
          }
        });
      });
    };
    return _this;
  }
  PublishButtons.prototype.render = function() {
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(MenuItem, {
        labelElement: React.createElement(Icon, {
          icon: "tick-circle",
          intent: Intent.SUCCESS
        }),
        onClick: this.publishProduct,
        text:
          "\u041F\u0443\u0431\u043B\u0438\u043A\u043E\u0432\u0430\u0442\u044C",
        title:
          "\u041F\u0443\u0431\u043B\u0438\u043A\u043E\u0432\u0430\u0442\u044C"
      }),
      React.createElement(MenuItem, {
        labelElement: React.createElement(Icon, {
          icon: "send-to",
          intent: Intent.SUCCESS
        }),
        onClick: this.stageProduct,
        text:
          "\u041E\u0442\u043F\u0440\u0430\u0432\u0438\u0442\u044C \u043D\u0430 stage",
        title:
          "\u041E\u0442\u043F\u0440\u0430\u0432\u0438\u0442\u044C \u043D\u0430 stage"
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataValidator)],
    PublishButtons.prototype,
    "_dataValidator",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", OverlayPresenter)],
    PublishButtons.prototype,
    "_overlayPresenter",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", ActionController)],
    PublishButtons.prototype,
    "_actionController",
    void 0
  );
  return PublishButtons;
})(Component);
export { PublishButtons };
//# sourceMappingURL=PublishButtons.js.map
