import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { Checkbox, Radio } from "@blueprintjs/core";
import { observer } from "mobx-react";
import {
  isMultiRelationField,
  PreloadingMode,
  PreloadingState
} from "Models/EditorSchemaModels";
import { isArray, isObject } from "Utils/TypeChecks";
import { AbstractRelationFieldEditor } from "../AbstractFieldEditor";
import "./RelationFieldCheckList.scss";
var optionsCache = new WeakMap();
/**
 * Отображение поля-связи в виде списка чекбоксов.
 * Требует @see PreloadingMode.Eager или @see PreloadingMode.Lazy
 */
var RelationFieldCheckList = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldCheckList, _super);
  function RelationFieldCheckList(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this.toggleEntity = function(entity) {
      if (_this._readonly) {
        return;
      }
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema;
      if (_this._multiple) {
        var relation = model[fieldSchema.FieldName];
        if (relation.includes(entity)) {
          relation.remove(entity);
        } else {
          relation.push(entity);
        }
      } else if (model[fieldSchema.FieldName] !== entity) {
        model[fieldSchema.FieldName] = entity;
      }
      model.setTouched(fieldSchema.FieldName, true);
    };
    var fieldSchema = props.fieldSchema;
    var displayFields = _this.makeDisplayFieldsSelectors(
      props.displayFields,
      fieldSchema
    );
    _this._getOption = function(entity) {
      return {
        entity: entity,
        fields: displayFields.map(function(field) {
          return field(entity);
        })
      };
    };
    _this._multiple = isMultiRelationField(props.fieldSchema);
    return _this;
  }
  RelationFieldCheckList.prototype.getCachedOptions = function() {
    var fieldSchema = this.props.fieldSchema;
    var options = optionsCache.get(fieldSchema);
    if (options) {
      return options;
    }
    if (
      fieldSchema.PreloadingMode === PreloadingMode.Lazy &&
      fieldSchema.PreloadingState !== PreloadingState.Done
    ) {
      if (fieldSchema.PreloadingState === PreloadingState.NotStarted) {
        this._relationController.preloadRelationArticles(fieldSchema);
      }
      var relation = this.props.model[fieldSchema.FieldName];
      if (isArray(relation)) {
        return relation.map(this._getOption);
      }
      if (isObject(relation)) {
        return [this._getOption(relation)];
      }
      return [];
    }
    options = fieldSchema.PreloadedArticles.map(this._getOption);
    optionsCache.set(fieldSchema, options);
    return options;
  };
  RelationFieldCheckList.prototype.getSortedOptions = function(
    model,
    fieldSchema
  ) {
    var _this = this;
    var _a;
    var cachedOptions = this.getCachedOptions();
    var baseRelation = model.getBaseValue(fieldSchema.FieldName);
    if (
      this._baseRelation !== baseRelation ||
      this._cachedOptions !== cachedOptions
    ) {
      // очищаем кеш внутри компонента
      this._baseRelation = baseRelation;
      this._cachedOptions = cachedOptions;
      this._sortedOptions = [];
      if (isArray(baseRelation)) {
        var notSelectedOptons_1 = [];
        cachedOptions.forEach(function(option) {
          if (baseRelation.includes(option.entity)) {
            _this._sortedOptions.push(option);
          } else {
            notSelectedOptons_1.push(option);
          }
        });
        (_a = this._sortedOptions).push.apply(
          _a,
          tslib_1.__spread(notSelectedOptons_1)
        );
      } else if (isObject(baseRelation)) {
        cachedOptions.forEach(function(option) {
          if (baseRelation === option.entity) {
            _this._sortedOptions.unshift(option);
          } else {
            _this._sortedOptions.push(option);
          }
        });
      }
    }
    return this._sortedOptions;
  };
  RelationFieldCheckList.prototype.render = function() {
    return this.getCachedOptions().length > 0
      ? _super.prototype.render.call(this)
      : null;
  };
  RelationFieldCheckList.prototype.renderField = function(model, fieldSchema) {
    var _this = this;
    var sortedOptions = this.getSortedOptions(model, fieldSchema);
    var relation = model[fieldSchema.FieldName];
    return React.createElement(
      Col,
      { xl: true, md: 6 },
      React.createElement(
        "div",
        {
          className: cn("relation-field-check-list", {
            "relation-field-check-list--single": sortedOptions.length === 1,
            "relation-field-check-list--scroll": sortedOptions.length > 7
          })
        },
        React.createElement(
          "table",
          null,
          React.createElement(
            "tbody",
            null,
            sortedOptions.map(function(_a) {
              var entity = _a.entity,
                fields = _a.fields;
              return React.createElement(
                "tr",
                { key: entity._ClientId },
                React.createElement(
                  "td",
                  { key: -1 },
                  _this._multiple
                    ? React.createElement(Checkbox, {
                        checked: relation.includes(entity),
                        disabled: _this._readonly,
                        onChange: function() {
                          return _this.toggleEntity(entity);
                        }
                      })
                    : React.createElement(Radio, {
                        checked: entity === relation,
                        disabled: _this._readonly,
                        onChange: function() {
                          return _this.toggleEntity(entity);
                        }
                      })
                ),
                fields.map(function(field, i) {
                  return React.createElement(
                    "td",
                    {
                      key: i,
                      className: "relation-field-check-list__cell",
                      onClick: function() {
                        return _this.toggleEntity(entity);
                      }
                    },
                    field
                  );
                })
              );
            })
          )
        )
      )
    );
  };
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    RelationFieldCheckList.prototype,
    "toggleEntity",
    void 0
  );
  RelationFieldCheckList = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    RelationFieldCheckList
  );
  return RelationFieldCheckList;
})(AbstractRelationFieldEditor);
export { RelationFieldCheckList };
//# sourceMappingURL=RelationFieldCheckList.js.map
