import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { action } from "mobx";
import { Intent, Icon, MenuItem } from "@blueprintjs/core";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { PublicationStatusIcons } from "Components/PublicationStatusIcons/PublicationStatusIcons";
import {
  RelationFieldAccordion,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { PublicationContext } from "Services/PublicationContext";
import { EntityController } from "Services/EntityController";
import { by, asc } from "Utils/Array";
import { onlyOneItemPerRegionHasDevices } from "../Utils/ActionDeviceValidators";
import { ParameterFields } from "./ParameterFields";
var ActionsByDeviceBlock = /** @class */ (function(_super) {
  tslib_1.__extends(ActionsByDeviceBlock, _super);
  function ActionsByDeviceBlock() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.renderActionParent = function(_a) {
      var model = _a.model,
        fieldSchema = _a.fieldSchema;
      var contentSchema = fieldSchema.RelatedContent;
      var product = model.Parent;
      return (
        product &&
        React.createElement(ArticleEditor, {
          model: product,
          contentSchema: contentSchema,
          skipOtherFields: true,
          fieldOrders: ["Regions", "ActionMarketingDevices"],
          fieldEditors: {
            Regions: _this.renderActionRegions,
            ActionMarketingDevices: _this.renderActionDevice
          }
        })
      );
    };
    _this.renderActionRegions = function(props) {
      return React.createElement(
        MultiRelationFieldTags,
        tslib_1.__assign({}, props, { readonly: true, sortItemsBy: "Title" })
      );
    };
    _this.renderActionDevice = function(props) {
      var marketingDevice = _this.props.marketingDevice;
      var actionParent = props.model;
      var actionDevice = actionParent.ActionMarketingDevices.find(function(
        device
      ) {
        return device.MarketingDevice === marketingDevice;
      });
      var productRelationContentSchema =
        props.fieldSchema.RelatedContent.Fields.Parent.RelatedContent;
      return (
        actionDevice &&
        actionDevice.Parent &&
        React.createElement(ArticleEditor, {
          model: actionDevice.Parent,
          contentSchema: productRelationContentSchema,
          fieldOrders: ["Title", "Parameters"],
          fieldEditors: {
            Parameters: _this.renderActionDeviceParameters
          }
        })
      );
    };
    _this.renderActionDeviceParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
            { Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" }
          ]
        })
      );
    };
    return _this;
  }
  ActionsByDeviceBlock.prototype.saveActionDevice = function(e, action) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var _a,
        marketingDevice,
        actionsFieldSchema,
        actionDevice,
        productRelation,
        productRelationContentSchema;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            e.stopPropagation();
            (_a = this.props),
              (marketingDevice = _a.marketingDevice),
              (actionsFieldSchema = _a.actionsFieldSchema);
            actionDevice = action.Parent.ActionMarketingDevices.find(function(
              device
            ) {
              return device.MarketingDevice === marketingDevice;
            });
            productRelation = actionDevice && actionDevice.Parent;
            productRelationContentSchema =
              actionsFieldSchema.RelatedContent.Fields.Parent.RelatedContent
                .Fields.ActionMarketingDevices.RelatedContent.Fields.Parent
                .RelatedContent;
            if (!productRelation) return [3 /*break*/, 2];
            return [
              4 /*yield*/,
              this.entityController.saveEntity(
                productRelation,
                productRelationContentSchema
              )
            ];
          case 1:
            _b.sent();
            _b.label = 2;
          case 2:
            return [2 /*return*/];
        }
      });
    });
  };
  ActionsByDeviceBlock.prototype.render = function() {
    var _this = this;
    var _a = this.props,
      marketingDevice = _a.marketingDevice,
      marketingTariff = _a.marketingTariff,
      actionsFieldSchema = _a.actionsFieldSchema,
      filterModel = _a.filterModel;
    return React.createElement(RelationFieldAccordion, {
      model: marketingTariff,
      fieldSchema: actionsFieldSchema,
      filterItems: filterModel.filterDeviceActions(marketingDevice),
      highlightItems: filterModel.highlightAction,
      sortItems: by(
        asc(function(action) {
          return (
            action.Parent.MarketingProduct &&
            action.Parent.MarketingProduct._ServerId
          );
        }),
        asc(function(action) {
          return action.Parent._ServerId;
        })
      ),
      columnProportions: [4, 8, 3, 1],
      displayFields: [
        titleDisplayField,
        regionsDisplayField,
        rentPriceDisplayField(marketingDevice),
        function(action) {
          return React.createElement(PublicationStatusIcons, {
            model: action,
            product: action.Parent,
            contentSchema: actionsFieldSchema.RelatedContent,
            publicationContext: _this.publicationContext
          });
        }
      ],
      skipOtherFields: true,
      fieldEditors: {
        Parent: this.renderActionParent
      },
      validateItems: onlyOneItemPerRegionHasDevices(
        marketingTariff.FixConnectActions
      ),
      canSaveEntity: false,
      entityActions: function(action) {
        return React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "floppy-disk" }),
          intent: Intent.PRIMARY,
          onClick: function(e) {
            return _this.saveActionDevice(e, action);
          },
          text: "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C",
          title:
            "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C \u0430\u043A\u0446\u0438\u043E\u043D\u043D\u043E\u0435 \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435"
        });
      }
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationContext)],
    ActionsByDeviceBlock.prototype,
    "publicationContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    ActionsByDeviceBlock.prototype,
    "entityController",
    void 0
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    ActionsByDeviceBlock.prototype,
    "saveActionDevice",
    null
  );
  return ActionsByDeviceBlock;
})(Component);
export { ActionsByDeviceBlock };
var titleDisplayField = function(action) {
  return (
    action.Parent &&
    action.Parent.MarketingProduct &&
    action.Parent.MarketingProduct.Title
  );
};
var regionsDisplayField = function(action) {
  return (
    action.Parent &&
    React.createElement(
      "div",
      {
        className:
          "products-accordion__regions products-accordion__regions--action"
      },
      action.Parent.Regions.map(function(region) {
        return region.Title;
      }).join(", ")
    )
  );
};
var rentPriceDisplayField = function(marketingDevice) {
  return function(action) {
    var device =
      action.Parent &&
      action.Parent.ActionMarketingDevices.find(function(device) {
        return device.MarketingDevice === marketingDevice;
      });
    var parameter =
      device &&
      device.Parent.Parameters.find(function(parameter) {
        var baseParameter = parameter.BaseParameter;
        return baseParameter && baseParameter.Alias === "RentPrice";
      });
    return (
      parameter &&
      parameter.NumValue !== null &&
      React.createElement(
        React.Fragment,
        null,
        React.createElement(
          "div",
          null,
          "\u0426\u0435\u043D\u0430 \u0430\u0440\u0435\u043D\u0434\u044B:"
        ),
        React.createElement(
          "div",
          null,
          parameter.NumValue,
          " ",
          parameter.Unit && parameter.Unit.Title
        )
      )
    );
  };
};
//# sourceMappingURL=ActionsByDeviceBlock.js.map
