import * as tslib_1 from "tslib";
import React from "react";
import cn from "classnames";
import { inject } from "react-ioc";
import { action, untracked, computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Button, Tab, Tabs } from "@blueprintjs/core";
import { isString, isNullOrWhiteSpace } from "Utils/TypeChecks";
import { DataContext } from "Services/DataContext";
import { EntityController } from "Services/EntityController";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldEditor } from "Components/FieldEditors/AbstractFieldEditor";
import "./RelationFieldTabs.scss";
var defaultRelationHandler = function(action) {
  return action();
};
var defaultEntityHandler = function(_entity, action) {
  return action();
};
/** Отображение множественного поля-связи в виде вкладок (возможно вертикальных) */
var RelationFieldTabs = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldTabs, _super);
  function RelationFieldTabs(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this.state = {
      isOpen: !_this.props.collapsed,
      isTouched: !_this.props.collapsed,
      activeId: null
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
                    activeId: clone._ClientId,
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
          model[fieldSchema.FieldName].push(entity);
          model.setTouched(fieldSchema.FieldName, true);
          _this.setState({
            activeId: entity._ClientId,
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
          var nextEntity = _this.getNextTab(entity);
          var array = model[fieldSchema.FieldName];
          if (array) {
            array.remove(entity);
            model.setTouched(fieldSchema.FieldName, true);
          }
          _this.deactivateTab(entity, nextEntity);
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
            var nextEntity, isRemoved;
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  nextEntity = this.getNextTab(entity);
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
                    this.deactivateTab(entity, nextEntity);
                  }
                  return [2 /*return*/, isRemoved];
              }
            });
          });
        })
      );
    };
    _this.clearRelations = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onClearRelation = _a.onClearRelation;
      onClearRelation(
        action("clearRelations", function() {
          model[fieldSchema.FieldName].replace([]);
          model.setTouched(fieldSchema.FieldName, true);
          _this.setState({
            activeId: null,
            isOpen: false,
            isTouched: false
          });
        })
      );
    };
    _this.selectRelations = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onSelectRelation = _a.onSelectRelation;
      onSelectRelation(
        action("selectRelations", function() {
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
                    this._relationController.selectRelations(model, fieldSchema)
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
    _this.reloadRelations = function() {
      var _a = _this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        onReloadRelation = _a.onReloadRelation;
      onReloadRelation(
        action("reloadRelations", function() {
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
                    this._relationController.reloadRelations(model, fieldSchema)
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
    _this.handleTabChange = function(newTabId, _prevTabId, _e) {
      _this.setState({
        activeId: newTabId
      });
    };
    var _a = props,
      fieldSchema = _a.fieldSchema,
      sortItems = _a.sortItems,
      sortItemsBy = _a.sortItemsBy,
      displayField = _a.displayField;
    _this._displayField = _this.makeDisplayFieldSelector(
      displayField,
      fieldSchema
    );
    _this._entityComparer = _this.makeEntityComparer(
      sortItems || sortItemsBy,
      fieldSchema
    );
    untracked(function() {
      var array = props.model[fieldSchema.FieldName];
      if (array.length > 0) {
        var firstArticle = array[0];
        _this.state.activeId = firstArticle._ClientId;
      }
    });
    return _this;
  }
  RelationFieldTabs_1 = RelationFieldTabs;
  Object.defineProperty(RelationFieldTabs.prototype, "dataSource", {
    get: function() {
      var _a = this.props,
        model = _a.model,
        fieldSchema = _a.fieldSchema,
        filterItems = _a.filterItems;
      var array = model[fieldSchema.FieldName];
      return array && array.filter(filterItems).sort(this._entityComparer);
    },
    enumerable: true,
    configurable: true
  });
  RelationFieldTabs.prototype.cloneEntity = function(entity) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var _a, model, fieldSchema, onCloneEntity;
      var _this = this;
      return tslib_1.__generator(this, function(_b) {
        (_a = this.props),
          (model = _a.model),
          (fieldSchema = _a.fieldSchema),
          (onCloneEntity = _a.onCloneEntity);
        onCloneEntity(
          entity,
          action("cloneEntity", function() {
            return tslib_1.__awaiter(_this, void 0, void 0, function() {
              var clone;
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
                    clone = _a.sent();
                    this.setState({
                      activeId: clone._ClientId,
                      isOpen: true,
                      isTouched: true
                    });
                    return [2 /*return*/, clone];
                }
              });
            });
          })
        );
        return [2 /*return*/];
      });
    });
  };
  RelationFieldTabs.prototype.getNextTab = function(entity) {
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema;
    return untracked(function() {
      var array = model[fieldSchema.FieldName];
      var index = array.indexOf(entity);
      return index > 0 ? array[index - 1] : array.length > 1 ? array[1] : null;
    });
  };
  RelationFieldTabs.prototype.deactivateTab = function(entity, nextEntity) {
    var activeId = this.state.activeId;
    if (activeId === entity._ClientId) {
      if (nextEntity) {
        this.setState({ activeId: nextEntity._ClientId });
      } else {
        this.setState({ activeId: null });
      }
    }
  };
  RelationFieldTabs.prototype.getTitle = function(entity) {
    var title = this._displayField(entity);
    return isNullOrWhiteSpace(title) ? "..." : title;
  };
  RelationFieldTabs.prototype.render = function() {
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
  RelationFieldTabs.prototype.renderControls = function(model, fieldSchema) {
    var _a = this.props,
      relationActions = _a.relationActions,
      canCreateEntity = _a.canCreateEntity,
      canSelectRelation = _a.canSelectRelation,
      canClearRelation = _a.canClearRelation,
      canReloadRelation = _a.canReloadRelation,
      canClonePrototype = _a.canClonePrototype;
    var isOpen = this.state.isOpen;
    var list = model[fieldSchema.FieldName];
    var isEmpty = !list || list.length === 0;
    return React.createElement(
      "div",
      { className: "relation-field-tabs__controls" },
      React.createElement(
        RelationFieldMenu,
        {
          onCreate: canCreateEntity && !this._readonly && this.createEntity,
          onSelect:
            canSelectRelation && !this._readonly && this.selectRelations,
          onClear:
            canClearRelation &&
            !this._readonly &&
            !isEmpty &&
            this.clearRelations,
          onReload:
            canReloadRelation && model._ServerId > 0 && this.reloadRelations,
          onClonePrototype:
            canClonePrototype && model._ServerId > 0 && this.clonePrototype
        },
        relationActions && relationActions()
      ),
      React.createElement(
        Button,
        {
          small: true,
          disabled: isEmpty,
          rightIcon: isOpen ? "chevron-up" : "chevron-down",
          onClick: this.toggleEditor
        },
        isOpen ? "Свернуть" : "Развернуть"
      )
    );
  };
  RelationFieldTabs.prototype.renderField = function(model, fieldSchema) {
    var _this = this;
    var _a = this.props,
      skipOtherFields = _a.skipOtherFields,
      fieldOrders = _a.fieldOrders,
      fieldEditors = _a.fieldEditors,
      vertical = _a.vertical,
      renderAllTabs = _a.renderAllTabs,
      className = _a.className,
      titleField = _a.titleField,
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
      isTouched = _b.isTouched,
      activeId = _b.activeId;
    var dataSource = this.dataSource;
    var contentSchema = fieldSchema.RelatedContent;
    var isEmpty = !dataSource || dataSource.length === 0;
    var isSingle = dataSource && dataSource.length === 1;
    var tabId = RelationFieldTabs_1._tabIdsByModel.get(model);
    if (!tabId) {
      tabId = RelationFieldTabs_1._nextTabId++;
      RelationFieldTabs_1._tabIdsByModel.set(model, tabId);
    }
    return React.createElement(
      Tabs,
      {
        renderActiveTabPanelOnly: !renderAllTabs,
        vertical: vertical,
        id: tabId + "_" + fieldSchema.FieldName,
        className: cn("relation-field-tabs", className, {
          "relation-field-tabs--hidden": !isOpen,
          "relation-field-tabs--empty": isEmpty,
          "relation-field-tabs--single": isSingle,
          "container-md": vertical && !className
        }),
        selectedTabId: activeId,
        onChange: this.handleTabChange
      },
      isTouched &&
        dataSource &&
        dataSource.map(function(entity) {
          var title = _this.getTitle(entity);
          var shouldRender = renderAllTabs || activeId === entity._ClientId;
          var isEdited = contentSchema.isEdited(entity);
          var hasVisibleErrors = contentSchema.hasVisibleErrors(entity);
          return React.createElement(
            Tab,
            {
              key: entity._ClientId,
              id: entity._ClientId,
              panel:
                shouldRender &&
                React.createElement(
                  EntityEditor,
                  {
                    model: entity,
                    contentSchema: fieldSchema.RelatedContent,
                    skipOtherFields: skipOtherFields,
                    fieldOrders: fieldOrders,
                    fieldEditors: fieldEditors,
                    titleField: titleField,
                    withHeader: true,
                    onMountEntity: onMountEntity,
                    onUnmountEntity: onUnmountEntity,
                    onSaveEntity: onSaveEntity,
                    onRefreshEntity: onRefreshEntity,
                    onReloadEntity: onReloadEntity,
                    onPublishEntity: onPublishEntity,
                    onCloneEntity: _this.cloneEntity,
                    onDetachEntity: _this.detachEntity,
                    onRemoveEntity: _this.removeEntity,
                    canSaveEntity: canSaveEntity,
                    canRefreshEntity: canRefreshEntity,
                    canReloadEntity: canReloadEntity,
                    canDetachEntity: !_this._readonly && canDetachEntity,
                    canRemoveEntity: canRemoveEntity,
                    canPublishEntity: canPublishEntity,
                    canCloneEntity: canCloneEntity,
                    customActions: entityActions
                  },
                  children
                )
            },
            React.createElement(
              "div",
              {
                className: cn("relation-field-tabs__title", {
                  "relation-field-tabs__title--edited": isEdited,
                  "relation-field-tabs__title--invalid": hasVisibleErrors
                }),
                title: isString(title) ? title : ""
              },
              title
            )
          );
        })
    );
  };
  var RelationFieldTabs_1;
  RelationFieldTabs.defaultProps = {
    filterItems: function() {
      return true;
    },
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
  RelationFieldTabs._nextTabId = 0;
  RelationFieldTabs._tabIdsByModel = new WeakMap();
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    RelationFieldTabs.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    RelationFieldTabs.prototype,
    "_entityController",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    RelationFieldTabs.prototype,
    "dataSource",
    null
  );
  RelationFieldTabs = RelationFieldTabs_1 = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    RelationFieldTabs
  );
  return RelationFieldTabs;
})(AbstractRelationFieldEditor);
export { RelationFieldTabs };
//# sourceMappingURL=RelationFieldTabs.js.map
