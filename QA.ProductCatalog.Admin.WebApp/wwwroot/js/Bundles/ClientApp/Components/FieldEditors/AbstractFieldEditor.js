import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { Col, Row } from "react-flexbox-grid";
import { inject } from "react-ioc";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject } from "Models/EditorDataModels";
import { UpdatingMode, FieldExactTypes } from "Models/EditorSchemaModels";
import { RelationController } from "Services/RelationController";
import { isArray, isString } from "Utils/TypeChecks";
import { required } from "Utils/Validators";
import { newUid } from "Utils/Common";
import "./FieldEditors.scss";
import { asc } from "Utils/Array";
/** Абстрактный компонент для редактирования произвольного поля */
var AbstractFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(AbstractFieldEditor, _super);
  function AbstractFieldEditor() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this._id = "_" + newUid();
    _this._readonly =
      _this.props.readonly || _this.props.fieldSchema.IsReadOnly;
    return _this;
  }
  /** Отображение блока валидации */
  AbstractFieldEditor.prototype.renderValidation = function(
    model,
    fieldSchema
  ) {
    var validate = this.props.validate;
    var rules = [];
    if (validate) {
      if (isArray(validate)) {
        rules.push.apply(rules, tslib_1.__spread(validate));
      } else {
        rules.push(validate);
      }
    }
    if (fieldSchema.IsRequired) {
      rules.push(required);
    }
    return React.createElement(Validate, {
      model: model,
      name: fieldSchema.FieldName,
      errorClassName: "bp3-form-helper-text",
      rules: rules
    });
  };
  /** Отображение названия поля */
  AbstractFieldEditor.prototype.renderLabel = function(model, fieldSchema) {
    return React.createElement(
      "label",
      { htmlFor: this._id, title: fieldSchema.FieldName },
      fieldSchema.IsRequired &&
        React.createElement(
          "span",
          { className: "field-editor__label-required" },
          "*\u00A0"
        ),
      React.createElement(
        "span",
        {
          className: cn("field-editor__label-text", {
            "field-editor__label-text--edited": model.isEdited(
              fieldSchema.FieldName
            ),
            "field-editor__label-text--invalid": model.hasVisibleErrors(
              fieldSchema.FieldName
            )
          })
        },
        fieldSchema.FieldTitle || fieldSchema.FieldName,
        ":"
      ),
      fieldSchema.FieldDescription &&
        React.createElement(
          React.Fragment,
          null,
          "\u00A0",
          React.createElement(Icon, {
            icon: "help",
            title: fieldSchema.FieldDescription
          })
        )
    );
  };
  AbstractFieldEditor.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    return React.createElement(
      Col,
      {
        xl: 6,
        md: 12,
        className: cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })
      },
      React.createElement(
        Row,
        null,
        React.createElement(
          Col,
          { xl: 4, md: 3, className: "field-editor__label" },
          this.renderLabel(model, fieldSchema)
        ),
        this.renderField(model, fieldSchema)
      ),
      React.createElement(
        Row,
        null,
        React.createElement(Col, {
          xl: 4,
          md: 3,
          className: "field-editor__label"
        }),
        React.createElement(
          Col,
          { md: true },
          this.renderValidation(model, fieldSchema)
        )
      )
    );
  };
  return AbstractFieldEditor;
})(Component);
export { AbstractFieldEditor };
/** Абстрактный компонент для редактирования поля-связи */
var AbstractRelationFieldEditor = /** @class */ (function(_super) {
  tslib_1.__extends(AbstractRelationFieldEditor, _super);
  function AbstractRelationFieldEditor(props, context) {
    var _this = _super.call(this, props, context) || this;
    var fieldSchema = _this.props.fieldSchema;
    // фикс для M2ORelation
    _this._readonly =
      _this._readonly ||
      (fieldSchema.FieldType === FieldExactTypes.M2ORelation &&
        fieldSchema.UpdatingMode === UpdatingMode.Ignore);
    return _this;
  }
  /** Собрать функцию выбора поля для отображения в заголовке */
  AbstractRelationFieldEditor.prototype.makeDisplayFieldSelector = function(
    displayField,
    fieldSchema
  ) {
    var expression =
      displayField ||
      fieldSchema.RelatedContent.DisplayFieldName ||
      function() {
        return "";
      };
    return isString(expression)
      ? function(entity) {
          return entity[expression];
        }
      : expression;
  };
  /** Собрать набор функций выбора полей для отображения в заголовке */
  AbstractRelationFieldEditor.prototype.makeDisplayFieldsSelectors = function(
    displayFields,
    fieldSchema
  ) {
    var expressions = displayFields || fieldSchema.DisplayFieldNames || [];
    return expressions.map(function(field) {
      return isString(field)
        ? function(entity) {
            return entity[field];
          }
        : field;
    });
  };
  /** Собрать функцию сравнения статей по выбранным полям */
  AbstractRelationFieldEditor.prototype.makeEntityComparer = function(
    sortItems,
    fieldSchema
  ) {
    var expression =
      sortItems || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    if (isString(expression)) {
      return asc(function(entity) {
        return entity[expression];
      });
    }
    if (expression.length === 1) {
      return asc(expression);
    }
    if (expression.length === 2) {
      return expression;
    }
    throw new Error("Invalid `sortItems` parameter");
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", RelationController)],
    AbstractRelationFieldEditor.prototype,
    "_relationController",
    void 0
  );
  return AbstractRelationFieldEditor;
})(AbstractFieldEditor);
export { AbstractRelationFieldEditor };
//# sourceMappingURL=AbstractFieldEditor.js.map
