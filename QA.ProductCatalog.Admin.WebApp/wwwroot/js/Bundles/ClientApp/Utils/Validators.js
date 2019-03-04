import { isArray, isString } from "Utils/TypeChecks";
export var required = function(value) {
  if (value == null || value === "" || (isArray(value) && value.length === 0)) {
    return "Поле обязательно для заполнения";
  }
  return undefined;
};
export var pattern = function(regExp) {
  if (isString(regExp)) {
    regExp = new RegExp(regExp);
  }
  return function(value) {
    if (value && !regExp.test(value)) {
      return "Поле не соответствует шаблону";
    }
    return undefined;
  };
};
export var maxCount = function(max) {
  return function(value) {
    if (value && value.length > max) {
      return (
        "\u0414\u043E\u043F\u0443\u0441\u0442\u0438\u043C\u043E \u043D\u0435 \u0431\u043E\u043B\u0435\u0435 " +
        max +
        " \u044D\u043B\u0435\u043C\u0435\u043D\u0442\u043E\u0432"
      );
    }
    return undefined;
  };
};
//# sourceMappingURL=Validators.js.map
