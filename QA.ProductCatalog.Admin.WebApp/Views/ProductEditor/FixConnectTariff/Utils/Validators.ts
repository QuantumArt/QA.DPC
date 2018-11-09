import { setEquals } from "Utils/Array";
import {
  Product,
  ProductParameter,
  LinkParameter,
  DevicesForFixConnectAction,
  Region
} from "../TypeScriptSchema";

export const hasUniqueRegions = (product: Product) => () => {
  const productsWithSameRegions = product.MarketingProduct.Products.filter(
    otherProduct =>
      otherProduct !== product &&
      product.Regions.some(region => otherProduct.getBaseValue("Regions").includes(region))
  );

  if (productsWithSameRegions.length > 0) {
    const productIds = productsWithSameRegions.map(product => product._ServerId || 0);
    return `Продукт содержит регионы из продуктов: ${productIds.join(", ")}`;
  }
  return undefined;
};

export const isUniqueRegion = (product: Product) => (region: Region) => {
  const productsWithSameRegions = product.MarketingProduct.Products.filter(
    otherProduct =>
      otherProduct !== product && otherProduct.getBaseValue("Regions").includes(region)
  );

  if (productsWithSameRegions.length > 0) {
    const productIds = productsWithSameRegions.map(product => product._ServerId || 0);
    return `Регион содержится в продуктах: ${productIds.join(", ")}`;
  }
  return undefined;
};

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

type Parameter = ProductParameter | LinkParameter;

export const hasUniqueTariffDirection = (
  parameter: Parameter,
  allParameters: Parameter[]
) => () => {
  if (parameter._IsVirtual || !parameter.BaseParameter) {
    return undefined;
  }

  const paramsWithSameTariffDirection = allParameters.filter(
    otherParameter =>
      !otherParameter._IsVirtual &&
      otherParameter !== parameter &&
      otherParameter.BaseParameter === parameter.BaseParameter &&
      otherParameter.Zone === parameter.Zone &&
      otherParameter.Direction === parameter.Direction &&
      setEquals(parameter.BaseParameterModifiers, otherParameter.BaseParameterModifiers)
  );

  if (paramsWithSameTariffDirection.length > 0) {
    const titles = paramsWithSameTariffDirection.map(param => `[${param.Title}]`);
    return `Тарифное направление совпадает с ${titles.join(", ")}`;
  }

  return undefined;
};
