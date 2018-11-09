import { DevicesForFixConnectAction } from "../TypeScriptSchema";

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
