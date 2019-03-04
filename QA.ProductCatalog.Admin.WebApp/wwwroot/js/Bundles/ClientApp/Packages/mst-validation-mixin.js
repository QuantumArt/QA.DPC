import * as tslib_1 from "tslib";
import React, { Component } from "react";
import {
  observable,
  computed,
  action,
  transaction,
  intercept,
  observe,
  comparer,
  extendObservable,
  isObservableArray,
  isObservableMap
} from "mobx";
import {
  getSnapshot,
  isStateTreeNode,
  getChildType,
  resolveIdentifier,
  isReferenceType,
  clone
} from "mobx-state-tree";
import { isArray } from "Utils/TypeChecks";
var SHALLOW = { deep: false };
var FieldState = /** @class */ (function() {
  function FieldState(_model, _name) {
    this._model = _model;
    this._name = _name;
    this.isTouched = false;
    this.isChanged = false;
    this.hasFocus = false;
    this.baseValue = undefined;
    this.validators = observable.array(null, SHALLOW);
    this.errors = observable.array(null, SHALLOW);
  }
  Object.defineProperty(FieldState.prototype, "allErrors", {
    get: function() {
      var fieldErrors = [];
      var value = this._model[this._name];
      this.validators.forEach(function(validator) {
        var error = validator(value);
        if (error && !fieldErrors.includes(error)) {
          fieldErrors.push(error);
        }
      });
      fieldErrors.push.apply(fieldErrors, tslib_1.__spread(this.errors));
      return fieldErrors;
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(FieldState.prototype, "isEdited", {
    get: function() {
      return this.isTouched && this.isChanged;
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(FieldState.prototype, "hasErrors", {
    get: function() {
      return this.allErrors.length > 0;
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(FieldState.prototype, "hasVisibleErrors", {
    get: function() {
      return this.isTouched && !this.hasFocus && this.hasErrors;
    },
    enumerable: true,
    configurable: true
  });
  Object.defineProperty(FieldState.prototype, "visibleErrors", {
    get: function() {
      return this.hasVisibleErrors ? this.allErrors : null;
    },
    enumerable: true,
    configurable: true
  });
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Object)],
    FieldState.prototype,
    "isTouched",
    void 0
  );
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Object)],
    FieldState.prototype,
    "isChanged",
    void 0
  );
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Object)],
    FieldState.prototype,
    "hasFocus",
    void 0
  );
  tslib_1.__decorate(
    [observable.ref, tslib_1.__metadata("design:type", Object)],
    FieldState.prototype,
    "baseValue",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    FieldState.prototype,
    "allErrors",
    null
  );
  return FieldState;
})();
/**
 * Mixin для MobX State Tree, реализующий интерфейс @see ValidatableObject
 * @example
 * types.model("Product", {
 *   Title: types.string,
 *   Order: types.number,
 * }).extend(validationMixin);
 */
