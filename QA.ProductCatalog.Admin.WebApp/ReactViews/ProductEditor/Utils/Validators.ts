import { isArray, isString } from "ProductEditor/Utils/TypeChecks";

export const required = (value: any) => {
  if (value == null || value === "" || (isArray(value) && value.length === 0)) {
    return "Поле обязательно для заполнения";
  }
  return undefined;
};

export const pattern = (regExp: string | RegExp) => {
  if (isString(regExp)) {
    regExp = new RegExp(regExp);
  }
  return (value: string) => {
    if (value && !(regExp as RegExp).test(value)) {
      return "Поле не соответствует шаблону";
    }
    return undefined;
  };
};

export const maxCount = (max: number) => (value: any[]) => {
  if (value && value.length > max) {
    return `Допустимо не более ${max} элементов`;
  }
  return undefined;
};
