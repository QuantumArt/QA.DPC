import * as tslib_1 from "tslib";
import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { untracked, runInAction, isObservableArray } from "mobx";
import { Intent } from "@blueprintjs/core";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import { OverlayPresenter } from "Services/OverlayPresenter";
import {
  PreloadingState,
  isMultiRelationField,
  isSingleRelationField
} from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { EditorSettings, EditorQueryParams } from "Models/EditorSettingsModels";
import { trace, modal, progress, handleError } from "Utils/Decorators";
import { isArray } from "Utils/TypeChecks";
import { newUid, rootUrl } from "Utils/Common";
/** Действия со связями */
var RelationController = /** @class */ (function() {
  function RelationController() {
    var _this = this;
    this._callbackUid = newUid();
    this._observer = new QP8.BackendEventObserver(this._callbackUid, function(
      eventType,
      args
    ) {
      if (
        eventType === QP8.BackendEventTypes.EntitiesSelected &&
        isArray(args.selectedEntityIDs)
      ) {
        _this._resolvePromise(args.selectedEntityIDs);
      } else {
        _this._resolvePromise(CANCEL);
      }
    });
  }
  RelationController.prototype.dispose = function() {
    this._observer.dispose();
  };
  /** Открыть окно QP для выбора единичной связи */
  RelationController.prototype.selectRelation = function(model, fieldSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var existingArticleIds, selectedArticles;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            existingArticleIds = untracked(function() {
              var existingArticle = model[fieldSchema.FieldName];
              return existingArticle && existingArticle._ServerId > 0
                ? [existingArticle._ServerId]
                : [];
            });
            return [
              4 /*yield*/,
              this.selectArticles(existingArticleIds, fieldSchema, false)
            ];
          case 1:
            selectedArticles = _a.sent();
            if (selectedArticles !== CANCEL) {
              runInAction("selectRelation", function() {
                model[fieldSchema.FieldName] = selectedArticles[0] || null;
                model.setTouched(fieldSchema.FieldName, true);
              });
            }
            return [2 /*return*/];
        }
      });
    });
  };
  /** Открыть окно QP для выбора множественной связи */
  RelationController.prototype.selectRelations = function(model, fieldSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var existingArticleIds, selectedArticles;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            existingArticleIds = untracked(function() {
              var existingArticles = model[fieldSchema.FieldName];
              return existingArticles
                .map(function(article) {
                  return article._ServerId;
                })
                .filter(function(id) {
                  return id > 0;
                });
            });
            return [
              4 /*yield*/,
              this.selectArticles(existingArticleIds, fieldSchema, true)
            ];
          case 1:
            selectedArticles = _a.sent();
            if (selectedArticles !== CANCEL) {
              runInAction("selectRelations", function() {
                var relatedArticles = model[fieldSchema.FieldName];
                var newlyCreatedArticles = relatedArticles.filter(function(
                  article
                ) {
                  return article._ServerId === null;
                });
                relatedArticles.replace(
                  tslib_1.__spread(selectedArticles, newlyCreatedArticles)
                );
                model.setTouched(fieldSchema.FieldName, true);
              });
            }
            return [2 /*return*/];
        }
      });
    });
  };
  RelationController.prototype.selectArticles = function(
    existingArticleIds,
    fieldSchema,
    multiple
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var contentSchema, options, selectedArticleIds, articleToLoadIds;
      var _this = this;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            contentSchema = fieldSchema.RelatedContent;
            options = {
              selectActionCode: multiple
                ? "multiple_select_article"
                : "select_article",
              entityTypeCode: "article",
              parentEntityId: contentSchema.ContentId,
              selectedEntityIDs: existingArticleIds,
              isMultiple: multiple,
              callerCallback: this._callbackUid
            };
            if (fieldSchema.RelationCondition) {
              options.options = { filter: fieldSchema.RelationCondition };
            }
            QP8.openSelectWindow(
              options,
              this._queryParams.hostUID,
              window.parent
            );
            return [
              4 /*yield*/,
              new Promise(function(resolve) {
                _this._resolvePromise = resolve;
              })
            ];
          case 1:
            selectedArticleIds = _a.sent();
            if (selectedArticleIds === CANCEL) {
              return [2 /*return*/, CANCEL];
            }
            articleToLoadIds = selectedArticleIds.filter(function(id) {
              return !existingArticleIds.includes(id);
            });
            if (articleToLoadIds.length === 0) {
              return [
                2 /*return*/,
                this.getLoadedArticles(contentSchema, selectedArticleIds)
              ];
            }
            return [
              4 /*yield*/,
              this.loadSelectedArticles(contentSchema, articleToLoadIds)
            ];
          case 2:
            _a.sent();
            return [
              2 /*return*/,
              this.getLoadedArticles(contentSchema, selectedArticleIds)
            ];
        }
      });
    });
  };
  RelationController.prototype.getLoadedArticles = function(
    contentSchema,
    articleIds
  ) {
    var _this = this;
    return untracked(function() {
      var contextArticles =
        _this._dataContext.tables[contentSchema.ContentName];
      return articleIds.map(function(id) {
        return contextArticles.get(String(id));
      });
    });
  };
  RelationController.prototype.loadSelectedArticles = function(
    contentSchema,
    articleToLoadIds
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var response, _a, dataTrees, _b, _c, dataSnapshot;
      return tslib_1.__generator(this, function(_d) {
        switch (_d.label) {
          case 0:
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorQuery/LoadPartialProduct?" +
                  qs.stringify(this._queryParams),
                {
                  method: "POST",
                  credentials: "include",
                  headers: {
                    "Content-Type": "application/json"
                  },
                  body: JSON.stringify({
                    ProductDefinitionId: this._editorSettings
                      .ProductDefinitionId,
                    ContentPath: contentSchema.ContentPath,
                    ArticleIds: articleToLoadIds
                  })
                }
              )
            ];
          case 1:
            response = _d.sent();
            if (!!response.ok) return [3 /*break*/, 3];
            _a = Error.bind;
            return [4 /*yield*/, response.text()];
          case 2:
            throw new (_a.apply(Error, [void 0, _d.sent()]))();
          case 3:
            _c = (_b = this._dataSerializer).deserialize;
            return [4 /*yield*/, response.text()];
          case 4:
            dataTrees = _c.apply(_b, [_d.sent()]);
            dataSnapshot = this._dataNormalizer.normalizeAll(
              dataTrees,
              contentSchema.ContentName
            );
            this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Refresh);
            return [2 /*return*/];
        }
      });
    });
  };
  /** Перезагрузить с сервера поле единичной связи */
  RelationController.prototype.reloadRelation = function(model, fieldSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var relationsJson;
      var _this = this;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            return [
              4 /*yield*/,
              this.loadProductRelationJson(model, fieldSchema)
            ];
          case 1:
            relationsJson = _a.sent();
            runInAction("reloadRelation", function() {
              var dataTree = _this._dataSerializer.deserialize(relationsJson);
              if (dataTree) {
                var dataSnapshot = _this._dataNormalizer.normalize(
                  dataTree,
                  fieldSchema.RelatedContent.ContentName
                );
                _this._dataMerger.mergeTables(
                  dataSnapshot,
                  MergeStrategy.Overwrite
                );
                var collection =
                  _this._dataContext.tables[
                    fieldSchema.RelatedContent.ContentName
                  ];
                model[fieldSchema.FieldName] =
                  collection.get(String(dataTree._ClientId)) || null;
              } else {
                model[fieldSchema.FieldName] = null;
              }
              model.setChanged(fieldSchema.FieldName, false);
            });
            return [2 /*return*/];
        }
      });
    });
  };
  /** Перезагрузить с сервера поле множественной связи */
  RelationController.prototype.reloadRelations = function(model, fieldSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var relationsJson;
      var _this = this;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            return [
              4 /*yield*/,
              this.loadProductRelationJson(model, fieldSchema)
            ];
          case 1:
            relationsJson = _a.sent();
            runInAction("reloadRelations", function() {
              var dataTrees = _this._dataSerializer.deserialize(relationsJson);
              var dataSnapshot = _this._dataNormalizer.normalizeAll(
                dataTrees,
                fieldSchema.RelatedContent.ContentName
              );
              _this._dataMerger.mergeTables(
                dataSnapshot,
                MergeStrategy.Overwrite
              );
              var loadedArticleIds = dataTrees.map(function(article) {
                return article._ClientId;
              });
              var loadedArticles = _this.getLoadedArticles(
                fieldSchema.RelatedContent,
                loadedArticleIds
              );
              var relatedArticles = model[fieldSchema.FieldName];
              relatedArticles.replace(loadedArticles);
              model.setChanged(fieldSchema.FieldName, false);
            });
            return [2 /*return*/];
        }
      });
    });
  };
  RelationController.prototype.loadProductRelationJson = function(
    model,
    fieldSchema
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var response, _a;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorQuery/LoadProductRelation?" +
                  qs.stringify(this._queryParams),
                {
                  method: "POST",
                  credentials: "include",
                  headers: {
                    "Content-Type": "application/json"
                  },
                  body: JSON.stringify({
                    ProductDefinitionId: this._editorSettings
                      .ProductDefinitionId,
                    ContentPath: fieldSchema.ParentContent.ContentPath,
                    RelationFieldName: fieldSchema.FieldName,
                    ParentArticleId: model._ServerId
                  })
                }
              )
            ];
          case 1:
            response = _b.sent();
            if (!!response.ok) return [3 /*break*/, 3];
            _a = Error.bind;
            return [4 /*yield*/, response.text()];
          case 2:
            throw new (_a.apply(Error, [void 0, _b.sent()]))();
          case 3:
            return [4 /*yield*/, response.text()];
          case 4:
            return [2 /*return*/, _b.sent()];
        }
      });
    });
  };
  /**
   * Предзагрузка всех допустимых статей для поля связи.
   * Используется только при @see PreloadingMode.Lazy
   */
  RelationController.prototype.preloadRelationArticles = function(fieldSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var relationsJson;
      var _this = this;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            runInAction("preloadRelationArticles", function() {
              fieldSchema.PreloadingState = PreloadingState.Loading;
            });
            return [4 /*yield*/, this.preloadRelationArticlesJson(fieldSchema)];
          case 1:
            relationsJson = _a.sent();
            runInAction("preloadRelationArticles", function() {
              var dataTrees = _this._dataSerializer.deserialize(relationsJson);
              var dataSnapshot = _this._dataNormalizer.normalizeAll(
                dataTrees,
                fieldSchema.RelatedContent.ContentName
              );
              _this._dataMerger.mergeTables(
                dataSnapshot,
                MergeStrategy.Refresh
              );
              var loadedArticleIds = dataTrees.map(function(article) {
                return article._ClientId;
              });
              var loadedArticles = _this.getLoadedArticles(
                fieldSchema.RelatedContent,
                loadedArticleIds
              );
              fieldSchema.PreloadingState = PreloadingState.Done;
              fieldSchema.PreloadedArticles = loadedArticles;
            });
            return [2 /*return*/];
        }
      });
    });
  };
  RelationController.prototype.preloadRelationArticlesJson = function(
    fieldSchema
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var response, _a;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorQuery/PreloadRelationArticles?" +
                  qs.stringify(this._queryParams),
                {
                  method: "POST",
                  credentials: "include",
                  headers: {
                    "Content-Type": "application/json"
                  },
                  body: JSON.stringify({
                    ProductDefinitionId: this._editorSettings
                      .ProductDefinitionId,
                    ContentPath: fieldSchema.ParentContent.ContentPath,
                    RelationFieldName: fieldSchema.FieldName
                  })
                }
              )
            ];
          case 1:
            response = _b.sent();
            if (!!response.ok) return [3 /*break*/, 3];
            _a = Error.bind;
            return [4 /*yield*/, response.text()];
          case 2:
            throw new (_a.apply(Error, [void 0, _b.sent()]))();
          case 3:
            return [4 /*yield*/, response.text()];
          case 4:
            return [2 /*return*/, _b.sent()];
        }
      });
    });
  };
  /** Создание статьи по образцу, и добавление в поле-связь */
  RelationController.prototype.cloneProductPrototype = function(
    parent,
    fieldSchema
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var contentSchema, response, _a, dataTree, _b, _c;
      var _this = this;
      return tslib_1.__generator(this, function(_d) {
        switch (_d.label) {
          case 0:
            contentSchema = fieldSchema.RelatedContent;
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorCommand/ClonePartialProductPrototype?" +
                  qs.stringify(this._queryParams),
                {
                  method: "POST",
                  credentials: "include",
                  headers: {
                    "Content-Type": "application/json"
                  },
                  body: JSON.stringify({
                    ProductDefinitionId: this._editorSettings
                      .ProductDefinitionId,
                    ContentPath: fieldSchema.ParentContent.ContentPath,
                    RelationFieldName: fieldSchema.FieldName,
                    ParentArticleId: parent._ServerId
                  })
                }
              )
            ];
          case 1:
            response = _d.sent();
            if (!!response.ok) return [3 /*break*/, 3];
            _a = Error.bind;
            return [4 /*yield*/, response.text()];
          case 2:
            throw new (_a.apply(Error, [void 0, _d.sent()]))();
          case 3:
            _c = (_b = this._dataSerializer).deserialize;
            return [4 /*yield*/, response.text()];
          case 4:
            dataTree = _c.apply(_b, [_d.sent()]);
            return [
              2 /*return*/,
              runInAction("cloneProductPrototype", function() {
                var dataSnapshot = _this._dataNormalizer.normalize(
                  dataTree,
                  contentSchema.ContentName
                );
                _this._dataMerger.mergeTables(
                  dataSnapshot,
                  MergeStrategy.Refresh
                );
                var cloneId = String(dataTree._ClientId);
                var clonedEntity = _this._dataContext.tables[
                  contentSchema.ContentName
                ].get(cloneId);
                var relation = parent[fieldSchema.FieldName];
                var wasRelationChanged = parent.isChanged(
                  fieldSchema.FieldName
                );
                if (
                  isMultiRelationField(fieldSchema) &&
                  isObservableArray(relation)
                ) {
                  relation.push(clonedEntity);
                } else if (isSingleRelationField(fieldSchema) && !relation) {
                  parent[fieldSchema.FieldName] = clonedEntity;
                }
                // клонированный продукт уже сохранен на сервере,
                // поэтому считаем, что связь уже синхронизирована с бекэндом
                if (!wasRelationChanged) {
                  parent.setChanged(fieldSchema.FieldName, false);
                }
                _this._overlayPresenter.notify({
                  message:
                    "\u0421\u043E\u0437\u0434\u0430\u043D\u0430 \u043D\u043E\u0432\u0430\u044F \u0441\u0442\u0430\u0442\u044C\u044F " +
                    clonedEntity._ServerId,
                  intent: Intent.SUCCESS
                });
                return clonedEntity;
              })
            ];
        }
      });
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorSettings)],
    RelationController.prototype,
    "_editorSettings",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    RelationController.prototype,
    "_queryParams",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataSerializer)],
    RelationController.prototype,
    "_dataSerializer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataNormalizer)],
    RelationController.prototype,
    "_dataNormalizer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataMerger)],
    RelationController.prototype,
    "_dataMerger",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    RelationController.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", OverlayPresenter)],
    RelationController.prototype,
    "_overlayPresenter",
    void 0
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "selectRelation",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "selectRelations",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Array]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "loadSelectedArticles",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "reloadRelation",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "reloadRelations",
    null
  );
  tslib_1.__decorate(
    [
      modal,
      progress,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "loadProductRelationJson",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "preloadRelationArticles",
    null
  );
  tslib_1.__decorate(
    [
      modal,
      progress,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "preloadRelationArticlesJson",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    RelationController.prototype,
    "cloneProductPrototype",
    null
  );
  return RelationController;
})();
export { RelationController };
var CANCEL = Symbol();
//# sourceMappingURL=RelationController.js.map
