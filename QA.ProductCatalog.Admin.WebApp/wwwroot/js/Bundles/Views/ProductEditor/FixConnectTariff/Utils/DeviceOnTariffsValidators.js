/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
export var hasUniqueCities = function(device, otherDevices) {
  return function() {
    var devicesWithSameRegions = otherDevices.filter(function(otherDevice) {
      return (
        otherDevice !== device &&
        device.MarketingTariffs.some(function(marketingTariff) {
          return otherDevice
            .getBaseValue("MarketingTariffs")
            .includes(marketingTariff);
        }) &&
        device.Cities.some(function(region) {
          return otherDevice.getBaseValue("Cities").includes(region);
        })
      );
    });
    if (devicesWithSameRegions.length > 0) {
      var deviceIds = devicesWithSameRegions.map(function(device) {
        return device._ServerId || 0;
      });
      return (
        "\u041E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435 \u043D\u0430 \u0442\u0430\u0440\u0438\u0444\u0430\u0445 \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442 \u0440\u0435\u0433\u0438\u043E\u043D\u044B \u0438\u0437 \u0441\u0442\u0430\u0442\u0435\u0439: " +
        deviceIds.join(", ")
      );
    }
    return undefined;
  };
};
/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
export var itemHasUniqueCities = function(otherDevices) {
  return function(device) {
    var devicesWithSameRegions = otherDevices.filter(function(otherDevice) {
      return (
        otherDevice !== device &&
        device.MarketingTariffs.some(function(marketingTariff) {
          return otherDevice
            .getBaseValue("MarketingTariffs")
            .includes(marketingTariff);
        }) &&
        device.Cities.some(function(region) {
          return otherDevice.getBaseValue("Cities").includes(region);
        })
      );
    });
    if (devicesWithSameRegions.length > 0) {
      var deviceIds = devicesWithSameRegions.map(function(device) {
        return device._ServerId || 0;
      });
      return (
        "\u041E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435 \u043D\u0430 \u0442\u0430\u0440\u0438\u0444\u0430\u0445 \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442 \u0440\u0435\u0433\u0438\u043E\u043D\u044B \u0438\u0437 \u0441\u0442\u0430\u0442\u0435\u0439: " +
        deviceIds.join(", ")
      );
    }
    return undefined;
  };
};
/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
export var isUniqueCity = function(device, otherDevices) {
  return function(region) {
    var devicesWithSameRegions = otherDevices.filter(function(otherDevice) {
      return (
        otherDevice !== device &&
        device.MarketingTariffs.some(function(marketingTariff) {
          return otherDevice
            .getBaseValue("MarketingTariffs")
            .includes(marketingTariff);
        }) &&
        otherDevice.getBaseValue("Cities").includes(region)
      );
    });
    if (devicesWithSameRegions.length > 0) {
      var deviceIds = devicesWithSameRegions.map(function(device) {
        return device._ServerId || 0;
      });
      return (
        "\u0420\u0435\u0433\u0438\u043E\u043D \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442\u0441\u044F \u0432 \u0441\u0442\u0430\u0442\u044C\u044F\u0445: " +
        deviceIds.join(", ")
      );
    }
    return undefined;
  };
};
/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
export var isUniqueMarketingTariff = function(device, otherDevices) {
  return function(marketingTariff) {
    var devicesWithSameTariffs = otherDevices.filter(function(otherDevice) {
      return (
        otherDevice !== device &&
        device.Cities.some(function(region) {
          return otherDevice.getBaseValue("Cities").includes(region);
        }) &&
        otherDevice.getBaseValue("MarketingTariffs").includes(marketingTariff)
      );
    });
    if (devicesWithSameTariffs.length > 0) {
      var deviceIds = devicesWithSameTariffs.map(function(device) {
        return device._ServerId || 0;
      });
      return (
        "\u041C\u0430\u0440\u043A\u0435\u0442\u0438\u043D\u0433\u043E\u0432\u044B\u0439 \u0442\u0430\u0440\u0438\u0444 \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442\u0441\u044F \u0432 \u0441\u0442\u0430\u0442\u044C\u044F\u0445: " +
        deviceIds.join(", ")
      );
    }
    return undefined;
  };
};
//# sourceMappingURL=DeviceOnTariffsValidators.js.map
