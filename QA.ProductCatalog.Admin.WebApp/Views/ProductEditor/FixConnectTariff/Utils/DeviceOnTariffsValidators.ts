import { DeviceOnTariffs, Region, MarketingProduct } from "../TypeScriptSchema";

/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
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

/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
export const itemHasUniqueCities = (otherDevices: DeviceOnTariffs[]) => (
  device: DeviceOnTariffs
) => {
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

/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
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

/** Недопустимо совпадение в матрице МаркетТариф + Город + оборудование */
export const isUniqueMarketingTariff = (
  device: DeviceOnTariffs,
  otherDevices: DeviceOnTariffs[]
) => (marketingTariff: MarketingProduct) => {
  const devicesWithSameTariffs = otherDevices.filter(
    otherDevice =>
      otherDevice !== device &&
      device.Cities.some(region => otherDevice.getBaseValue("Cities").includes(region)) &&
      otherDevice.getBaseValue("MarketingTariffs").includes(marketingTariff)
  );

  if (devicesWithSameTariffs.length > 0) {
    const deviceIds = devicesWithSameTariffs.map(device => device._ServerId || 0);
    return `Маркетинговый тариф содержится в статьях: ${deviceIds.join(", ")}`;
  }
  return undefined;
};
