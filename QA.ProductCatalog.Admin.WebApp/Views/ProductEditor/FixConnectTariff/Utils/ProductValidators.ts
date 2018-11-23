import { Product, Region } from "../TypeScriptSchema";

export const hasUniqueRegions = (
  product: Product,
  otherProducts: Product[] = product.MarketingProduct.Products
) => () => {
  const productsWithSameRegions = otherProducts.filter(
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

export const isUniqueRegion = (
  product: Product,
  otherProducts: Product[] = product.MarketingProduct.Products
) => (region: Region) => {
  const productsWithSameRegions = otherProducts.filter(
    otherProduct =>
      otherProduct !== product && otherProduct.getBaseValue("Regions").includes(region)
  );

  if (productsWithSameRegions.length > 0) {
    const productIds = productsWithSameRegions.map(product => product._ServerId || 0);
    return `Регион содержится в продуктах: ${productIds.join(", ")}`;
  }
  return undefined;
};
