import { Product } from "../TypeScriptSchema";

export function validateProduct(product: Product) {
  product.clearErrors();

  const productsWithSameRegions = product.MarketingProduct.Products.filter(
    otherProduct =>
      otherProduct !== product &&
      product.Regions.some(region => otherProduct.getBaseValue("Regions").includes(region))
  );

  if (productsWithSameRegions.length > 0) {
    const productIds = productsWithSameRegions.map(product => product._ServerId || 0);
    const message = `Продукт содержит регионы из продуктов: ${productIds.join(", ")}`;
    product.setTouched("Regions", true);
    product.addErrors("Regions", message);
  }
}