export var validationMixin = function(self) {
  var fields = observable.map(null, SHALLOW);
  var getOrAddFieldState = function(name) {
    var fieldState = fields.get(name);
    if (fieldState) {
      return fieldState;
    }
    fieldState = new FieldState(self, name);
    fields.set(name, fieldState);
    return fieldState;
  };
  var handleFieldChange = function(name, oldValue, isCollectionChange) {
    if (isCollectionChange === void 0) {
      isCollectionChange =
        isObservableArray(oldValue) || isObservableMap(oldValue);
    }
    var fieldState = getOrAddFieldState(name);
    if (!fieldState.isChanged) {
      fieldState.isChanged = true;
      if (isCollectionChange) {
        if (isStateTreeNode(oldValue)) {
          // @ts-ignore
          var elementType_1 = getChildType(oldValue);
          if (isReferenceType(elementType_1)) {
            if (isObservableArray(oldValue)) {
              fieldState.baseValue = getSnapshot(oldValue).map(function(id) {
                return resolveIdentifier(elementType_1, self, id);
              });
            } else if (isObservableMap(oldValue)) {
              var mapSnapshot_1 = getSnapshot(oldValue);
              fieldState.baseValue = new Map(
                Object.keys(mapSnapshot_1).map(function(key) {
                  return [
                    key,
                    resolveIdentifier(elementType_1, self, mapSnapshot_1[key])
                  ];
                })
              );
            }
          } else {
            // this cloned node is detached and can be used only for restore base value
            fieldState.baseValue = clone(oldValue);
          }
        } else if (isObservableArray(oldValue)) {
          fieldState.baseValue = observable.array(oldValue.peek());
        } else if (isObservableMap(oldValue)) {
          fieldState.baseValue = observable.map(oldValue.entries());
        }
      } else {
        fieldState.baseValue = oldValue;
      }
    }
    if (fieldState.errors.length > 0) {
      fieldState.errors.clear();
    }
  };
  var fieldInterceptors = Object.create(null);
  var addFieldInterceptor = function(name, value) {
    if (isObservableArray(value)) {
      fieldInterceptors[name] = intercept(value, function(change) {
        if (change.type === "splice") {
          if (change.removedCount > 0 || change.added.length > 0) {
            handleFieldChange(name, value, true);
          }
        } else if (change.type === "update") {
          if (
            !sutructuralEquals(value[change.index], change.newValue.storedValue)
          ) {
            handleFieldChange(name, value, true);
          }
        }
        return change;
      });
    } else if (isObservableMap(value)) {
      fieldInterceptors[name] = intercept(value, function(change) {
        if (change.type === "add") {
          handleFieldChange(name, value, true);
        } else if (change.type === "update") {
          if (
            !sutructuralEquals(
              value.get(change.name),
              change.newValue.storedValue
            )
          ) {
            handleFieldChange(name, value, true);
          }
        } else if (change.type === "delete") {
          if (value.has(change.name)) {
            handleFieldChange(name, value, true);
          }
        }
        return change;
      });
    }
  };
  var removeFieldInterceptor = function(name) {
    var fieldInterceptor = fieldInterceptors[name];
    if (fieldInterceptor) {
      fieldInterceptor();
      delete fieldInterceptors[name];
    }
  };
  intercept(self, function(change) {
    var oldValue = self[change.name];
    if (
      change.type === "remove" ||
      !sutructuralEquals(oldValue, change.newValue)
    ) {
      handleFieldChange(change.name, oldValue);
    }
    return change;
  });
  // otherwise, model can not be created from snapshot
  Promise.resolve().then(function() {
    Object.keys(self).forEach(function(name) {
      addFieldInterceptor(name, self[name]);
    });
  });
  observe(self, function(change) {
    removeFieldInterceptor(change.name);
    if (change.type !== "remove") {
      addFieldInterceptor(change.name, self[change.name]);
    }
  });
  return {
    actions: {
      addErrors: function(name) {
        var errors = [];
        for (var _i = 1; _i < arguments.length; _i++) {
          errors[_i - 1] = arguments[_i];
        }
        var fieldErrors = getOrAddFieldState(name).errors;
        errors.forEach(function(error) {
          if (!fieldErrors.includes(error)) {
            fieldErrors.push(error);
          }
        });
        return self;
      },
      clearErrors: function(name) {
        if (name) {
          var fieldState = fields.get(name);
          if (fieldState && fieldState.errors.length > 0) {
            fieldState.errors.clear();
          }
        } else {
          fields.forEach(function(fieldState) {
            if (fieldState.errors.length > 0) {
              fieldState.errors.clear();
            }
          });
        }
        return self;
      },
      addValidators: function(name) {
        var validators = [];
        for (var _i = 1; _i < arguments.length; _i++) {
          validators[_i - 1] = arguments[_i];
        }
        var _a;
        (_a = getOrAddFieldState(name).validators).push.apply(
          _a,
          tslib_1.__spread(validators)
        );
        return self;
      },
      removeValidators: function(name) {
        var validators = [];
        for (var _i = 1; _i < arguments.length; _i++) {
          validators[_i - 1] = arguments[_i];
        }
        var fieldState = fields.get(name);
        if (fieldState) {
          validators.forEach(function(validator) {
            fieldState.validators.remove(validator);
          });
        }
        return self;
      },
      setTouched: function(name, isTouched) {
        if (isTouched === void 0) {
          isTouched = true;
        }
        if (isTouched) {
          getOrAddFieldState(name).isTouched = true;
        } else {
          var fieldState = fields.get(name);
          if (fieldState) {
            fieldState.isTouched = false;
          }
        }
        return self;
      },
      setUntouched: function() {
        fields.forEach(function(fieldState) {
          fieldState.isTouched = false;
        });
        return self;
      },
      setChanged: function(name, isChanged) {
        if (isChanged === void 0) {
          isChanged = true;
        }
        if (isChanged) {
          var oldValue = self[name];
          handleFieldChange(name, oldValue);
        } else {
          var fieldState = fields.get(name);
          if (fieldState) {
            fieldState.isChanged = false;
            fieldState.baseValue = undefined;
          }
        }
        return self;
      },
      setUnchanged: function() {
        fields.forEach(function(fieldState) {
          fieldState.isChanged = false;
          fieldState.baseValue = undefined;
        });
        return self;
      },
      restoreBaseValue: function(name) {
        var fieldState = fields.get(name);
        if (fieldState && fieldState.isChanged) {
          self[name] = fieldState.baseValue;
          fieldState.isChanged = false;
          fieldState.baseValue = undefined;
        }
        return self;
      },
      restoreBaseValues: function() {
        fields.forEach(function(fieldState, name) {
          if (fieldState.isChanged) {
            self[name] = fieldState.baseValue;
            fieldState.isChanged = false;
            fieldState.baseValue = undefined;
          }
        });
        return self;
      },
      setFocus: function(name, hasFocus) {
        if (hasFocus === void 0) {
          hasFocus = true;
        }
        if (hasFocus) {
          getOrAddFieldState(name).hasFocus = true;
        } else {
          var fieldState = fields.get(name);
          if (fieldState) {
            fieldState.hasFocus = false;
          }
        }
        return self;
      }
    },
    views: {
      isEdited: function(name) {
        var e_1, _a;
        if (name) {
          var fieldState = fields.get(name);
          return fieldState ? fieldState.isEdited : false;
        }
        try {
          for (
            var _b = tslib_1.__values(fields.values()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var fieldState = _c.value;
            if (fieldState.isEdited) {
              return true;
            }
          }
        } catch (e_1_1) {
          e_1 = { error: e_1_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_1) throw e_1.error;
          }
        }
        return false;
      },
      isTouched: function(name) {
        var e_2, _a;
        if (name) {
          var fieldState = fields.get(name);
          return fieldState ? fieldState.isTouched : false;
        }
        try {
          for (
            var _b = tslib_1.__values(fields.values()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var fieldState = _c.value;
            if (fieldState.isTouched) {
              return true;
            }
          }
        } catch (e_2_1) {
          e_2 = { error: e_2_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_2) throw e_2.error;
          }
        }
        return false;
      },
      isChanged: function(name) {
        var e_3, _a;
        if (name) {
          var fieldState = fields.get(name);
          return fieldState ? fieldState.isChanged : false;
        }
        try {
          for (
            var _b = tslib_1.__values(fields.values()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var fieldState = _c.value;
            if (fieldState.isChanged) {
              return true;
            }
          }
        } catch (e_3_1) {
          e_3 = { error: e_3_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_3) throw e_3.error;
          }
        }
        return false;
      },
      getBaseValue: function(name) {
        var fieldState = fields.get(name);
        return fieldState && fieldState.isChanged
          ? fieldState.baseValue
          : self[name];
      },
      hasFocus: function(name) {
        var fieldState = fields.get(name);
        return fieldState ? fieldState.hasFocus : false;
      },
      hasErrors: function(name) {
        var e_4, _a;
        if (name) {
          var fieldState = fields.get(name);
          return fieldState ? fieldState.hasErrors : false;
        }
        try {
          for (
            var _b = tslib_1.__values(fields.values()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var fieldState = _c.value;
            if (fieldState.hasErrors) {
              return true;
            }
          }
        } catch (e_4_1) {
          e_4 = { error: e_4_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_4) throw e_4.error;
          }
        }
        return false;
      },
      hasVisibleErrors: function(name) {
        var e_5, _a;
        if (name) {
          var fieldState = fields.get(name);
          return fieldState ? fieldState.hasVisibleErrors : false;
        }
        try {
          for (
            var _b = tslib_1.__values(fields.values()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var fieldState = _c.value;
            if (fieldState.hasVisibleErrors) {
              return true;
            }
          }
        } catch (e_5_1) {
          e_5 = { error: e_5_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_5) throw e_5.error;
          }
        }
        return false;
      },
      getErrors: function(name) {
        var fieldState = fields.get(name);
        return fieldState ? fieldState.allErrors : null;
      },
      getVisibleErrors: function(name) {
        var fieldState = fields.get(name);
        return fieldState ? fieldState.visibleErrors : null;
      },
      getAllErrors: function() {
        var e_6, _a;
        var modelErrors = {};
        var hasErrors = false;
        try {
          for (
            var _b = tslib_1.__values(fields.entries()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var _d = tslib_1.__read(_c.value, 2),
              fieldName = _d[0],
              fieldState = _d[1];
            if (fieldState.hasErrors) {
              modelErrors[fieldName] = fieldState.allErrors;
              hasErrors = true;
            }
          }
        } catch (e_6_1) {
          e_6 = { error: e_6_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_6) throw e_6.error;
          }
        }
        return hasErrors ? modelErrors : null;
      },
      getAllVisibleErrors: function() {
        var e_7, _a;
        var modelErrors = {};
        var hasErrors = false;
        try {
          for (
            var _b = tslib_1.__values(fields.entries()), _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var _d = tslib_1.__read(_c.value, 2),
              fieldName = _d[0],
              fieldState = _d[1];
            if (fieldState.hasVisibleErrors) {
              modelErrors[fieldName] = fieldState.allErrors;
              hasErrors = true;
            }
          }
        } catch (e_7_1) {
          e_7 = { error: e_7_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_7) throw e_7.error;
          }
        }
        return hasErrors ? modelErrors : null;
      }
    }
  };
};
function sutructuralEquals(first, second) {
  if (first === second) {
    return true;
  }
  var firstSnapshot = isStateTreeNode(first) ? getSnapshot(first) : first;
  var secondSnapshot = isStateTreeNode(second) ? getSnapshot(second) : second;
  return comparer.structural(firstSnapshot, secondSnapshot);
}
var actionDecorators = {};
Object.keys(validationMixin(observable({})).actions).forEach(function(key) {
  actionDecorators[key] = action;
});
/**
 * Утилита для MobX, добавляющая интерфейс @see ValidatableObject
 * @example
 * const product = validatable(observable({
 *   Title: "",
 *   Order: 0,
 * }))
 */
