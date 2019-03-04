import * as tslib_1 from "tslib";
import React from "react";
import { Col } from "react-flexbox-grid";
import { observer } from "mobx-react";
import cn from "classnames";
import {
  isMultiRelationField,
  PreloadingMode,
  PreloadingState
} from "Models/EditorSchemaModels";
import { isArray, isObject, isNullOrWhiteSpace } from "Utils/TypeChecks";
import { Select } from "Components/FormControls/FormControls";
import { AbstractRelationFieldEditor } from "../AbstractFieldEditor";
var optionsCache = new WeakMap();
/**
 * Отображение поля-связи в виде комбо-бокса с автокомплитом.
 * Требует @see PreloadingMode.Eager или @see PreloadingMode.Lazy
 */
var RelationFieldSelect = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldSelect, _super);
  function RelationFieldSelect(props, context) {
    var _this = _super.call(this, props, context) || this;
    var fieldSchema = props.fieldSchema;
    var displayField = _this.makeDisplayFieldSelector(
      props.displayField,
      fieldSchema
    );
    _this._getOption = function(entity) {
      var title = displayField(entity);
      return {
        value: entity._ClientId,
        label: isNullOrWhiteSpace(title) ? "..." : title
      };
    };
    _this._multiple = isMultiRelationField(props.fieldSchema);
    return _this;
  }
  RelationFieldSelect.prototype.getCachedOptions = function() {
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
  RelationFieldSelect.prototype.render = function() {
    return this.getCachedOptions().length > 0
      ? _super.prototype.render.call(this)
      : null;
  };
  RelationFieldSelect.prototype.renderField = function(model, fieldSchema) {
    var options = this.getCachedOptions();
    return React.createElement(
      Col,
      { xl: true, md: 6 },
      React.createElement(Select, {
        id: this._id,
        model: model,
        name: fieldSchema.FieldName,
        options: options,
        required: fieldSchema.IsRequired,
        multiple: this._multiple,
        disabled: this._readonly,
        className: cn({
          "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      })
    );
  };
  RelationFieldSelect = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    RelationFieldSelect
  );
  return RelationFieldSelect;
})(AbstractRelationFieldEditor);
export { RelationFieldSelect };
//# sourceMappingURL=RelationFieldSelect.js.map
