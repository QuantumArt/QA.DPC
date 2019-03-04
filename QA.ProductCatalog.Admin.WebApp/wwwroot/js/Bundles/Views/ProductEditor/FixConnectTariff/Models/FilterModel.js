import * as tslib_1 from "tslib";
import { observable, action, computed } from "mobx";
var FilterModel = /** @class */ (function() {
  function FilterModel(fixTariff) {
    var _this = this;
    this.fixTariff = fixTariff;
    this.filterByTariffRegions = true;
    this.filterByMarketingTariff = true;
    this.selectedRegionIds = [];
    this.toggleFilterByTariffRegions = function() {
      _this.filterByTariffRegions = !_this.filterByTariffRegions;
    };
    this.toggleFilterByMarketingTariff = function() {
      _this.filterByMarketingTariff = !_this.filterByMarketingTariff;
    };
    this.setSelectedRegionIds = function(values) {
      _this.selectedRegionIds = values;
    };
    this.filterProducts = function(product) {
      var _a = _this,
        filterByTariffRegions = _a.filterByTariffRegions,
        fixTariffHasRegionId = _a.fixTariffHasRegionId,
        selectedRegionIds = _a.selectedRegionIds,
        hasSelectedRegionId = _a.hasSelectedRegionId;
      if (
        filterByTariffRegions &&
        !product.getBaseValue("Regions").some(function(region) {
          return fixTariffHasRegionId[region._ClientId];
        })
      ) {
        return false;
      }
      if (
        selectedRegionIds.length > 0 &&
        !product.getBaseValue("Regions").some(function(region) {
          return hasSelectedRegionId[region._ClientId];
        })
      ) {
        return false;
      }
      return true;
    };
    this.filterActions = function(action) {
      var _a = _this,
        filterByTariffRegions = _a.filterByTariffRegions,
        fixTariffHasRegionId = _a.fixTariffHasRegionId,
        selectedRegionIds = _a.selectedRegionIds,
        hasSelectedRegionId = _a.hasSelectedRegionId;
      if (
        filterByTariffRegions &&
        !action.Parent.getBaseValue("Regions").some(function(region) {
          return fixTariffHasRegionId[region._ClientId];
        })
      ) {
        return false;
      }
      if (
        selectedRegionIds.length > 0 &&
        !action.Parent.getBaseValue("Regions").some(function(region) {
          return hasSelectedRegionId[region._ClientId];
        })
      ) {
        return false;
      }
      return true;
    };
    this.filterDeviceActions = function(markDevice) {
      return function(action) {
        if (
          !action.Parent.getBaseValue("ActionMarketingDevices").some(function(
            actionDevice
          ) {
            return actionDevice.getBaseValue("MarketingDevice") === markDevice;
          })
        ) {
          return false;
        }
        return _this.filterActions(action);
      };
    };
    this.filterDevicesOnTariffs = function(device) {
      var _a = _this,
        filterByMarketingTariff = _a.filterByMarketingTariff,
        filterByTariffRegions = _a.filterByTariffRegions,
        fixTariffHasRegionId = _a.fixTariffHasRegionId,
        selectedRegionIds = _a.selectedRegionIds,
        hasSelectedRegionId = _a.hasSelectedRegionId;
      if (
        filterByMarketingTariff &&
        !device
          .getBaseValue("MarketingTariffs")
          .includes(_this.fixTariff.MarketingProduct)
      ) {
        return false;
      }
      if (
        filterByTariffRegions &&
        !device.getBaseValue("Cities").some(function(region) {
          return fixTariffHasRegionId[region._ClientId];
        })
      ) {
        return false;
      }
      if (
        selectedRegionIds.length > 0 &&
        !device.getBaseValue("Cities").some(function(region) {
          return hasSelectedRegionId[region._ClientId];
        })
      ) {
        return false;
      }
      return true;
    };
    this.highlightProduct = function(product) {
      var _a = _this,
        filterByTariffRegions = _a.filterByTariffRegions,
        fixTariffHasRegionId = _a.fixTariffHasRegionId;
      if (filterByTariffRegions) {
        return 0 /* None */;
      }
      if (
        product.getBaseValue("Regions").some(function(region) {
          return fixTariffHasRegionId[region._ClientId];
        })
      ) {
        return 1 /* Highlight */;
      }
      return 2 /* Shade */;
    };
    this.highlightAction = function(action) {
      var _a = _this,
        filterByTariffRegions = _a.filterByTariffRegions,
        fixTariffHasRegionId = _a.fixTariffHasRegionId;
      if (
        !action
          .getBaseValue("MarketingOffers")
          .includes(_this.fixTariff.MarketingProduct)
      ) {
        return 2 /* Shade */;
      }
      if (filterByTariffRegions) {
        return 0 /* None */;
      }
      if (
        action.Parent.getBaseValue("Regions").some(function(region) {
          return fixTariffHasRegionId[region._ClientId];
        })
      ) {
        return 1 /* Highlight */;
      }
      return 2 /* Shade */;
    };
    this.highlightDeviceOnTariffs = function(device) {
      var _a = _this,
        filterByMarketingTariff = _a.filterByMarketingTariff,
        filterByTariffRegions = _a.filterByTariffRegions,
        fixTariffHasRegionId = _a.fixTariffHasRegionId;
      if (filterByMarketingTariff && filterByTariffRegions) {
        return 0 /* None */;
      }
      if (
        !filterByMarketingTariff &&
        !device
          .getBaseValue("MarketingTariffs")
          .includes(_this.fixTariff.MarketingProduct)
      ) {
        return 2 /* Shade */;
      }
      if (
        !filterByTariffRegions &&
        !device.getBaseValue("Cities").some(function(region) {
          return fixTariffHasRegionId[region._ClientId];
        })
      ) {
        return 2 /* Shade */;
      }
      return 1 /* Highlight */;
    };
  }
  Object.defineProperty(FilterModel.prototype, "fixTariffHasRegionId", {
    get: function() {
      return this.fixTariff.Regions.reduce(function(obj, region) {
        obj[region._ClientId] = true;
        return obj;
      }, {});
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(FilterModel.prototype, "hasSelectedRegionId", {
    get: function() {
      return this.selectedRegionIds.reduce(function(obj, clientId) {
        obj[clientId] = true;
        return obj;
      }, {});
    },
    enumerable: true,
    configurable: true
  });
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Object)],
    FilterModel.prototype,
    "filterByTariffRegions",
    void 0
  );
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Object)],
    FilterModel.prototype,
    "filterByMarketingTariff",
    void 0
  );
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Array)],
    FilterModel.prototype,
    "selectedRegionIds",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    FilterModel.prototype,
    "toggleFilterByTariffRegions",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    FilterModel.prototype,
    "toggleFilterByMarketingTariff",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    FilterModel.prototype,
    "setSelectedRegionIds",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    FilterModel.prototype,
    "fixTariffHasRegionId",
    null
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    FilterModel.prototype,
    "hasSelectedRegionId",
    null
  );
  return FilterModel;
})();
export { FilterModel };
//# sourceMappingURL=FilterModel.js.map
