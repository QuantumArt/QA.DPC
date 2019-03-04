import { setEquals } from "Utils/Array";
export var hasUniqueTariffDirection = function(parameter, allParameters) {
  return function() {
    if (parameter._IsVirtual || !parameter.BaseParameter) {
      return undefined;
    }
    var paramsWithSameTariffDirection = allParameters.filter(function(
      otherParameter
    ) {
      return (
        !otherParameter._IsVirtual &&
        otherParameter !== parameter &&
        otherParameter.BaseParameter === parameter.BaseParameter &&
        otherParameter.Zone === parameter.Zone &&
        otherParameter.Direction === parameter.Direction &&
        setEquals(
          parameter.BaseParameterModifiers,
          otherParameter.BaseParameterModifiers
        )
      );
    });
    if (paramsWithSameTariffDirection.length > 0) {
      var titles = paramsWithSameTariffDirection.map(function(param) {
        return "[" + param.Title + "]";
      });
      return (
        "\u0422\u0430\u0440\u0438\u0444\u043D\u043E\u0435 \u043D\u0430\u043F\u0440\u0430\u0432\u043B\u0435\u043D\u0438\u0435 \u0441\u043E\u0432\u043F\u0430\u0434\u0430\u0435\u0442 \u0441 " +
        titles.join(", ")
      );
    }
    return undefined;
  };
};
//# sourceMappingURL=ParameterValidators.js.map
