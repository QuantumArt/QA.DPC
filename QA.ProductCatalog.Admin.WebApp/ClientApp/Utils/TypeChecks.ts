import { isObservableArray } from "mobx";

export type Mutable<T> = { [k in keyof T]: Mutable<T[k]> };

export function isString(arg): arg is string {
  return typeof arg === "string";
}

export function isBoolean(arg): arg is boolean {
  return typeof arg === "boolean";
}

export function isNumber(arg): arg is number {
  return Number.isFinite ? Number.isFinite(arg) : typeof arg === "number" && !isNaN(arg);
}

export function isInteger(arg): arg is number {
  return Number.isInteger
    ? Number.isInteger(arg)
    : typeof arg === "number" && /^-?[0-9]+$/.test(String(arg));
}

export function isArray(arg): arg is any[] {
  return Array.isArray(arg) || isObservableArray(arg);
}

export function isObject(arg: any): arg is Object {
  return arg && typeof arg === "object" && !isArray(arg);
}

export function isFunction(arg): arg is Function {
  return typeof arg === "function";
}

export function isPlainObject(arg): arg is Object {
  if (isObject(arg)) {
    const prototype = Object.getPrototypeOf(arg);
    return prototype === Object.prototype || prototype === null;
  }
  return false;
}

export function isPromiseLike<T = any>(arg): arg is PromiseLike<T> {
  return isObject(arg) && isFunction(arg.then);
}

export function isSingleKeyObject(arg): arg is Object {
  if (!isObject(arg)) {
    return false;
  }
  let count = 0;
  for (const _key in arg) {
    if (++count === 2) {
      return false;
    }
  }
  return count === 1;
}

export function isNullOrWhiteSpace(arg): arg is null | string {
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
const dateRegex = /^\d{4}-\d{2}-\d{2}(T| )\d{2}:\d{2}:\d{2}(\.\d+)?(Z|\+\d{2}:\d{2})?/;

export function isIsoDateString(arg): arg is string {
  return isString(arg) && dateRegex.test(arg);
}
