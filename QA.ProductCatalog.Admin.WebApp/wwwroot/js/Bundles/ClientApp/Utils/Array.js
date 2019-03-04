import * as tslib_1 from "tslib";
/**
 * Creates a comparsion function for Array `.sort()` that:
 * Compares values returned by selector function in ascending order.
 */
export var asc = function(selector) {
  if (typeof selector === "undefined") {
    return function(l, r) {
      return l < r ? -1 : l > r ? 1 : 0;
    };
  }
  return function(left, right) {
    var l = selector(left);
    var r = selector(right);
    return l < r ? -1 : l > r ? 1 : 0;
  };
};
/**
 * Creates a comparsion function for Array `.sort()` that:
 * Compares values returned by selector function in descending order.
 */
export var desc = function(selector) {
  if (typeof selector === "undefined") {
    return function(l, r) {
      return l < r ? 1 : l > r ? -1 : 0;
    };
  }
  return function(left, right) {
    var l = selector(left);
    var r = selector(right);
    return l < r ? 1 : l > r ? -1 : 0;
  };
};
/**
 * Creates a comparsion function for Array `.sort()` that:
 * Combines multiple other comparsion functions.
 */
export var by = function() {
  var comparers = [];
  for (var _i = 0; _i < arguments.length; _i++) {
    comparers[_i] = arguments[_i];
  }
  var _a = tslib_1.__read(comparers, 3),
    first = _a[0],
    second = _a[1],
    third = _a[2];
  switch (comparers.length) {
    case 1:
      return first;
    case 2:
      return function(left, right) {
        return first(left, right) || second(left, right);
      };
    case 3:
      return function(left, right) {
        return first(left, right) || second(left, right) || third(left, right);
      };
  }
  return function(left, right) {
    var length = comparers.length;
    for (var i = 0; i < length; i++) {
      var num = comparers[i](left, right);
      if (num) {
        return num;
      }
    }
    return 0;
  };
};
/**
 * Check if two arrays contains the same set of elements
 */
export function setEquals(first, second) {
  if (first === second) {
    return true;
  }
  var firstSet = first.length > 100 && new Set(first);
  var secondSet = second.length > 100 && new Set(second);
  return (
    first &&
    second &&
    first.every(
      secondSet
        ? function(el) {
            return secondSet.has(el);
          }
        : function(el) {
            return second.includes(el);
          }
    ) &&
    second.every(
      firstSet
        ? function(el) {
            return firstSet.has(el);
          }
        : function(el) {
            return first.includes(el);
          }
    )
  );
}
//# sourceMappingURL=Array.js.map
