import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityController } from "Services/EntityController";
import { Icon, Button, Intent } from "@blueprintjs/core";
import { action } from "mobx";
import "./ArticleEditor.scss";
/** Ссылка на редактирование статьи в модальном окне QP */
var EntityLink = /** @class */ (function(_super) {
  tslib_1.__extends(EntityLink, _super);
  function EntityLink() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.handleMouseDown = function(e) {
      if (e.nativeEvent.which === 2) {
        e.preventDefault();
        var _a = _this.props,
          model = _a.model,
          contentSchema = _a.contentSchema;
        _this._entityController.editEntity(model, contentSchema, false);
      }
    };
    _this.handleClick = function(e) {
      e.preventDefault();
      var _a = _this.props,
        model = _a.model,
        contentSchema = _a.contentSchema;
      _this._entityController.editEntity(model, contentSchema, true);
    };
    return _this;
  }
  EntityLink.prototype.render = function() {
    var model = this.props.model;
    return (
      model._ServerId > 0 &&
      React.createElement(
        Button,
        {
          minimal: true,
          small: true,
          className: "entity-link",
          rightIcon: React.createElement(Icon, {
            icon: "manually-entered-data",
            title: null
          }),
          intent: Intent.PRIMARY,
          title:
            "\u0420\u0435\u0434\u0430\u043A\u0442\u0438\u0440\u043E\u0432\u0430\u0442\u044C \u0441\u0442\u0430\u0442\u044C\u044E",
          onMouseDown: this.handleMouseDown,
          onClick: this.handleClick
        },
        model._ServerId
      )
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    EntityLink.prototype,
    "_entityController",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    EntityLink.prototype,
    "handleMouseDown",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    EntityLink.prototype,
    "handleClick",
    void 0
  );
  EntityLink = tslib_1.__decorate([observer], EntityLink);
  return EntityLink;
})(Component);
export { EntityLink };
//# sourceMappingURL=EntityLink.js.map
