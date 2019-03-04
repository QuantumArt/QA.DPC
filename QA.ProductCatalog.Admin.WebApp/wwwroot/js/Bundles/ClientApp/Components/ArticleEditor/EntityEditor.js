import * as tslib_1 from "tslib";
import React from "react";
import { Col, Row } from "react-flexbox-grid";
import { inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityController } from "Services/EntityController";
import { isString, isFunction } from "Utils/TypeChecks";
import { EntityMenu } from "./EntityMenu";
import { EntityLink } from "./EntityLink";
import { AbstractEditor } from "./ArticleEditor";
import "./ArticleEditor.scss";
import { action } from "mobx";
var defaultEntityHandler = function(_entity, action) {
  return action();
};
/** Компонент для отображения и редактирования статьи-сущности */
var EntityEditor = /** @class */ (function(_super) {
  tslib_1.__extends(EntityEditor, _super);
  function EntityEditor(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this.saveEntity = function() {
      var _a = _this.props,
        model = _a.model,
        contentSchema = _a.contentSchema,
        onSaveEntity = _a.onSaveEntity;
      onSaveEntity(
        model,
        action("saveEntity", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._entityController.saveEntity(model, contentSchema)
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
    _this.refreshEntity = function() {
      var _a = _this.props,
        model = _a.model,
        contentSchema = _a.contentSchema,
        onRefreshEntity = _a.onRefreshEntity;
      onRefreshEntity(
        model,
        action("refreshEntity", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._entityController.refreshEntity(model, contentSchema)
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
    _this.reloadEntity = function() {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var _a, model, contentSchema, onReloadEntity;
        var _this = this;
        return tslib_1.__generator(this, function(_b) {
          (_a = this.props),
            (model = _a.model),
            (contentSchema = _a.contentSchema),
            (onReloadEntity = _a.onReloadEntity);
          onReloadEntity(
            model,
            action("reloadEntity", function() {
              return tslib_1.__awaiter(_this, void 0, void 0, function() {
                return tslib_1.__generator(this, function(_a) {
                  switch (_a.label) {
                    case 0:
                      return [
                        4 /*yield*/,
                        this._entityController.reloadEntity(
                          model,
                          contentSchema
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
          return [2 /*return*/];
        });
      });
    };
    _this.publishEntity = function() {
      var _a = _this.props,
        model = _a.model,
        contentSchema = _a.contentSchema,
        onPublishEntity = _a.onPublishEntity;
      onPublishEntity(
        model,
        action("publishEntity", function() {
          return tslib_1.__awaiter(_this, void 0, void 0, function() {
            return tslib_1.__generator(this, function(_a) {
              switch (_a.label) {
                case 0:
                  return [
                    4 /*yield*/,
                    this._entityController.publishEntity(model, contentSchema)
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
    _this.detachEntity = function() {
      var _a = _this.props,
        model = _a.model,
        onDetachEntity = _a.onDetachEntity;
      if (onDetachEntity) {
        onDetachEntity(model);
      } else if (DEBUG) {
        console.warn("EntityEditor `onDetach` is not defined");
      }
    };
    _this.removeEntity = function() {
      var _a = _this.props,
        model = _a.model,
        onRemoveEntity = _a.onRemoveEntity;
      if (onRemoveEntity) {
        onRemoveEntity(model);
      } else if (DEBUG) {
        console.warn("EntityEditor `onRemove` is not defined");
      }
    };
    _this.cloneEntity = function() {
      var _a = _this.props,
        model = _a.model,
        onCloneEntity = _a.onCloneEntity;
      if (onCloneEntity) {
        onCloneEntity(model);
      } else if (DEBUG) {
        console.warn("EntityEditor `onClone` is not defined");
      }
    };
    var _a = _this.props,
      contentSchema = _a.contentSchema,
      _b = _a.titleField,
      titleField =
        _b === void 0
          ? contentSchema.DisplayFieldName ||
            function() {
              return "";
            }
          : _b;
    _this._titleField = isString(titleField)
      ? function(entity) {
          return entity[titleField];
        }
      : titleField;
    return _this;
  }
  EntityEditor.prototype.componentDidMount = function() {
    var _a = this.props,
      model = _a.model,
      onMountEntity = _a.onMountEntity;
    if (onMountEntity) {
      onMountEntity(model);
    }
  };
  EntityEditor.prototype.componentWillUnmount = function() {
    var _a = this.props,
      model = _a.model,
      onUnmountEntity = _a.onUnmountEntity;
    if (onUnmountEntity) {
      onUnmountEntity(model);
    }
  };
  EntityEditor.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      contentSchema = _a.contentSchema,
      className = _a.className,
      children = _a.children;
    return React.createElement(
      React.Fragment,
      null,
      this.renderHeader(),
      React.createElement(
        Col,
        { key: 2, md: true, className: className },
        React.createElement(Row, null, _super.prototype.render.call(this)),
        isFunction(children) &&
          React.createElement(Row, null, children(model, contentSchema))
      )
    );
  };
  EntityEditor.prototype.renderHeader = function() {
    var _a = this.props,
      model = _a.model,
      contentSchema = _a.contentSchema,
      withHeader = _a.withHeader;
    return withHeader === true
      ? React.createElement(
          Col,
          { key: 1, md: true, className: "entity-editor__header" },
          React.createElement(
            "div",
            {
              className: "entity-editor__title",
              title:
                contentSchema.ContentDescription ||
                contentSchema.ContentTitle ||
                contentSchema.ContentName
            },
            React.createElement(EntityLink, {
              model: model,
              contentSchema: contentSchema
            }),
            this._titleField(model)
          ),
          this.renderButtons()
        )
      : withHeader || null;
  };
  EntityEditor.prototype.renderButtons = function() {
    var _a = this.props,
      model = _a.model,
      customActions = _a.customActions,
      canSaveEntity = _a.canSaveEntity,
      canRefreshEntity = _a.canRefreshEntity,
      canReloadEntity = _a.canReloadEntity,
      canDetachEntity = _a.canDetachEntity,
      canRemoveEntity = _a.canRemoveEntity,
      canPublishEntity = _a.canPublishEntity,
      canCloneEntity = _a.canCloneEntity;
    var hasServerId = model._ServerId > 0;
    return React.createElement(
      "div",
      { className: "entity-editor__buttons" },
      React.createElement(
        EntityMenu,
        {
          onSave: canSaveEntity && this.saveEntity,
          onDetach: canDetachEntity && this.detachEntity,
          onRemove: canRemoveEntity && hasServerId && this.removeEntity,
          onRefresh: canRefreshEntity && hasServerId && this.refreshEntity,
          onReload: canReloadEntity && hasServerId && this.reloadEntity,
          onPublish: canPublishEntity && hasServerId && this.publishEntity,
          onClone: canCloneEntity && hasServerId && this.cloneEntity
        },
        customActions && customActions(model)
      )
    );
  };
  EntityEditor.defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    onSaveEntity: defaultEntityHandler,
    onRefreshEntity: defaultEntityHandler,
    onReloadEntity: defaultEntityHandler,
    onPublishEntity: defaultEntityHandler
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EntityController)],
    EntityEditor.prototype,
    "_entityController",
    void 0
  );
  EntityEditor = tslib_1.__decorate(
    [observer, tslib_1.__metadata("design:paramtypes", [Object, Object])],
    EntityEditor
  );
  return EntityEditor;
})(AbstractEditor);
export { EntityEditor };
//# sourceMappingURL=EntityEditor.js.map
