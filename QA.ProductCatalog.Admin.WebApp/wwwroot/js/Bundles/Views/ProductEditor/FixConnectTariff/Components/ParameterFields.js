import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { observable, runInAction, autorun, action } from "mobx";
import { observer } from "mobx-react";
import { WeakCache, ComputedCache } from "Utils/WeakCache";
import { asc } from "Utils/Array";
import { setEquals } from "Utils/Array";
import { DataContext } from "Services/DataContext";
import { ParameterBlock } from "./ParameterBlock";
var unitOptionsCache = new WeakCache();
var unitByAliasCache = new ComputedCache();
var baseParamsByAliasCache = new ComputedCache();
var directionByAliasCache = new ComputedCache();
var baseParamModifiersByAliasCache = new ComputedCache();
var paramModifiersByAliasCache = new ComputedCache();
var ParameterFields = /** @class */ (function(_super) {
  tslib_1.__extends(ParameterFields, _super);
  function ParameterFields() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.isMounted = false;
    _this.reactions = [];
    _this.fieldOrdersByTitile = {};
    return _this;
  }
  ParameterFields.prototype.componentDidMount = function() {
    var _this = this;
    this.createVirtualParameters();
    this.reactions.push(
      autorun(function() {
        var parameters = _this.getParameters();
        // find missing virtual parameters then add it to entity field
        var parametersToAdd = _this.virtualParameters.filter(function(virtual) {
          return virtual.BaseParameter
            ? !parameters.some(function(parameter) {
                return _this.hasSameTariffDerection(parameter, virtual);
              })
            : !parameters.some(function(parameter) {
                return parameter.Title === virtual.Title;
              });
        });
        runInAction("addVirtualParameters", function() {
          if (parametersToAdd.length > 0) {
            _this.resetParameters(parametersToAdd);
            parameters.push.apply(
              parameters,
              tslib_1.__spread(parametersToAdd)
            );
          }
          parameters.forEach(function(parameter) {
            // @ts-ignore
            parameter.setTouched("BaseParameter");
          });
        });
      })
    );
    this.reactions.push(
      autorun(function() {
        var parameters = _this.getParameters();
        // touch parameter NumValue and Value to capture reaction dependencies
        parameters.forEach(function(parameter) {
          return parameter.NumValue, parameter.Value;
        });
        runInAction("synchronizeParameters", function() {
          parameters.forEach(function(parameter) {
            var isVirtual = parameter.NumValue === null && !parameter.Value;
            if (parameter._IsVirtual !== isVirtual) {
              parameter._IsVirtual = isVirtual;
            }
          });
        });
      })
    );
    runInAction("componentDidMount", function() {
      _this.isMounted = true;
    });
  };
  ParameterFields.prototype.componentWillUnmount = function() {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            this.reactions.forEach(function(disposer) {
              return disposer();
            });
            // wait until all <ParameterBlock> are unmounted
            return [4 /*yield*/, Promise.resolve()];
          case 1:
            // wait until all <ParameterBlock> are unmounted
            _a.sent();
            this.deleteVirtualParameters();
            return [2 /*return*/];
        }
      });
    });
  };
  ParameterFields.prototype.createVirtualParameters = function() {
    var _this = this;
    var _a = this.props,
      _b = _a.fields,
      fields = _b === void 0 ? [] : _b,
      optionalBaseParamModifiers = _a.optionalBaseParamModifiers;
    if (DEBUG) {
      var invalidField = fields.find(function(field) {
        return (
          field.BaseParamModifiers &&
          field.BaseParamModifiers.some(function(modifier) {
            return optionalBaseParamModifiers.includes(modifier);
          })
        );
      });
      if (invalidField) {
        throw new Error(
          'Field "' +
            invalidField.Title +
            '" is invalid: BaseParameterModifiers ' +
            JSON.stringify(invalidField.BaseParamModifiers) +
            " intersects with optionalBaseParamModifiers " +
            JSON.stringify(optionalBaseParamModifiers)
        );
      }
    }
    var contentName = this.getContentName();
    var unitsByAlias = this.getUnitsByAlias();
    var baseParamsByAlias = this.getBaseParamsByAlias();
    var directionsByAlias = this.getDirectionsByAlias();
    var baseParamModifiersByAlias = this.getBaseParameterModifiersByAlias();
    var paramModifiersByAlias = this.getParameterModifiersByAlias();
    fields.forEach(function(field, i) {
      _this.fieldOrdersByTitile[field.Title] = i;
    });
    // create virtual paramters in context table
    this.virtualParameters = fields.map(function(field) {
      return _this._dataContext.createEntity(contentName, {
        _IsVirtual: true,
        Title: field.Title,
        Unit: unitsByAlias[field.Unit],
        BaseParameter: baseParamsByAlias[field.BaseParam],
        Direction: directionsByAlias[field.Direction],
        BaseParameterModifiers:
          field.BaseParamModifiers &&
          field.BaseParamModifiers.map(function(alias) {
            return baseParamModifiersByAlias[alias];
          }),
        Modifiers:
          field.Modifiers &&
          field.Modifiers.map(function(alias) {
            return paramModifiersByAlias[alias];
          })
      });
    });
  };
  ParameterFields.prototype.deleteVirtualParameters = function() {
    var _this = this;
    var parameters = this.getParameters();
    // remove virtual parameters from context table
    this.virtualParameters.forEach(function(virtual) {
      if (!parameters.includes(virtual)) {
        _this._dataContext.deleteEntity(virtual);
      }
    });
  };
  ParameterFields.prototype.hasSameTariffDerection = function(
    parameter,
    virtual
  ) {
    var optionalBaseParamModifiers = this.props.optionalBaseParamModifiers;
    return (
      parameter.BaseParameter === virtual.BaseParameter &&
      parameter.Zone === virtual.Zone &&
      parameter.Direction === virtual.Direction &&
      setEquals(
        parameter.BaseParameterModifiers.filter(function(alias) {
          return !optionalBaseParamModifiers.includes(alias.Alias);
        }),
        virtual.BaseParameterModifiers.filter(function(alias) {
          return !optionalBaseParamModifiers.includes(alias.Alias);
        })
      ) &&
      setEquals(parameter.Modifiers, virtual.Modifiers)
    );
  };
  ParameterFields.prototype.resetParameters = function(parameters) {
    var optionalBaseParamModifiers = this.props.optionalBaseParamModifiers;
    parameters.forEach(function(parameter) {
      parameter.NumValue = null;
      parameter.Value = null;
      // remove optional BaseParameterModifiers
      parameter.BaseParameterModifiers.replace(
        parameter.BaseParameterModifiers.filter(function(alias) {
          return !optionalBaseParamModifiers.includes(alias.Alias);
        })
      );
      parameter.setUnchanged();
      parameter.setUntouched();
      // @ts-ignore
      parameter.clearErrors();
    });
  };
  ParameterFields.prototype.getParameters = function() {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    return model[fieldSchema.FieldName];
  };
  ParameterFields.prototype.getContentName = function() {
    var fieldSchema = this.props.fieldSchema;
    return fieldSchema.RelatedContent.ContentName;
  };
  // BaseParameter.PreloadingMode должно быть PreloadingMode.Eager
  ParameterFields.prototype.getBaseParamsByAlias = function() {
    var _this = this;
    return baseParamsByAliasCache.getOrAdd(
      this._dataContext,
      { keepAlive: true },
      function() {
        var e_1, _a;
        var byAlias = {};
        try {
          for (
            var _b = tslib_1.__values(
                _this._dataContext.tables.BaseParameter.values()
              ),
              _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var baseParameter = _c.value;
            byAlias[baseParameter.Alias] = baseParameter;
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
        return byAlias;
      }
    );
  };
  // Direction.PreloadingMode должно быть PreloadingMode.Eager
  ParameterFields.prototype.getDirectionsByAlias = function() {
    var _this = this;
    return directionByAliasCache.getOrAdd(
      this._dataContext,
      { keepAlive: true },
      function() {
        var e_2, _a;
        var byAlias = {};
        try {
          for (
            var _b = tslib_1.__values(
                _this._dataContext.tables.Direction.values()
              ),
              _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var direction = _c.value;
            byAlias[direction.Alias] = direction;
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
        return byAlias;
      }
    );
  };
  // BaseParameterModifiers.PreloadingMode должно быть PreloadingMode.Eager
  ParameterFields.prototype.getBaseParameterModifiersByAlias = function() {
    var _this = this;
    return baseParamModifiersByAliasCache.getOrAdd(
      this._dataContext,
      { keepAlive: true },
      function() {
        var e_3, _a;
        var byAlias = {};
        try {
          for (
            var _b = tslib_1.__values(
                _this._dataContext.tables.BaseParameterModifier.values()
              ),
              _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var modifier = _c.value;
            byAlias[modifier.Alias] = modifier;
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
        return byAlias;
      }
    );
  };
  // Modifiers.PreloadingMode должно быть PreloadingMode.Eager
  ParameterFields.prototype.getParameterModifiersByAlias = function() {
    var _this = this;
    return paramModifiersByAliasCache.getOrAdd(
      this._dataContext,
      { keepAlive: true },
      function() {
        var e_4, _a;
        var byAlias = {};
        try {
          for (
            var _b = tslib_1.__values(
                _this._dataContext.tables.ParameterModifier.values()
              ),
              _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var modifier = _c.value;
            byAlias[modifier.Alias] = modifier;
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
        return byAlias;
      }
    );
  };
  // Unit.PreloadingMode должно быть PreloadingMode.Eager
  ParameterFields.prototype.getUnitsByAlias = function() {
    var _this = this;
    return unitByAliasCache.getOrAdd(
      this._dataContext,
      { keepAlive: true },
      function() {
        var e_5, _a;
        var byAlias = {};
        try {
          for (
            var _b = tslib_1.__values(_this._dataContext.tables.Unit.values()),
              _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var unit = _c.value;
            byAlias[unit.Alias] = unit;
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
        return byAlias;
      }
    );
  };
  ParameterFields.prototype.getOptionalBaseParameterModifiers = function() {
    var _a = this.props,
      optionalBaseParamModifiers = _a.optionalBaseParamModifiers,
      showBaseParamModifiers = _a.showBaseParamModifiers;
    if (!showBaseParamModifiers) {
      return null;
    }
    var baseParamModifiersByAlias = this.getBaseParameterModifiersByAlias();
    return optionalBaseParamModifiers.map(function(alias) {
      return baseParamModifiersByAlias[alias];
    });
  };
  // Unit.PreloadingMode должно быть PreloadingMode.Eager
  ParameterFields.prototype.getUnitOptions = function() {
    var fieldSchema = this.props.fieldSchema;
    var unitFieldSchema = fieldSchema.RelatedContent.Fields.Unit;
    return unitOptionsCache.getOrAdd(fieldSchema, function() {
      return unitFieldSchema.PreloadedArticles.map(function(unit) {
        return {
          value: unit._ClientId,
          label: unit.Title
        };
      });
    });
  };
  ParameterFields.prototype.render = function() {
    var _this = this;
    if (!this.isMounted) {
      return null;
    }
    var fields = this.props.fields;
    var fieldSchema = this.props.fieldSchema;
    var numValueSchema = fieldSchema.RelatedContent.Fields.NumValue;
    var parameters = this.getParameters();
    var unitOptions = this.getUnitOptions();
    var optionalBaseParamModifiers = this.getOptionalBaseParameterModifiers();
    return parameters
      .slice()
      .sort(
        fields
          ? asc(function(p) {
              return _this.fieldOrdersByTitile[p.Title];
            })
          : asc(function(p) {
              return p.Title;
            })
      )
      .map(function(parameter) {
        return React.createElement(ParameterBlock, {
          key: parameter._ClientId,
          parameter: parameter,
          allParameters: parameters,
          numValueSchema: numValueSchema,
          unitOptions: unitOptions,
          baseParamModifiers: optionalBaseParamModifiers
        });
      });
  };
  ParameterFields.defaultProps = {
    optionalBaseParamModifiers: ["LowerBound"]
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    ParameterFields.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [observable, tslib_1.__metadata("design:type", Object)],
    ParameterFields.prototype,
    "isMounted",
    void 0
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", []),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    ParameterFields.prototype,
    "createVirtualParameters",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", []),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    ParameterFields.prototype,
    "deleteVirtualParameters",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Array]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    ParameterFields.prototype,
    "resetParameters",
    null
  );
  ParameterFields = tslib_1.__decorate([observer], ParameterFields);
  return ParameterFields;
})(Component);
export { ParameterFields };
//# sourceMappingURL=ParameterFields.js.map
