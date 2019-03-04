import * as tslib_1 from "tslib";
import React, { Fragment } from "react";
import cn from "classnames";
import { inject } from "react-ioc";
import { action, computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Icon, Button } from "@blueprintjs/core";
import { ComputedCache } from "Utils/WeakCache";
import { DataContext } from "Services/DataContext";
import { EntityController } from "Services/EntityController";
import { EntityMenu } from "Components/ArticleEditor/EntityMenu";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { EntityLink } from "Components/ArticleEditor/EntityLink";
import { AbstractRelationFieldEditor } from "Components/FieldEditors/AbstractFieldEditor";
import "./RelationFieldAccordion.scss";
var defaultRelationHandler = function(action) {
  return action();
};
var defaultEntityHandler = function(_entity, action) {
  return action();
};
/** Отображение множественного поля-связи в виде раскрывающейся таблицы-аккордеона. */
var RelationFieldAccordion = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldAccordion, _super);
  function RelationFieldAccordion(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this._validationCache = new ComputedCache();
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
                  this.toggleScreen(clone);
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
          _this.toggleScreen(entity);
          return entity;
        })
      );
    };
    _this.publishEntity = function(e, entity) {
      e.stopPropagation();
      var _a = _this.props,
        fieldSchema = _a.fieldSchema,
        onPublishEntity = _a.onPublishEntity;
      onPublishEntity(
        entity,
        action("publishEntity", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._entityController.publishEntity(
                      entity,
                      fieldSchema.RelatedContent
                    )
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
    _this._displayFieldsNodeCache = new ComputedCache();
    var _a = props,
      fieldSchema = _a.fieldSchema,
      sortItems = _a.sortItems,
      sortItemsBy = _a.sortItemsBy,
      displayFields = _a.displayFields,
      columnProportions = _a.columnProportions;
    _this._displayFields = _this.makeDisplayFieldsSelectors(
      displayFields,
      fieldSchema
    );
    _this._entityComparer = _this.makeEntityComparer(
      sortItems || sortItemsBy,
      fieldSchema
    );
    _this._columnProportions = columnProportions;
    return _this;
  }
  Object.defineProperty(RelationFieldAccordion.prototype, "dataSource", {
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
  RelationFieldAccordion.prototype.detachEntity = function(e, entity) {
    var _this = this;
    e.stopPropagation();
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema,
      onDetachEntity = _a.onDetachEntity;
    onDetachEntity(
      entity,
      action("detachEntity", function() {
        var array = model[fieldSchema.FieldName];
        if (array) {
          array.remove(entity);
          model.setTouched(fieldSchema.FieldName, true);
        }
        _this.deactivateScreen(entity);
      })
    );
  };
  RelationFieldAccordion.prototype.removeEntity = function(e, entity) {
    var _this = this;
    e.stopPropagation();
    var _a = this.props,
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
                  this.deactivateScreen(entity);
                }
                return [2 /*return*/, isRemoved];
            }
          });
        });
      })
    );
  };
  RelationFieldAccordion.prototype.saveEntity = function(e, entity) {
    var _this = this;
    e.stopPropagation();
    var _a = this.props,
      fieldSchema = _a.fieldSchema,
      onSaveEntity = _a.onSaveEntity;
    var contentSchema = fieldSchema.RelatedContent;
    onSaveEntity(
      entity,
      action("saveEntity", function() {
        return tslib_1.__awaiter(_this, void 0, void 0, function() {
          return tslib_1.__generator(this, function(_a) {
            switch (_a.label) {
              case 0:
                return [
                  4 /*yield*/,
                  this._entityController.saveEntity(entity, contentSchema)
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
  RelationFieldAccordion.prototype.refreshEntity = function(e, entity) {
    var _this = this;
    e.stopPropagation();
    var _a = this.props,
      fieldSchema = _a.fieldSchema,
      onRefreshEntity = _a.onRefreshEntity;
    var contentSchema = fieldSchema.RelatedContent;
    onRefreshEntity(
      entity,
      action("refreshEntity", function() {
        return tslib_1.__awaiter(_this, void 0, void 0, function() {
          return tslib_1.__generator(this, function(_a) {
            switch (_a.label) {
              case 0:
                return [
                  4 /*yield*/,
                  this._entityController.refreshEntity(entity, contentSchema)
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
  RelationFieldAccordion.prototype.reloadEntity = function(e, entity) {
    var _this = this;
    e.stopPropagation();
    var _a = this.props,
      fieldSchema = _a.fieldSchema,
      onReloadEntity = _a.onReloadEntity;
    var contentSchema = fieldSchema.RelatedContent;
    onReloadEntity(
      entity,
      action("reloadEntity", function() {
        return tslib_1.__awaiter(_this, void 0, void 0, function() {
          return tslib_1.__generator(this, function(_a) {
            switch (_a.label) {
              case 0:
                return [
                  4 /*yield*/,
                  this._entityController.reloadEntity(entity, contentSchema)
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
  RelationFieldAccordion.prototype.cloneEntity = function(e, entity) {
    var _this = this;
    e.stopPropagation();
    var _a = this.props,
      model = _a.model,
      fieldSchema = _a.fieldSchema,
      onCloneEntity = _a.onCloneEntity;
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
                this.toggleScreen(clone);
                return [2 /*return*/, clone];
            }
          });
        });
      })
    );
  };
  RelationFieldAccordion.prototype.handleToggle = function(e, entity) {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;
    this.toggleScreen(entity);
  };
  RelationFieldAccordion.prototype.toggleScreen = function(entity) {
    var activeId = this.state.activeId;
    if (activeId === entity._ClientId) {
      this.setState({
        activeId: null,
        isOpen: true,
        isTouched: true
      });
    } else {
      this.setState({
        activeId: entity._ClientId,
        isOpen: true,
        isTouched: true
      });
    }
  };
  RelationFieldAccordion.prototype.deactivateScreen = function(entity) {
    var activeId = this.state.activeId;
    if (activeId === entity._ClientId) {
      this.setState({ activeId: null });
    }
  };
  RelationFieldAccordion.prototype.getBodyColSpan = function() {
    var columnProportions = this.props.columnProportions;
    return columnProportions
      ? columnProportions.reduce(function(sum, n) {
          return sum + n;
        }, 0) + 3
      : this._displayFields.length + 3;
  };
  RelationFieldAccordion.prototype.render = function() {
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
  RelationFieldAccordion.prototype.renderControls = function(
    model,
    fieldSchema
  ) {
    var _a = this.props,
      relationActions = _a.relationActions,
      canCreateEntity = _a.canCreateEntity,
      canClonePrototype = _a.canClonePrototype,
      canSelectRelation = _a.canSelectRelation,
      canClearRelation = _a.canClearRelation,
      canReloadRelation = _a.canReloadRelation;
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
  RelationFieldAccordion.prototype.renderField = function(_model, fieldSchema) {
    var _this = this;
    var _a = this.props,
      fieldOrders = _a.fieldOrders,
      fieldEditors = _a.fieldEditors,
      highlightItems = _a.highlightItems,
      validateItems = _a.validateItems,
      skipOtherFields = _a.skipOtherFields,
      entityActions = _a.entityActions,
      onMountEntity = _a.onMountEntity,
      onUnmountEntity = _a.onUnmountEntity,
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
    return isTouched && dataSource
      ? React.createElement(
          "table",
          {
            className: cn("relation-field-accordion", {
              "relation-field-accordion--hidden": !isOpen
            }),
            cellSpacing: "0",
            cellPadding: "0"
          },
          React.createElement(
            "tbody",
            null,
            dataSource.map(function(entity) {
              var isOpen = entity._ClientId === activeId;
              var hasServerId = entity._ServerId > 0;
              var isEdited = contentSchema.isEdited(entity);
              var hasVisibleErrors = contentSchema.hasVisibleErrors(entity);
              var highlightMode = highlightItems(entity);
              var highlight = highlightMode === 1; /* Highlight */
              var shade = highlightMode === 2; /* Shade */
              var itemError =
                validateItems &&
                _this._validationCache.getOrAdd(entity, function() {
                  return validateItems(entity);
                });
              return React.createElement(
                Fragment,
                { key: entity._ClientId },
                React.createElement(
                  "tr",
                  {
                    className: cn("relation-field-accordion__header", {
                      "relation-field-accordion__header--open": isOpen,
                      "relation-field-accordion__header--edited": isEdited,
                      "relation-field-accordion__header--invalid":
                        hasVisibleErrors || !!itemError,
                      "relation-field-accordion__header--highlight": highlight,
                      "relation-field-accordion__header--shade": shade
                    }),
                    title: itemError,
                    onClick: function(e) {
                      return _this.handleToggle(e, entity);
                    }
                  },
                  React.createElement(
                    "td",
                    {
                      className: "relation-field-accordion__expander",
                      title: isOpen ? "Свернуть" : "Развернуть"
                    },
                    React.createElement(Icon, {
                      icon: isOpen ? "caret-down" : "caret-right",
                      title: false
                    })
                  ),
                  _this._displayFieldsNodeCache.getOrAdd(entity, function() {
                    return React.createElement(
                      React.Fragment,
                      null,
                      React.createElement(
                        "td",
                        {
                          key: -1,
                          className: "relation-field-accordion__cell"
                        },
                        React.createElement(EntityLink, {
                          model: entity,
                          contentSchema: contentSchema
                        })
                      ),
                      _this._displayFields.map(function(displayField, i) {
                        return React.createElement(
                          "td",
                          {
                            key: i,
                            colSpan: _this._columnProportions
                              ? _this._columnProportions[i]
                              : 1,
                            className: "relation-field-accordion__cell"
                          },
                          displayField(entity)
                        );
                      })
                    );
                  }),
                  React.createElement(
                    "td",
                    { className: "relation-field-accordion__controls" },
                    isOpen &&
                      React.createElement(
                        EntityMenu,
                        {
                          small: true,
                          onSave:
                            canSaveEntity &&
                            function(e) {
                              return _this.saveEntity(e, entity);
                            },
                          onDetach:
                            canDetachEntity &&
                            !_this._readonly &&
                            function(e) {
                              return _this.detachEntity(e, entity);
                            },
                          onRemove:
                            canRemoveEntity &&
                            hasServerId &&
                            function(e) {
                              return _this.removeEntity(e, entity);
                            },
                          onRefresh:
                            canRefreshEntity &&
                            hasServerId &&
                            function(e) {
                              return _this.refreshEntity(e, entity);
                            },
                          onReload:
                            canReloadEntity &&
                            hasServerId &&
                            function(e) {
                              return _this.reloadEntity(e, entity);
                            },
                          onClone:
                            canCloneEntity &&
                            hasServerId &&
                            function(e) {
                              return _this.cloneEntity(e, entity);
                            },
                          onPublish:
                            canPublishEntity &&
                            hasServerId &&
                            function(e) {
                              return _this.publishEntity(e, entity);
                            }
                        },
                        entityActions && entityActions(entity)
                      )
                  )
                ),
                React.createElement(
                  "tr",
                  { className: "relation-field-accordion__main" },
                  React.createElement(
                    "td",
                    {
                      className: cn("relation-field-accordion__body", {
                        "relation-field-accordion__body--open": isOpen
                      }),
                      colSpan: _this.getBodyColSpan()
                    },
                    isOpen &&
                      React.createElement(
                        EntityEditor,
                        {
                          model: entity,
                          contentSchema: fieldSchema.RelatedContent,
                          fieldOrders: fieldOrders,
                          fieldEditors: fieldEditors,
                          skipOtherFields: skipOtherFields,
                          onMountEntity: onMountEntity,
                          onUnmountEntity: onUnmountEntity
                        },
                        children
                      )
                  )
                )
              );
            })
          )
        )
      : null;
  };
  RelationFieldAccordion.defaultProps = {
    filterItems: function() {
      return true;
    },
    highlightItems: function() {
      return 0 /* None */;
    },
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true,
    onClonePrototype: defaultRelationHandler,
    onCreateEntity: defaultRelationHandler,
    onCloneEntity: defaultEntityHandler,
    onSaveEntity: defaultEntityHandler,
    onRefreshEntity: defaultEntityHandler,
    onReloadEntity: defaultEntityHandler,
    onRemoveEntity: defaultEntityHandler,
    onPublishEntity: defaultEntityHandler,
    onDetachEntity: defaultEntityHandler,
    onSelectRelation: defaultRelationHandler,
    onReloadRelation: defaultRelationHandler,
    onClearRelation: defaultRelationHandler
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    RelationFieldAccordion.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    RelationFieldAccordion.prototype,
    "_entityController",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    RelationFieldAccordion.prototype,
    "dataSource",
    null
  );
  RelationFieldAccordion = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    RelationFieldAccordion
  );
  return RelationFieldAccordion;
})(AbstractRelationFieldEditor);
export { RelationFieldAccordion };
//# sourceMappingURL=RelationFieldAccordion.js.map
