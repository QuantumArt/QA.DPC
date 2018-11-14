import { DevicesForFixConnectAction, FixConnectAction, Product } from "../TypeScriptSchema";

export const hasUniqueMarketingDevice = (device: DevicesForFixConnectAction) => () => {
  const devicesWithSameMarketing = device.FixConnectAction.ActionMarketingDevices.filter(
    otherDevice => otherDevice !== device && otherDevice.MarketingDevice === device.MarketingDevice
  );

  if (devicesWithSameMarketing.length > 0) {
    const titles = devicesWithSameMarketing.map(devices => `[${devices.Parent.Title}]`);
    return `Маркетинговое оборудование совпадает с ${titles.join(", ")}`;
  }
  return undefined;
};

/**
 * Не может быть двух разных акций (с разными маркетинговыми акциями),
 * пересекаюхщихся по городам, и при этом имеющих непустые наборы акционного оборудования.
 */
export const onlyOnePerRegionHasDevices = (
  actionParent: Product,
  otherActions: FixConnectAction[]
) => () => {
  if (actionParent.ActionMarketingDevices.length === 0) {
    return undefined;
  }

  const otherActionsWithDevices = otherActions.filter(otherAction => {
    const otherParent = otherAction.Parent;
    return (
      otherParent !== actionParent &&
      otherParent.getBaseValue("ActionMarketingDevices").length > 0 &&
      otherParent.getBaseValue("MarketingProduct") !== actionParent.MarketingProduct &&
      actionParent.Regions.some(region => otherParent.getBaseValue("Regions").includes(region))
    );
  });

  if (otherActionsWithDevices.length > 0) {
    const strings = otherActionsWithDevices.map(
      action => `[${action._ServerId || 0}] ${action.Parent.MarketingProduct.Title}`
    );
    return `Акция содержит акционное оборудование совместно с продуктами: ${strings.join(", ")}`;
  }
  return undefined;
};

/**
 * Не может быть двух разных акций (с разными маркетинговыми акциями),
 * пересекаюхщихся по городам, и при этом имеющих непустые наборы акционного оборудования.
 */
export const onlyOneItemPerRegionHasDevices = (otherActions: FixConnectAction[]) => (
  action: FixConnectAction
) => {
  const actionParent = action.Parent;
  if (actionParent.ActionMarketingDevices.length === 0) {
    return undefined;
  }
  const otherActionsWithDevices = otherActions.filter(otherAction => {
    const otherParent = otherAction.Parent;
    return (
      otherParent !== actionParent &&
      otherParent.getBaseValue("ActionMarketingDevices").length > 0 &&
      otherParent.getBaseValue("MarketingProduct") !== actionParent.MarketingProduct &&
      actionParent.Regions.some(region => otherParent.getBaseValue("Regions").includes(region))
    );
  });

  if (otherActionsWithDevices.length > 0) {
    const strings = otherActionsWithDevices.map(
      action => `[${action._ServerId || 0}] ${action.Parent.MarketingProduct.Title}`
    );
    return `Акция содержит акционное оборудование совместно с продуктами: ${strings.join(", ")}`;
  }
  return undefined;
};
