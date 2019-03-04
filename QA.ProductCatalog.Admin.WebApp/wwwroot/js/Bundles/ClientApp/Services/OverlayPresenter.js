import * as tslib_1 from "tslib";
import React from "react";
import { observable, action } from "mobx";
import { Intent, Toaster, Position } from "@blueprintjs/core";
import { AlertWrapper } from "Components/Overlay/AlertWrapper";
var OverlayPresenter = /** @class */ (function() {
  function OverlayPresenter() {
    this.overlays = observable.array();
  }
  OverlayPresenter.prototype.alert = function(message, button) {
    var _this = this;
    var index = this.overlays.length;
    return new Promise(function(resolve) {
      _this.overlays.push(
        React.createElement(
          AlertWrapper,
          {
            key: index,
            canEscapeKeyCancel: true,
            canOutsideClickCancel: true,
            confirmButtonText: button,
            onClose: function() {
              resolve();
              setTimeout(
                action(function() {
                  return _this.overlays.splice(index, 1);
                }),
                300
              );
            }
          },
          message
        )
      );
    });
  };
  OverlayPresenter.prototype.confirm = function(
    message,
    confirmButton,
    cancelButton
  ) {
    var _this = this;
    var index = this.overlays.length;
    return new Promise(function(resolve) {
      _this.overlays.push(
        React.createElement(
          AlertWrapper,
          {
            key: index,
            confirmButtonText: confirmButton,
            cancelButtonText: cancelButton,
            intent: Intent.PRIMARY,
            onClose: function(confirmed) {
              resolve(confirmed);
              setTimeout(
                action(function() {
                  return _this.overlays.splice(index, 1);
                }),
                300
              );
            }
          },
          message
        )
      );
    });
  };
  OverlayPresenter.prototype.notify = function(toast) {
    NotificationPresenter.show(toast);
  };
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, String]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    OverlayPresenter.prototype,
    "alert",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, String, String]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    OverlayPresenter.prototype,
    "confirm",
    null
  );
  return OverlayPresenter;
})();
export { OverlayPresenter };
export var NotificationPresenter = Toaster.create({
  position: Position.BOTTOM_RIGHT
});
//# sourceMappingURL=OverlayPresenter.js.map
