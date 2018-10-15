import { Product, ProductParameter, LinkParameter } from "../TypeScriptSchema";

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

type Parameter = ProductParameter | LinkParameter;

export const hasUniqueTariffDirection = (
  parameter: Parameter,
  allParameters: Parameter[]
) => () => {
  const paramsWithSameTariffDirection = allParameters.filter(
    otherParameter =>
      otherParameter !== parameter &&
      otherParameter.BaseParameter === parameter.BaseParameter &&
      setEquals(parameter.BaseParameterModifiers, otherParameter.BaseParameterModifiers)
  );

  if (paramsWithSameTariffDirection.length > 0) {
    const titles = paramsWithSameTariffDirection.map(param => `[${param.Title}]`);
    return `Тарифное направление совпадает с ${titles.join(", ")}`;
  }
  return undefined;
};

function setEquals(first: any[], second: any[]) {
  if (first === second) {
    return true;
  }
  return (
    first &&
    second &&
    first.every(el => second.includes(el)) &&
    second.every(el => first.includes(el))
  );
}
