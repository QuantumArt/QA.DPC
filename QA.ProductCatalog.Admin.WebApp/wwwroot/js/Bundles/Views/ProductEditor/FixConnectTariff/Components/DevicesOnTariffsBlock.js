import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { action } from "mobx";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleEditor, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  MultiRelationFieldTable,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { by, desc, asc } from "Utils/Array";
import {
  hasUniqueCities,
  isUniqueCity,
  isUniqueMarketingTariff,
  itemHasUniqueCities
} from "../Utils/DeviceOnTariffsValidators";
import { ParameterFields } from "./ParameterFields";
var DevicesOnTariffsBlock = /** @class */ (function(_super) {
  tslib_1.__extends(DevicesOnTariffsBlock, _super);
  function DevicesOnTariffsBlock() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.renderCities = function(props) {
      var device = props.model;
      var marketingDevice = _this.props.model;
      return React.createElement(
        MultiRelationFieldTags,
        tslib_1.__assign({}, props, {
          sortItemsBy: "Title",
          validate: hasUniqueCities(device, marketingDevice.DevicesOnTariffs),
          validateItems: isUniqueCity(device, marketingDevice.DevicesOnTariffs)
        })
      );
    };
    _this.renderParent = function(_a) {
      var model = _a.model,
        fieldSchema = _a.fieldSchema;
      var contentSchema = fieldSchema.RelatedContent;
      var productRelation = model.Parent;
      return (
        productRelation &&
        React.createElement(ArticleEditor, {
          model: productRelation,
          contentSchema: contentSchema,
          fieldOrders: ["Title", "Parameters"],
          fieldEditors: {
            Parameters: _this.renderParameters
          }
        })
      );
    };
    _this.renderMarketingTariffs = function(props) {
      var device = props.model;
      var marketingDevice = _this.props.model;
      var marketingTariff = _this.props.marketingTariff;
      return React.createElement(
        MultiRelationFieldTable,
        tslib_1.__assign({}, props, {
          highlightItems: function(deviceTariff) {
            return deviceTariff === marketingTariff
              ? 1 /* Highlight */
              : 0 /* None */;
          },
          sortItems: by(
            desc(function(deviceTariff) {
              return deviceTariff === marketingTariff;
            }),
            asc(function(deviceTariff) {
              return deviceTariff.Title;
            })
          ),
          validateItems: isUniqueMarketingTariff(
            device,
            marketingDevice.DevicesOnTariffs
          ),
          relationActions: function() {
            return React.createElement(
              React.Fragment,
              null,
              !device.MarketingTariffs.includes(marketingTariff) &&
                React.createElement(
                  Button,
                  {
                    minimal: true,
                    small: true,
                    rightIcon: "pin",
                    intent: Intent.PRIMARY,
                    onClick: function() {
                      return _this.pinDeviceToMarketingTariff(device);
                    },
                    title:
                      "\u041F\u0440\u0438\u0432\u044F\u0437\u0430\u0442\u044C \u043A \u0442\u0435\u043A\u0443\u0449\u0435\u043C\u0443 \u043C\u0430\u0440\u043A\u0435\u0442\u0438\u043D\u0433\u043E\u0432\u043E\u043C\u0443 \u0442\u0430\u0440\u0438\u0444\u0443 \u0444\u0438\u043A\u0441\u0438\u0440\u043E\u0432\u0430\u043D\u043D\u043E\u0439 \u0441\u0432\u044F\u0437\u0438"
                  },
                  "\u041F\u0440\u0438\u0432\u044F\u0437\u0430\u0442\u044C \u043A \u0442\u0435\u043A\u0443\u0449\u0435\u043C\u0443 \u0442\u0430\u0440\u0438\u0444\u0443"
                )
            );
          }
        })
      );
    };
    _this.renderParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
            { Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" },
            { Title: "Цена продажи", Unit: "rub", BaseParam: "SalePrice" }
          ]
        })
      );
    };
    return _this;
  }
  DevicesOnTariffsBlock.prototype.pinDeviceToMarketingTariff = function(
    deviceOnTariffs
  ) {
    var marketingTariff = this.props.marketingTariff;
    if (!deviceOnTariffs.MarketingTariffs.includes(marketingTariff)) {
      deviceOnTariffs.MarketingTariffs.push(marketingTariff);
      deviceOnTariffs.setTouched("MarketingTariffs");
    }
  };
  DevicesOnTariffsBlock.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema,
      filterModel = _a.filterModel;
    var marketingDevice = model;
    return React.createElement(RelationFieldAccordion, {
      model: model,
      fieldSchema: fieldSchema,
      canCloneEntity: true,
      canRemoveEntity: true,
      canClonePrototype: true,
      columnProportions: [3, 1, 1],
      displayFields: [
        regionsDisplayField,
        rentPriceDisplayField,
        salePriceDisplayField
      ],
      filterItems: filterModel.filterDevicesOnTariffs,
      highlightItems: filterModel.highlightDeviceOnTariffs,
      validateItems: itemHasUniqueCities(marketingDevice.DevicesOnTariffs),
      fieldOrders: ["Cities", "Parent", "MarketingTariffs"],
      fieldEditors: {
        MarketingDevice: IGNORE,
        Cities: this.renderCities,
        Parent: this.renderParent,
        MarketingTariffs: this.renderMarketingTariffs
      },
      onMountEntity: function(device) {
        return device.setTouched("Cities");
      }
    });
  };
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    DevicesOnTariffsBlock.prototype,
    "pinDeviceToMarketingTariff",
    null
  );
  return DevicesOnTariffsBlock;
})(Component);
export { DevicesOnTariffsBlock };
var regionsDisplayField = function(device) {
  return React.createElement(
    "div",
    { className: "products-accordion__regions" },
    device.Cities.map(function(region) {
      return region.Title;
    }).join(", ")
  );
};
var rentPriceDisplayField = function(device) {
  var parameter = device.Parent.Parameters.find(function(parameter) {
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
var salePriceDisplayField = function(device) {
  var parameter = device.Parent.Parameters.find(function(parameter) {
    var baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "SalePrice";
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
        "\u0426\u0435\u043D\u0430 \u043F\u0440\u043E\u0434\u0430\u0436\u0438:"
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
//# sourceMappingURL=DevicesOnTariffsBlock.js.map
