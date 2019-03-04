import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { inject } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Button } from "@blueprintjs/core";
import { DataContext } from "Services/DataContext";
import { EntityController } from "Services/EntityController";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldEditor } from "Components/FieldEditors/AbstractFieldEditor";
import "./RelationFieldForm.scss";
var defaultRelationHandler = function(action) {
  return action();
};
var defaultEntityHandler = function(_entity, action) {
  return action();
};
/** Отображение единичного поля-связи в виде раскрывающейся формы редактирования */
var RelationFieldForm = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldForm, _super);
  function RelationFieldForm() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.state = {
      isOpen: !_this.props.collapsed,
      isTouched: !_this.props.collapsed
    };
    _this.clonePrototype = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onClonePrototype = _a.onClonePrototype;
      onClonePrototype(
        action("clonePrototype", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            var clone;
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._relationController.cloneProductPrototype(
                      model,
                      fieldSchema
                    )
                  ];
                case 1:
                  clone = _a.sent();
                  this.setState({
                    isOpen: true,
                    isTouched: true
                  });
                  return [2 /*return*/, clone];
              }
            });
          });
        })
      );
    };
    _this.createEntity = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onCreateEntity = _a.onCreateEntity;
      var contentName = fieldSchema.RelatedContent.ContentName;
      onCreateEntity(
        action("createEntity", function() {
          var entity = _this._dataContext.createEntity(contentName);
          model[fieldSchema.FieldName] = entity;
          model.setTouched(fieldSchema.FieldName, true);
          _this.setState({
            isOpen: true,
            isTouched: true
          });
          return entity;
        })
      );
    };
    _this.detachEntity = function(entity) {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onDetachEntity = _a.onDetachEntity;
      onDetachEntity(
        entity,
        action("detachEntity", function() {
          model[fieldSchema.FieldName] = null;
          model.setTouched(fieldSchema.FieldName, true);
          _this.setState({
            isOpen: false,
            isTouched: false
          });
        })
      );
    };
    _this.removeEntity = function(entity) {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onRemoveEntity = _a.onRemoveEntity;
      onRemoveEntity(
        entity,
        action("removeEntity", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            var isRemoved;
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._entityController.removeRelatedEntity(
                      model,
                      fieldSchema,
                      entity
                    )
                  ];
                case 1:
                  isRemoved = _a.sent();
                  if (isRemoved) {
                    this.setState({
                      isOpen: false,
                      isTouched: false
                    });
                  }
                  return [2 /*return*/, isRemoved];
              }
            });
          });
        })
      );
    };
    _this.cloneEntity = function(entity) {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onCloneEntity = _a.onCloneEntity;
      onCloneEntity(
        entity,
        action("cloneEntity", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._entityController.cloneRelatedEntity(
                      model,
                      fieldSchema,
                      entity
                    )
                  ];
                case 1:
                  return [2 /*return*/, _a.sent()];
              }
            });
          });
        })
      );
    };
    _this.clearRelation = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onClearRelation = _a.onClearRelation;
      onClearRelation(
        action("clearRelation", function() {
          model[fieldSchema.FieldName] = null;
          model.setTouched(fieldSchema.FieldName, true);
          _this.setState({
            isOpen: false,
            isTouched: false
          });
        })
      );
    };
    _this.selectRelation = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onSelectRelation = _a.onSelectRelation;
      onSelectRelation(
        action("selectRelation", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  this.setState({
                    isOpen: true,
                    isTouched: true
                  });
                  return [
                    4 /*yield*/,
                    this._relationController.selectRelation(model, fieldSchema)
                  ];
                case 1:
                  _a.sent();
                  return [2 /*return*/];
              }
            });
          });
        })
      );
    };
    _this.reloadRelation = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onReloadRelation = _a.onReloadRelation;
      onReloadRelation(
        action("reloadRelation", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  this.setState({
                    isOpen: true,
                    isTouched: true
                  });
                  return [
                    4 /*yield*/,
                    this._relationController.reloadRelation(model, fieldSchema)
                  ];
                case 1:
                  _a.sent();
                  return [2 /*return*/];
              }
            });
          });
        })
      );
    };
    _this.toggleEditor = function() {
      var isOpen = _this.state.isOpen;
      _this.setState({
        isOpen: !isOpen,
        isTouched: true
      });
    };
    return _this;
  }
  RelationFieldForm.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    return React.createElement(
      Col,
      {
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
          {
            xl: 2,
            md: 3,
            className: "field-editor__label field-editor__label--small"
          },
          this.renderLabel(model, fieldSchema)
        ),
        React.createElement(
          Col,
          { md: true },
          this.renderControls(model, fieldSchema),
          this.renderValidation(model, fieldSchema)
        )
      ),
      React.createElement(
        Row,
        null,
        React.createElement(
          Col,
          { md: true },
          this.renderField(model, fieldSchema)
        )
      )
    );
  };
  RelationFieldForm.prototype.renderControls = function(model, fieldSchema) {
    var _a = this.props,
      relationActions = _a.relationActions,
      canCreateEntity = _a.canCreateEntity,
      canSelectRelation = _a.canSelectRelation,
      canClearRelation = _a.canClearRelation,
      canReloadRelation = _a.canReloadRelation,
      canClonePrototype = _a.canClonePrototype;
    var isOpen = this.state.isOpen;
    var entity = model[fieldSchema.FieldName];
    return React.createElement(
      "div",
      { className: "relation-field-form__controls" },
      React.createElement(
        RelationFieldMenu,
        {
          onCreate:
            canCreateEntity && !this._readonly && !entity && this.createEntity,
          onSelect: canSelectRelation && !this._readonly && this.selectRelation,
          onClear:
            canClearRelation &&
            !this._readonly &&
            !!entity &&
            this.clearRelation,
          onReload:
            canReloadRelation && model._ServerId > 0 && this.reloadRelation,
          onClonePrototype:
            canClonePrototype &&
            model._ServerId > 0 &&
            !entity &&
            this.clonePrototype
        },
        relationActions && relationActions()
      ),
      React.createElement(
        Button,
        {
          small: true,
          disabled: !entity,
          rightIcon: isOpen ? "chevron-up" : "chevron-down",
          onClick: this.toggleEditor
        },
        isOpen ? "Свернуть" : "Развернуть"
      )
    );
  };
  RelationFieldForm.prototype.renderField = function(model, fieldSchema) {
    var _a = this.props,
      skipOtherFields = _a.skipOtherFields,
      fieldOrders = _a.fieldOrders,
      fieldEditors = _a.fieldEditors,
      borderless = _a.borderless,
      className = _a.className,
      entityActions = _a.entityActions,
      onMountEntity = _a.onMountEntity,
      onUnmountEntity = _a.onUnmountEntity,
      onSaveEntity = _a.onSaveEntity,
      onRefreshEntity = _a.onRefreshEntity,
      onReloadEntity = _a.onReloadEntity,
      onPublishEntity = _a.onPublishEntity,
      canSaveEntity = _a.canSaveEntity,
      canRefreshEntity = _a.canRefreshEntity,
      canReloadEntity = _a.canReloadEntity,
      canDetachEntity = _a.canDetachEntity,
      canRemoveEntity = _a.canRemoveEntity,
      canPublishEntity = _a.canPublishEntity,
      canCloneEntity = _a.canCloneEntity,
      children = _a.children;
    var _b = this.state,
      isOpen = _b.isOpen,
      isTouched = _b.isTouched;
    var entity = model[fieldSchema.FieldName];
    return isTouched && entity
      ? React.createElement(
          "div",
          {
            className: cn("relation-field-form", className, {
              "relation-field-form--hidden": !isOpen,
              "relation-field-form--borderless": borderless
            })
          },
          React.createElement(
            EntityEditor,
            {
              withHeader: true,
              model: entity,
              contentSchema: fieldSchema.RelatedContent,
              skipOtherFields: skipOtherFields,
              fieldOrders: fieldOrders,
              fieldEditors: fieldEditors,
              onMountEntity: onMountEntity,
              onUnmountEntity: onUnmountEntity,
              onSaveEntity: onSaveEntity,
              onRefreshEntity: onRefreshEntity,
              onReloadEntity: onReloadEntity,
              onPublishEntity: onPublishEntity,
              onDetachEntity: this.detachEntity,
              onCloneEntity: this.cloneEntity,
              onRemoveEntity: this.removeEntity,
              canSaveEntity: canSaveEntity,
              canRefreshEntity: canRefreshEntity,
              canReloadEntity: canReloadEntity,
              canDetachEntity: !this._readonly && canDetachEntity,
              canRemoveEntity: canRemoveEntity,
              canPublishEntity: canPublishEntity,
              canCloneEntity: canCloneEntity,
              customActions: entityActions
            },
            children
          )
        )
      : null;
  };
  RelationFieldForm.defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true,
    onClonePrototype: defaultRelationHandler,
    onCreateEntity: defaultRelationHandler,
    onCloneEntity: defaultEntityHandler,
    onRemoveEntity: defaultEntityHandler,
    onDetachEntity: defaultEntityHandler,
    onSelectRelation: defaultRelationHandler,
    onReloadRelation: defaultRelationHandler,
    onClearRelation: defaultRelationHandler
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    RelationFieldForm.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    RelationFieldForm.prototype,
    "_entityController",
    void 0
  );
  RelationFieldForm = tslib_1.__decorate([observer], RelationFieldForm);
  return RelationFieldForm;
})(AbstractRelationFieldEditor);
export { RelationFieldForm };
//# sourceMappingURL=RelationFieldForm.js.map
