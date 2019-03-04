import { isObservableArray } from "mobx";
export function isString(arg) {
  return typeof arg === "string";
}
export function isBoolean(arg) {
  return typeof arg === "boolean";
}
export function isNumber(arg) {
  return Number.isFinite
    ? Number.isFinite(arg)
    : typeof arg === "number" && !isNaN(arg);
}
export function isInteger(arg) {
  return Number.isInteger
    ? Number.isInteger(arg)
    : typeof arg === "number" && /^-?[0-9]+$/.test(String(arg));
}
export function isArray(arg) {
  return Array.isArray(arg) || isObservableArray(arg);
}
export function isObject(arg) {
  return arg && typeof arg === "object" && !isArray(arg);
}
export function isFunction(arg) {
  return typeof arg === "function";
}
export function isPlainObject(arg) {
  if (isObject(arg)) {
    var prototype = Object.getPrototypeOf(arg);
    return prototype === Object.prototype || prototype === null;
  }
  return false;
}
export function isPromiseLike(arg) {
  return isObject(arg) && isFunction(arg.then);
}
export function isSingleKeyObject(arg) {
  if (!isObject(arg)) {
    return false;
  }
  var count = 0;
  for (var _key in arg) {
    if (++count === 2) {
      return false;
    }
  }
  return count === 1;
}
export function isNullOrWhiteSpace(arg) {
  return arg == null || (isString(arg) && /^\s*$/.test(arg));
}
// Примеры для проверки
// 2018-03-12 10:46:32
// 2018-03-12T10:46:32
// 2018-03-12T10:46:32Z
// 2018-03-12T10:46:32+03:00
// 2018-03-12T10:46:32.123
// 2018-03-12T10:46:32.123Z
// 2018-03-12T10:46:32.123+03:00
// 2018-02-10T09:42:14.4575689
// 2018-02-10T09:42:14.4575689Z
// 2018-02-10T09:42:14.4575689+03:00
var dateRegex = /^\d{4}-\d{2}-\d{2}(T| )\d{2}:\d{2}:\d{2}(\.\d+)?(Z|\+\d{2}:\d{2})?/;
export function isIsoDateString(arg) {
  return isString(arg) && dateRegex.test(arg);
}
//# sourceMappingURL=TypeChecks.js.map
