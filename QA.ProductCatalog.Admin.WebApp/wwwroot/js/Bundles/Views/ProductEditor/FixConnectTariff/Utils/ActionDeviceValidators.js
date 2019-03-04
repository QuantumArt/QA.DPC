export var hasUniqueMarketingDevice = function(device) {
  return function() {
    var devicesWithSameMarketing = device.FixConnectAction.ActionMarketingDevices.filter(
      function(otherDevice) {
        return (
          otherDevice !== device &&
          otherDevice.MarketingDevice === device.MarketingDevice
        );
      }
    );
    if (devicesWithSameMarketing.length > 0) {
      var titles = devicesWithSameMarketing.map(function(devices) {
        return "[" + devices.Parent.Title + "]";
      });
      return (
        "\u041C\u0430\u0440\u043A\u0435\u0442\u0438\u043D\u0433\u043E\u0432\u043E\u0435 \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435 \u0441\u043E\u0432\u043F\u0430\u0434\u0430\u0435\u0442 \u0441 " +
        titles.join(", ")
      );
    }
    return undefined;
  };
};
/**
 * Не может быть двух разных акций (с разными маркетинговыми акциями),
 * пересекаюхщихся по городам, и при этом имеющих непустые наборы акционного оборудования.
 */
export var onlyOnePerRegionHasDevices = function(actionParent, otherActions) {
  return function() {
    if (actionParent.ActionMarketingDevices.length === 0) {
      return undefined;
    }
    var otherActionsWithDevices = otherActions.filter(function(otherAction) {
      var otherParent = otherAction.Parent;
      return (
        otherParent !== actionParent &&
        otherParent.getBaseValue("ActionMarketingDevices").length > 0 &&
        otherParent.getBaseValue("MarketingProduct") !==
          actionParent.MarketingProduct &&
        actionParent.Regions.some(function(region) {
          return otherParent.getBaseValue("Regions").includes(region);
        })
      );
    });
    if (otherActionsWithDevices.length > 0) {
      var strings = otherActionsWithDevices.map(function(action) {
        return (
          "[" +
          (action._ServerId || 0) +
          "] " +
          action.Parent.MarketingProduct.Title
        );
      });
      return (
        "\u0410\u043A\u0446\u0438\u044F \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442 \u0430\u043A\u0446\u0438\u043E\u043D\u043D\u043E\u0435 \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435 \u0441\u043E\u0432\u043C\u0435\u0441\u0442\u043D\u043E \u0441 \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0430\u043C\u0438: " +
        strings.join(", ")
      );
    }
    return undefined;
  };
};
/**
 * Не может быть двух разных акций (с разными маркетинговыми акциями),
 * пересекаюхщихся по городам, и при этом имеющих непустые наборы акционного оборудования.
 */
export var onlyOneItemPerRegionHasDevices = function(otherActions) {
  return function(action) {
    var actionParent = action.Parent;
    if (actionParent.ActionMarketingDevices.length === 0) {
      return undefined;
    }
    var otherActionsWithDevices = otherActions.filter(function(otherAction) {
      var otherParent = otherAction.Parent;
      return (
        otherParent !== actionParent &&
        otherParent.getBaseValue("ActionMarketingDevices").length > 0 &&
        otherParent.getBaseValue("MarketingProduct") !==
          actionParent.MarketingProduct &&
        actionParent.Regions.some(function(region) {
          return otherParent.getBaseValue("Regions").includes(region);
        })
      );
    });
    if (otherActionsWithDevices.length > 0) {
      var strings = otherActionsWithDevices.map(function(action) {
        return (
          "[" +
          (action._ServerId || 0) +
          "] " +
          action.Parent.MarketingProduct.Title
        );
      });
      return (
        "\u0410\u043A\u0446\u0438\u044F \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442 \u0430\u043A\u0446\u0438\u043E\u043D\u043D\u043E\u0435 \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435 \u0441\u043E\u0432\u043C\u0435\u0441\u0442\u043D\u043E \u0441 \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0430\u043C\u0438: " +
        strings.join(", ")
      );
    }
    return undefined;
  };
};
//# sourceMappingURL=ActionDeviceValidators.js.map
