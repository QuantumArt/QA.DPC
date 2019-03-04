import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { Alert, Intent, Icon } from "@blueprintjs/core";
import "./AlertWrapper.scss";
var AlertWrapper = /** @class */ (function(_super) {
  tslib_1.__extends(AlertWrapper, _super);
  function AlertWrapper() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.state = { isOpen: true };
    _this.handleClose = function(confirmed) {
      _this.setState({ isOpen: false });
      _this.props.onClose(confirmed);
    };
    return _this;
  }
  AlertWrapper.prototype.render = function() {
    var isOpen = this.state.isOpen;
    return React.createElement(
      Alert,
      tslib_1.__assign({}, this.props, {
        isOpen: isOpen,
        onClose: this.handleClose,
        icon: React.createElement(Icon, {
          icon: "warning-sign",
          iconSize: Icon.SIZE_LARGE,
          intent: Intent.WARNING
        })
      })
    );
  };
  return AlertWrapper;
})(Component);
export { AlertWrapper };
//# sourceMappingURL=AlertWrapper.js.map
