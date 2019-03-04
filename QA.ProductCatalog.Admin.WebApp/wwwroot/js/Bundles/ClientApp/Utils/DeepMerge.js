import { isPlainObject } from "./TypeChecks";
/**
 * Deep merge two objects with structural sharing of unchanged parts.
 */
export function deepMerge(left, right) {
  if (left === right) {
    return right;
  }
  if (isPlainObject(right)) {
    if (!isPlainObject(left)) {
      return right;
    }
    var result = {};
    for (var key in left) {
      result[key] = null;
    }
    for (var key in right) {
      result[key] = null;
    }
    for (var key in result) {
      result[key] = deepMerge(left[key], right[key]);
    }
    return result;
  }
  if (Array.isArray(right)) {
    if (!Array.isArray(left)) {
      return right;
    }
    var result = [];
    var length_1 = Math.max(left.length, right.length);
    for (var i = 0; i < length_1; i++) {
      result[i] = deepMerge(left[i], right[i]);
    }
    return result;
  }
  if (right != null || typeof left === "undefined") {
    return right;
  }
  return left;
}
//# sourceMappingURL=DeepMerge.js.map