export function validatable(target) {
  var _a = validationMixin(target),
    actions = _a.actions,
    views = _a.views;
  for (var name_1 in views) {
    target[name_1] = views[name_1];
  }
  extendObservable(target, actions, actionDecorators, SHALLOW);
  return target;
}
/**
 * Mixin для MobX, реализующий интерфейс @see ValidatableObject
 * @example
 * class Entity {
 *   Id: number;
 * }
 *
 * class Product extends Validatable(Entity) {
 *   @observable Title: string;
 *   @observable Order: number;
 * }
 */
export function Validatable(constructor) {
  if (!constructor) {
    constructor = /** @class */ (function() {
      function class_1() {}
      return class_1;
    })();
  }
  return /** @class */ (function(_super) {
    tslib_1.__extends(class_2, _super);
    function class_2() {
      var args = [];
      for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
      }
      var _this = _super.apply(this, tslib_1.__spread(args)) || this;
      validatable(_this);
      return _this;
    }
    return class_2;
  })(constructor);
}
/**
 * React компонент для декларативного описания валидации поля
 * @example
 * <Validate silent model={article} name="Title" rules={[required, pattern(/^[A-Z]+$/i)]} />
 *
 * <Validate name="Title" model={article} rules={required}>
 *   <input value={article.Title} onChange={e => (article.Title = e.target.value)} />
 * </Validate>
 */
