import * as tslib_1 from "tslib";
import React, { Component } from "react";
import qs from "qs";
import { provider, inject } from "react-ioc";
import { Observer } from "mobx-react";
import { Grid } from "react-flexbox-grid";
import { LocaleContext } from "react-lazy-i18n";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationsConfig } from "Components/ArticleEditor/ArticleEditor";
import { DataContext } from "Services/DataContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import { DataMerger } from "Services/DataMerger";
import { DataValidator } from "Services/DataValidator";
import { ActionController } from "Services/ActionController";
import { EntityController } from "Services/EntityController";
import { RelationController } from "Services/RelationController";
import { InitializationController } from "Services/InitializationController";
import { isFunction, isString, isObject } from "Utils/TypeChecks";
import {
  EditorSettings,
  EditorQueryParams,
  PublicationTrackerSettings
} from "Models/EditorSettingsModels";
import { FileController } from "Services/FileController";
import { SchemaLinker } from "Services/SchemaLinker";
import { SchemaCompiler } from "Services/SchemaCompiler";
import { PublicationContext } from "Services/PublicationContext";
import { PublicationTracker } from "Services/PublicationTracker";
import { OverlayPresenter } from "Services/OverlayPresenter";
/**
 * Корневой компонент редактора. Регистрирует необходимые сервисы, загружает схему и данные.
 */
var ProductEditor = /** @class */ (function(_super) {
  tslib_1.__extends(ProductEditor, _super);
  function ProductEditor(props, context) {
    var _this = _super.call(this, props, context) || this;
    _this.state = {
      entity: null,
      contentSchema: null
    };
    var _a = _this.props,
      editorSettings = _a.editorSettings,
      queryParams = _a.queryParams,
      relationEditors = _a.relationEditors,
      publicationTrackerSettings = _a.publicationTrackerSettings;
    Object.assign(_this._editorSettings, editorSettings);
    Object.assign(_this._relationsConfig, relationEditors);
    Object.assign(
      _this._publicationTrackerSettings,
      publicationTrackerSettings
    );
    if (isObject(queryParams)) {
      Object.assign(_this._queryParams, queryParams);
    } else {
      var queryString = isString(queryParams)
        ? queryParams
        : document.location.search;
      if (queryString.startsWith("?")) {
        queryString = queryString.slice(1);
      }
      Object.assign(_this._queryParams, qs.parse(queryString));
    }
    return _this;
  }
  ProductEditor.prototype.componentDidMount = function() {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var _a;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            _a = this.setState;
            return [4 /*yield*/, this._initializationController.initialize()];
          case 1:
            _a.apply(this, [_b.sent()]);
            return [2 /*return*/];
        }
      });
    });
  };
  ProductEditor.prototype.render = function() {
    var _this = this;
    var children = this.props.children;
    var _a = this.state,
      entity = _a.entity,
      contentSchema = _a.contentSchema;
    if (!entity || !contentSchema) {
      return null;
    }
    return React.createElement(
      LocaleContext.Provider,
      { value: this._editorSettings.UserLocale },
      React.createElement(
        Grid,
        { fluid: true },
        isFunction(children)
          ? children(entity, contentSchema)
          : React.createElement(EntityEditor, {
              model: entity,
              contentSchema: contentSchema
            })
      ),
      React.createElement(Observer, null, function() {
        return _this._overlayPresenter.overlays.peek();
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorSettings)],
    ProductEditor.prototype,
    "_editorSettings",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    ProductEditor.prototype,
    "_queryParams",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", RelationsConfig)],
    ProductEditor.prototype,
    "_relationsConfig",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationTrackerSettings)],
    ProductEditor.prototype,
    "_publicationTrackerSettings",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", InitializationController)],
    ProductEditor.prototype,
    "_initializationController",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", OverlayPresenter)],
    ProductEditor.prototype,
    "_overlayPresenter",
    void 0
  );
  ProductEditor = tslib_1.__decorate(
    [
      provider(
        DataContext,
        PublicationContext,
        PublicationTracker,
        DataNormalizer,
        DataSerializer,
        DataMerger,
        DataValidator,
        SchemaLinker,
        SchemaCompiler,
        ActionController,
        EntityController,
        InitializationController,
        FileController,
        RelationController,
        OverlayPresenter,
        RelationsConfig,
        EditorSettings,
        EditorQueryParams,
        PublicationTrackerSettings
      ),
      tslib_1.__metadata("design:paramtypes", [Object, Object])
    ],
    ProductEditor
  );
  return ProductEditor;
})(Component);
export { ProductEditor };
//# sourceMappingURL=ProductEditor.js.map
