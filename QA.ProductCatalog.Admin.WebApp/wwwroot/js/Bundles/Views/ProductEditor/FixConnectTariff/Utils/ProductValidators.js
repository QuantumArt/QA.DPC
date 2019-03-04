export var hasUniqueRegions = function(product, otherProducts) {
  if (otherProducts === void 0) {
    otherProducts = product.MarketingProduct.Products;
  }
  return function() {
    var productsWithSameRegions = otherProducts.filter(function(otherProduct) {
      return (
        otherProduct !== product &&
        product.Regions.some(function(region) {
          return otherProduct.getBaseValue("Regions").includes(region);
        })
      );
    });
    if (productsWithSameRegions.length > 0) {
      var productIds = productsWithSameRegions.map(function(product) {
        return product._ServerId || 0;
      });
      return (
        "\u041F\u0440\u043E\u0434\u0443\u043A\u0442 \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442 \u0440\u0435\u0433\u0438\u043E\u043D\u044B \u0438\u0437 \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u043E\u0432: " +
        productIds.join(", ")
      );
    }
    return undefined;
  };
};
export var isUniqueRegion = function(product, otherProducts) {
  if (otherProducts === void 0) {
    otherProducts = product.MarketingProduct.Products;
  }
  return function(region) {
    var productsWithSameRegions = otherProducts.filter(function(otherProduct) {
      return (
        otherProduct !== product &&
        otherProduct.getBaseValue("Regions").includes(region)
      );
    });
    if (productsWithSameRegions.length > 0) {
      var productIds = productsWithSameRegions.map(function(product) {
        return product._ServerId || 0;
      });
      return (
        "\u0420\u0435\u0433\u0438\u043E\u043D \u0441\u043E\u0434\u0435\u0440\u0436\u0438\u0442\u0441\u044F \u0432 \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0430\u0445: " +
        productIds.join(", ")
      );
    }
    return undefined;
  };
};
//# sourceMappingURL=ProductValidators.js.map