var Validate = /** @class */ (function(_super) {
  tslib_1.__extends(Validate, _super);
  function Validate() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.handleFocus = function() {
      var _a = _this.props,
        model = _a.model,
        name = _a.name;
      transaction(function() {
        model.setFocus(name, true);
        model.setTouched(name, true);
      });
    };
    _this.handleChange = function() {
      var _a = _this.props,
        model = _a.model,
        name = _a.name;
      model.setTouched(name, true);
    };
    _this.handleBlur = function() {
      var _a = _this.props,
        model = _a.model,
        name = _a.name;
      model.setFocus(name, false);
    };
    return _this;
  }
  Validate.prototype.componentDidMount = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      rules = _a.rules;
    if (rules) {
      var validators = (isArray(rules) ? rules : [rules]).filter(Boolean);
      if (validators.length > 0) {
        this._validators = validators;
        model.addValidators.apply(
          model,
          tslib_1.__spread([name], this._validators)
        );
      }
    }
  };
  Validate.prototype.componentWillUnmount = function() {
    if (this._validators) {
      var _a = this.props,
        model = _a.model,
        name_2 = _a.name;
      model.removeValidators.apply(
        model,
        tslib_1.__spread([name_2], this._validators)
      );
    }
  };
  Validate.prototype.render = function() {
    var _a = this.props,
      silent = _a.silent,
      children = _a.children;
    if (silent) {
      return children ? this.renderWrapper(children) : null;
    }
    if (children) {
      return this.renderWrapper(children, this.renderErrors());
    }
    return this.renderErrors();
  };
  Validate.prototype.renderWrapper = function(children, errors) {
    var className = this.props.className;
    return React.createElement(
      "div",
      {
        className: className,
        onFocus: this.handleFocus,
        onChange: this.handleChange,
        onBlur: this.handleBlur
      },
      children,
      errors
    );
  };
  Validate.prototype.renderErrors = function() {
    var _a = this.props,
      model = _a.model,
      name = _a.name,
      errorClassName = _a.errorClassName,
      renderErrors = _a.renderErrors;
    if (model.hasVisibleErrors(name)) {
      var errors = model.getVisibleErrors(name);
      return renderErrors
        ? renderErrors.apply(void 0, tslib_1.__spread(errors))
        : React.createElement(
            "div",
            { className: errorClassName },
            errors.map(function(error, i) {
              return React.createElement("div", { key: i }, error);
            })
          );
    }
    return null;
  };
  return Validate;
})(Component);
export { Validate };
//# sourceMappingURL=mst-validation-mixin.js.map
