import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { Checkbox, Button, Intent, Icon } from "@blueprintjs/core";
import { MultiRelationFieldTable } from "Components/FieldEditors/FieldEditors";
import { inject } from "react-ioc";
import { FileController } from "Services/FileController";
var AdvantagesTable = /** @class */ (function(_super) {
  tslib_1.__extends(AdvantagesTable, _super);
  function AdvantagesTable() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.renderTitle = function(advantage) {
      return advantage.Title;
    };
    _this.renderIsGift = function(advantage) {
      return React.createElement(
        "div",
        { className: "advantages-table__is-gift" },
        "IsGift: ",
        React.createElement(Checkbox, {
          checked: advantage.IsGift,
          disabled: true
        })
      );
    };
    _this.renderImageSvg = function(advantage) {
      return advantage.ImageSvg;
    };
    _this.renderPreviewButton = function(advantage) {
      return (
        advantage.ImageSvg &&
        React.createElement(Button, {
          small: true,
          minimal: true,
          icon: React.createElement(Icon, {
            icon: "media",
            title: "\u041F\u0440\u043E\u0441\u043C\u043E\u0442\u0440"
          }),
          intent: Intent.PRIMARY,
          onClick: function() {
            return _this.previewImage(advantage);
          }
        })
      );
    };
    return _this;
  }
  AdvantagesTable.prototype.previewImage = function(advantage) {
    var relationFieldSchema = this.props.fieldSchema;
    var fileFieldSchema = relationFieldSchema.RelatedContent.Fields.ImageSvg;
    this._fileController.previewImage(advantage, fileFieldSchema);
  };
  AdvantagesTable.prototype.render = function() {
    return React.createElement(
      MultiRelationFieldTable,
      tslib_1.__assign({}, this.props, {
        displayFields: [
          this.renderTitle,
          this.renderIsGift,
          this.renderImageSvg,
          this.renderPreviewButton
        ]
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", FileController)],
    AdvantagesTable.prototype,
    "_fileController",
    void 0
  );
  return AdvantagesTable;
})(Component);
export { AdvantagesTable };
//# sourceMappingURL=AdvantagesTable.js.map
