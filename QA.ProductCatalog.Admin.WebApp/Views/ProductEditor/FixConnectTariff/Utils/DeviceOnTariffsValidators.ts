import { DeviceOnTariffs, Region } from "../TypeScriptSchema";

export const hasUniqueCities = (device: DeviceOnTariffs, otherDevices: DeviceOnTariffs[]) => () => {
  const devicesWithSameRegions = otherDevices.filter(
    otherDevice =>
      otherDevice !== device &&
      device.MarketingTariffs.some(marketingTariff =>
        otherDevice.getBaseValue("MarketingTariffs").includes(marketingTariff)
      ) &&
      device.Cities.some(region => otherDevice.getBaseValue("Cities").includes(region))
  );

  if (devicesWithSameRegions.length > 0) {
    const deviceIds = devicesWithSameRegions.map(device => device._ServerId || 0);
    return `Оборудование на тарифах содержит регионы из статей: ${deviceIds.join(", ")}`;
  }
  return undefined;
};

export const isUniqueCity = (device: DeviceOnTariffs, otherDevices: DeviceOnTariffs[]) => (
  region: Region
) => {
  const devicesWithSameRegions = otherDevices.filter(
    otherDevice =>
      otherDevice !== device &&
      device.MarketingTariffs.some(marketingTariff =>
        otherDevice.getBaseValue("MarketingTariffs").includes(marketingTariff)
      ) &&
      otherDevice.getBaseValue("Cities").includes(region)
  );

  if (devicesWithSameRegions.length > 0) {
    const deviceIds = devicesWithSameRegions.map(device => device._ServerId || 0);
    return `Регион содержится в статьях: ${deviceIds.join(", ")}`;
  }
  return undefined;
};
