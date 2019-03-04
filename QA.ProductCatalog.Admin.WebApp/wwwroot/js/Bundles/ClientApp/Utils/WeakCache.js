import { computed } from "mobx";
var WeakCache = /** @class */ (function() {
  function WeakCache() {
    this._weakMap = new WeakMap();
  }
  WeakCache.prototype.get = function(key) {
    return this._weakMap.get(key);
  };
  WeakCache.prototype.set = function(key, value) {
    this._weakMap.set(key, value);
    return this;
  };
  WeakCache.prototype.getOrAdd = function(key, getValue) {
    var value = this._weakMap.get(key);
    if (!value) {
      value = getValue(key);
      this._weakMap.set(key, value);
    }
    return value;
  };
  return WeakCache;
})();
export { WeakCache };
var ComputedCache = /** @class */ (function() {
  function ComputedCache() {
    this._weakMap = new WeakMap();
  }
  ComputedCache.prototype.get = function(key) {
    var computedValue = this._weakMap.get(key);
    return computedValue && computedValue.get();
  };
  ComputedCache.prototype.getOrAdd = function(
    key,
    optionsOrGetValue,
    getValue
  ) {
    var computedValue = this._weakMap.get(key);
    if (!computedValue) {
      computedValue = getValue
        ? computed(getValue, optionsOrGetValue)
        : computed(optionsOrGetValue);
      this._weakMap.set(key, computedValue);
    }
    return computedValue.get();
  };
  return ComputedCache;
})();
export { ComputedCache };
//# sourceMappingURL=WeakCache.js.map
