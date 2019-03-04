import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { computed } from "mobx";
import { observer } from "mobx-react";
import { Col } from "react-flexbox-grid";
import { Callout, Intent } from "@blueprintjs/core";
var DevicesWarningBlock = /** @class */ (function(_super) {
  tslib_1.__extends(DevicesWarningBlock, _super);
  function DevicesWarningBlock() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  Object.defineProperty(DevicesWarningBlock.prototype, "saleUnavailable", {
    get: function() {
      return this.props.marketingDevice.Modifiers.some(function(modifier) {
        return modifier.Alias === "SaleUnavailable";
      });
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(DevicesWarningBlock.prototype, "rentUnavailable", {
    get: function() {
      return this.props.marketingDevice.Modifiers.some(function(modifier) {
        return modifier.Alias === "RentUnavailable";
      });
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(DevicesWarningBlock.prototype, "notFoundRegions", {
    get: function() {
      var _this = this;
      var _a = this.props,
        fixConnectTariff = _a.fixConnectTariff,
        marketingDevice = _a.marketingDevice;
      var devices = marketingDevice.Products;
      var notFoundRegions = [];
      fixConnectTariff.Regions.forEach(function(region) {
        if (
          !notFoundRegions.includes(region) &&
          !devices.some(function(device) {
            return _this.deviceHasRegionAndParams(device, region);
          })
        ) {
          notFoundRegions.push(region);
        }
      });
      return notFoundRegions;
    },
    enumerable: true,
    configurable: true
  });
  /**
   * a.	Проверить на наличие Продуктов (тип “Оборудование”) в каждом городе текущего тарифа.
   * b.	Если у Маркетингового Продукта нет модификатора Продукта “Продажа недоступна”,
   *    проверить на наличие у Продуктов параметра с БП “Цена продажи”.
   * c.	Если у Маркетингового Продукта нет модификатора Продукта “Аренда недоступна”,
   *    проверить на наличие у Продуктов параметра с БП “Цена аренды”.
   */
  DevicesWarningBlock.prototype.deviceHasRegionAndParams = function(
    device,
    region
  ) {
    return (
      device.getBaseValue("Regions").includes(region) &&
      (this.saleUnavailable || this.deviceHasParameter(device, "SalePrice")) &&
      (this.rentUnavailable || this.deviceHasParameter(device, "RentPrice"))
    );
  };
  DevicesWarningBlock.prototype.deviceHasParameter = function(device, alias) {
    return device.getBaseValue("Parameters").some(function(parameter) {
      var baseParameter = parameter.BaseParameter;
      return (
        baseParameter &&
        baseParameter.Alias === alias &&
        parameter.getBaseValue("NumValue") !== null
      );
    });
  };
  DevicesWarningBlock.prototype.render = function() {
    var regionTitles = this.notFoundRegions.map(function(region) {
      return region.Title;
    });
    return regionTitles.length > 0
      ? React.createElement(
          Col,
          { md: 12 },
          React.createElement(
            Callout,
            { intent: Intent.DANGER },
            "\u0412\u043D\u0438\u043C\u0430\u043D\u0438\u0435! \u0414\u043B\u044F \u0433\u043E\u0440\u043E\u0434\u043E\u0432 ",
            regionTitles.join(", "),
            " \u043F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u043E\u0441\u0442\u044C \u0437\u0430\u043F\u043E\u043B\u043D\u0435\u043D\u0438\u044F \u043F\u0430\u0440\u0430\u043C\u0435\u0442\u0440\u043E\u0432: ",
            !this.saleUnavailable && "Цена продажи",
            !this.saleUnavailable && !this.rentUnavailable && ", ",
            !this.rentUnavailable && "Цена аренды",
            "!"
          )
        )
      : null;
  };
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    DevicesWarningBlock.prototype,
    "saleUnavailable",
    null
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    DevicesWarningBlock.prototype,
    "rentUnavailable",
    null
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    DevicesWarningBlock.prototype,
    "notFoundRegions",
    null
  );
  DevicesWarningBlock = tslib_1.__decorate([observer], DevicesWarningBlock);
  return DevicesWarningBlock;
})(Component);
export { DevicesWarningBlock };
//# sourceMappingURL=DevicesWarningBlock.js.map
