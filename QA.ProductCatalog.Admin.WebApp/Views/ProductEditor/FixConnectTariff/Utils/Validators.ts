import { Product, DeviceOnTariffs } from "../TypeScriptSchema";

export function validateProduct(product: Product) {
  product.clearErrors();
  // @ts-ignore
  product.Type_Extension[product.Type].clearErrors();

  const productsWithSameRegions = product.MarketingProduct.Products.filter(
    otherProduct =>
      otherProduct !== product &&
      product.Regions.some(region => otherProduct.getBaseValue("Regions").includes(region))
  );

  if (productsWithSameRegions.length > 0) {
    const productIds = productsWithSameRegions.map(product => product._ServerId || 0).join(", ");
    const message = `Продукт содержит регионы из продуктов: ${productIds}`;
    product.setTouched("Regions", true);
    product.addErrors("Regions", message);
  }

  product.Parameters.forEach(param => {
    param.clearErrors();

    const paramsWithSameTariffDirection = product.Parameters.filter(
      otherParam =>
        otherParam !== param &&
        otherParam.BaseParameter === param.BaseParameter &&
        setEquals(param.BaseParameterModifiers, otherParam.BaseParameterModifiers)
    );

    if (paramsWithSameTariffDirection.length > 0) {
      const titles = paramsWithSameTariffDirection.map(param => `[${param.Title}]`).join(", ");
      const message = `Тарифное направление совпадает с ${titles}`;
      param.setTouched("BaseParameter", true);
      param.addErrors("BaseParameter", message);
    }
  });
}

export function validateDeviceOnTariffs(device: DeviceOnTariffs) {
  device.clearErrors();
  device.Parent.clearErrors();

  device.Parent.Parameters.forEach(param => {
    param.clearErrors();

    const paramsWithSameTariffDirection = device.Parent.Parameters.filter(
      otherParam =>
        otherParam !== param &&
        otherParam.BaseParameter === param.BaseParameter &&
        setEquals(param.BaseParameterModifiers, otherParam.BaseParameterModifiers)
    );

    if (paramsWithSameTariffDirection.length > 0) {
      const titles = paramsWithSameTariffDirection.map(param => `[${param.Title}]`).join(", ");
      const message = `Тарифное направление совпадает с ${titles}`;
      param.setTouched("BaseParameter", true);
      param.addErrors("BaseParameter", message);
    }
  });
}

function setEquals(first: any[], second: any[]) {
  return first.every(el => second.includes(el)) && second.every(el => first.includes(el));
}
