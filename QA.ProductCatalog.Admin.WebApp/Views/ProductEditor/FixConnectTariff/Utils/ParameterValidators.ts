import { setEquals } from "Utils/Array";
import { ProductParameter, LinkParameter } from "../TypeScriptSchema";

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
