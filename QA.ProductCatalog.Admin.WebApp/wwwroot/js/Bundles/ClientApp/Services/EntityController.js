import * as tslib_1 from "tslib";
import "Scripts/pmrpc";
import QP8 from "Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import React from "react";
import { inject } from "react-ioc";
import { isObservableArray, runInAction } from "mobx";
import { Intent } from "@blueprintjs/core";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataValidator } from "Services/DataValidator";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import { ValidationSummay } from "Components/ValidationSummary/ValidationSummary";
import { OverlayPresenter } from "Services/OverlayPresenter";
import {
  isMultiRelationField,
  isSingleRelationField
} from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { EditorSettings, EditorQueryParams } from "Models/EditorSettingsModels";
import { trace, modal, progress, handleError } from "Utils/Decorators";
import { newUid, rootUrl } from "Utils/Common";
/** Действия со статьями */
var EntityController = /** @class */ (function() {
  function EntityController() {}
  /** Обновление статьи с сервера (несохраненные изменения остаются) */
  EntityController.prototype.refreshEntity = function(model, contentSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            return [
              4 /*yield*/,
              this.loadPartialProduct(
                model,
                contentSchema,
                MergeStrategy.Refresh
              )
            ];
          case 1:
            _a.sent();
            return [2 /*return*/];
        }
      });
    });
  };
  /** Перезагрузка статьи с сервера (несохраненные изменения отбрасываются) */
  EntityController.prototype.reloadEntity = function(model, contentSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            return [
              4 /*yield*/,
              this.loadPartialProduct(
                model,
                contentSchema,
                MergeStrategy.Overwrite
              )
            ];
          case 1:
            _a.sent();
            return [2 /*return*/];
        }
      });
    });
  };
  EntityController.prototype.loadPartialProduct = function(
    model,
    contentSchema,
    strategy
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
                    ArticleIds: [model._ServerId]
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
            this._dataMerger.mergeTables(dataSnapshot, strategy);
            return [2 /*return*/];
        }
      });
    });
  };
  /** Открытие модального окна QP для редактирования статьи */
  EntityController.prototype.editEntity = function(
    model,
    contentSchema,
    isWindow
  ) {
    var _this = this;
    if (isWindow === void 0) {
      isWindow = true;
    }
    var callbackUid = newUid();
    var articleOptions = {
      // убираем кнопку Save в модальном окне
      disabledActionCodes: isWindow ? ["update_article"] : []
    };
    var executeOptions = {
      isWindow: isWindow,
      actionCode: "edit_article",
      entityTypeCode: "article",
      parentEntityId: contentSchema.ContentId,
      entityId: model._ServerId > 0 ? model._ServerId : 0,
      callerCallback: callbackUid,
      changeCurrentTab: false,
      options: articleOptions
    };
    QP8.executeBackendAction(
      executeOptions,
      this._queryParams.hostUID,
      window.parent
    );
    var observer = new QP8.BackendEventObserver(callbackUid, function(
      eventType,
      args
    ) {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        return tslib_1.__generator(this, function(_a) {
          switch (_a.label) {
            case 0:
              if (!(eventType === QP8.BackendEventTypes.HostUnbinded))
                return [3 /*break*/, 1];
              observer.dispose();
              return [3 /*break*/, 5];
            case 1:
              if (!(eventType === QP8.BackendEventTypes.ActionExecuted))
                return [3 /*break*/, 5];
              if (!(args.actionCode === "update_article"))
                return [3 /*break*/, 3];
              return [
                4 /*yield*/,
                this.loadPartialProduct(
                  model,
                  contentSchema,
                  MergeStrategy.ServerWins
                )
              ];
            case 2:
              _a.sent();
              return [3 /*break*/, 5];
            case 3:
              if (!(args.actionCode === "update_article_and_up"))
                return [3 /*break*/, 5];
              observer.dispose();
              return [
                4 /*yield*/,
                this.loadPartialProduct(
                  model,
                  contentSchema,
                  MergeStrategy.ServerWins
                )
              ];
            case 4:
              _a.sent();
              _a.label = 5;
            case 5:
              return [2 /*return*/];
          }
        });
      });
    });
  };
  /** Опубликовать продграф статей начиная с заданной, согласно XML ProductDefinition  */
  EntityController.prototype.publishEntity = function(entity, contentSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var errors;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            errors = this._dataValidator.collectErrors(
              entity,
              contentSchema,
              false
            );
            if (!(errors.length > 0)) return [3 /*break*/, 2];
            return [
              4 /*yield*/,
              this._overlayPresenter.alert(
                React.createElement(ValidationSummay, { errors: errors }),
                "OK"
              )
            ];
          case 1:
            _a.sent();
            return [3 /*break*/, 4];
          case 2:
            return [4 /*yield*/, this.publishProduct(entity)];
          case 3:
            _a.sent();
            _a.label = 4;
          case 4:
            return [2 /*return*/];
        }
      });
    });
  };
  EntityController.prototype.publishProduct = function(entity) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var response, errorData, _a;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorCommand/PublishProduct?" +
                  qs.stringify(
                    tslib_1.__assign({}, this._queryParams, {
                      articleId: entity._ServerId
                    })
                  ),
                {
                  method: "POST",
                  credentials: "include"
                }
              )
            ];
          case 1:
            response = _b.sent();
            if (
              !(
                response.status === 500 &&
                response.headers
                  .get("Content-Type")
                  .startsWith("application/json")
              )
            )
              return [3 /*break*/, 4];
            return [4 /*yield*/, response.json()];
          case 2:
            errorData = _b.sent();
            return [
              4 /*yield*/,
              this._overlayPresenter.alert(
                React.createElement("pre", null, errorData.Message),
                "OK"
              )
            ];
          case 3:
            _b.sent();
            return [3 /*break*/, 7];
          case 4:
            if (!!response.ok) return [3 /*break*/, 6];
            _a = Error.bind;
            return [4 /*yield*/, response.text()];
          case 5:
            throw new (_a.apply(Error, [void 0, _b.sent()]))();
          case 6:
            this._overlayPresenter.notify({
              message:
                "\u0421\u0442\u0430\u0442\u044C\u044F " +
                entity._ServerId +
                " \u0443\u0441\u043F\u0435\u0448\u043D\u043E \u043E\u043F\u0443\u0431\u043B\u0438\u043A\u043E\u0432\u0430\u043D\u0430",
              intent: Intent.SUCCESS
            });
            _b.label = 7;
          case 7:
            return [2 /*return*/];
        }
      });
    });
  };
  /** Удаление с статьи с сервера */
  EntityController.prototype.removeRelatedEntity = function(
    parent,
    fieldSchema,
    entity
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var confirmed;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            return [
              4 /*yield*/,
              this._overlayPresenter.confirm(
                React.createElement(
                  React.Fragment,
                  null,
                  "\u0412\u044B \u0434\u0435\u0439\u0441\u0442\u0432\u0438\u0442\u0435\u043B\u044C\u043D\u043E \u0445\u043E\u0442\u0438\u0442\u0435 \u0443\u0434\u0430\u043B\u0438\u0442\u044C \u0441\u0442\u0430\u0442\u044C\u044E ",
                  entity._ServerId,
                  " ?"
                ),
                "Удалить",
                "Отмена"
              )
            ];
          case 1:
            confirmed = _a.sent();
            if (!confirmed) return [3 /*break*/, 3];
            return [
              4 /*yield*/,
              this.removePartialProduct(parent, fieldSchema, entity)
            ];
          case 2:
            _a.sent();
            _a.label = 3;
          case 3:
            return [2 /*return*/, confirmed];
        }
      });
    });
  };
  EntityController.prototype.removePartialProduct = function(
    parent,
    fieldSchema,
    entity
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var contentSchema, response, _a;
      var _this = this;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            contentSchema = fieldSchema.RelatedContent;
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorCommand/RemovePartialProduct?" +
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
                    RemoveArticleId: entity._ServerId
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
            runInAction("removeRelatedEntity", function() {
              var relation = parent[fieldSchema.FieldName];
              var wasRelationChanged = parent.isChanged(fieldSchema.FieldName);
              if (
                isMultiRelationField(fieldSchema) &&
                isObservableArray(relation)
              ) {
                relation.remove(entity);
              } else if (isSingleRelationField(fieldSchema) && !relation) {
                parent[fieldSchema.FieldName] = null;
              }
              // продукт уже удален на сервере, поэтому считаем,
              // что связь уже синхронизирована с бекэндом
              if (!wasRelationChanged) {
                parent.setChanged(fieldSchema.FieldName, false);
              }
              _this._overlayPresenter.notify({
                message:
                  "\u0421\u0442\u0430\u0442\u044C\u044F " +
                  entity._ServerId +
                  " \u0443\u0441\u043F\u0435\u0448\u043D\u043E \u0443\u0434\u0430\u043B\u0435\u043D\u0430",
                intent: Intent.WARNING
              });
            });
            return [2 /*return*/];
        }
      });
    });
  };
  /** Клонирование подграфа статей начиная с заданной, согласно XML ProductDefinition */
  EntityController.prototype.cloneRelatedEntity = function(
    parent,
    fieldSchema,
    entity
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
                  "/ProductEditorCommand/ClonePartialProduct?" +
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
                    CloneArticleId: entity._ServerId
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
              runInAction("cloneRelatedEntity", function() {
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
                  // клонированный продукт уже сохранен на сервере,
                  // поэтому считаем, что связь уже синхронизирована с бекэндом
                  if (!wasRelationChanged) {
                    parent.setChanged(fieldSchema.FieldName, false);
                  }
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
  /** Сохранение подграфа статей начиная с заданной, согласно XML ProductDefinition */
  EntityController.prototype.saveEntity = function(entity, contentSchema) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var errors;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            errors = this._dataValidator.collectErrors(
              entity,
              contentSchema,
              true
            );
            if (!(errors.length > 0)) return [3 /*break*/, 2];
            return [
              4 /*yield*/,
              this._overlayPresenter.alert(
                React.createElement(ValidationSummay, { errors: errors }),
                "OK"
              )
            ];
          case 1:
            _a.sent();
            return [3 /*break*/, 4];
          case 2:
            return [
              4 /*yield*/,
              this.savePartialProduct(entity, contentSchema)
            ];
          case 3:
            _a.sent();
            _a.label = 4;
          case 4:
            return [2 /*return*/];
        }
      });
    });
  };
  EntityController.prototype.savePartialProduct = function(
    entity,
    contentSchema
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var partialProduct,
        response,
        dataTree_1,
        _a,
        _b,
        dataSnapshot_1,
        serverWins,
        _c,
        okResponse,
        dataTree,
        dataSnapshot;
      return tslib_1.__generator(this, function(_d) {
        switch (_d.label) {
          case 0:
            partialProduct = this._dataSerializer.serialize(
              entity,
              contentSchema
            );
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorCommand/SavePartialProduct?" +
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
                    PartialProduct: partialProduct
                  })
                }
              )
            ];
          case 1:
            response = _d.sent();
            if (!(response.status === 409)) return [3 /*break*/, 8];
            _b = (_a = this._dataSerializer).deserialize;
            return [4 /*yield*/, response.text()];
          case 2:
            dataTree_1 = _b.apply(_a, [_d.sent()]);
            dataSnapshot_1 = this._dataNormalizer.normalize(
              dataTree_1,
              contentSchema.ContentName
            );
            if (!this._dataMerger.tablesHasConflicts(dataSnapshot_1))
              return [3 /*break*/, 5];
            return [
              4 /*yield*/,
              this._overlayPresenter.confirm(
                React.createElement(
                  React.Fragment,
                  null,
                  "\u0414\u0430\u043D\u043D\u044B\u0435 \u043D\u0430 \u0441\u0435\u0440\u0432\u0435\u0440\u0435 \u0431\u044B\u043B\u0438 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u044B \u0434\u0440\u0443\u0433\u0438\u043C \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u0435\u043C.",
                  React.createElement("br", null),
                  "\u041F\u0440\u0438\u043C\u0435\u043D\u0438\u0442\u044C \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F \u0441 \u0441\u0435\u0440\u0432\u0435\u0440\u0430?"
                ),
                "Применить",
                "Отмена"
              )
            ];
          case 3:
            serverWins = _d.sent();
            if (serverWins) {
              this._dataMerger.mergeTables(
                dataSnapshot_1,
                MergeStrategy.ServerWins
              );
            } else {
              this._dataMerger.mergeTables(
                dataSnapshot_1,
                MergeStrategy.ClientWins
              );
            }
            return [
              4 /*yield*/,
              this._overlayPresenter.alert(
                "\u041F\u043E\u0436\u0430\u043B\u0443\u0439\u0441\u0442\u0430, \u043F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u043E\u0441\u0442\u044C \u0434\u0430\u043D\u043D\u044B\u0445 \u0438 \u0441\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u0435 \u0441\u0442\u0430\u0442\u044C\u044E \u0441\u043D\u043E\u0432\u0430.",
                "OK"
              )
            ];
          case 4:
            _d.sent();
            return [3 /*break*/, 7];
          case 5:
            this._dataMerger.mergeTables(
              dataSnapshot_1,
              MergeStrategy.ClientWins
            );
            return [
              4 /*yield*/,
              this._overlayPresenter.alert(
                React.createElement(
                  React.Fragment,
                  null,
                  "\u0414\u0430\u043D\u043D\u044B\u0435 \u043D\u0430 \u0441\u0435\u0440\u0432\u0435\u0440\u0435 \u0431\u044B\u043B\u0438 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u044B \u0434\u0440\u0443\u0433\u0438\u043C \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u0435\u043C.",
                  React.createElement("br", null),
                  "\u041F\u043E\u0436\u0430\u043B\u0443\u0439\u0441\u0442\u0430, \u043F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u043E\u0441\u0442\u044C \u0434\u0430\u043D\u043D\u044B\u0445 \u0438 \u0441\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u0435 \u0441\u0442\u0430\u0442\u044C\u044E \u0441\u043D\u043E\u0432\u0430."
                ),
                "OK"
              )
            ];
          case 6:
            _d.sent();
            _d.label = 7;
          case 7:
            return [2 /*return*/];
          case 8:
            if (!!response.ok) return [3 /*break*/, 10];
            _c = Error.bind;
            return [4 /*yield*/, response.text()];
          case 9:
            throw new (_c.apply(Error, [void 0, _d.sent()]))();
          case 10:
            return [4 /*yield*/, response.json()];
          case 11:
            okResponse = _d.sent();
            this._dataSerializer.extendIdMappings(okResponse.IdMappings);
            dataTree = this._dataSerializer.deserialize(
              okResponse.PartialProduct
            );
            dataSnapshot = this._dataNormalizer.normalize(
              dataTree,
              contentSchema.ContentName
            );
            this._dataMerger.mergeTables(
              dataSnapshot,
              MergeStrategy.ServerWins
            );
            this._overlayPresenter.notify({
              message:
                "\u0421\u0442\u0430\u0442\u044C\u044F " +
                entity._ServerId +
                " \u0443\u0441\u043F\u0435\u0448\u043D\u043E \u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u0430",
              intent: Intent.SUCCESS
            });
            return [2 /*return*/];
        }
      });
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorSettings)],
    EntityController.prototype,
    "_editorSettings",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    EntityController.prototype,
    "_queryParams",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataSerializer)],
    EntityController.prototype,
    "_dataSerializer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataNormalizer)],
    EntityController.prototype,
    "_dataNormalizer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataValidator)],
    EntityController.prototype,
    "_dataValidator",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataMerger)],
    EntityController.prototype,
    "_dataMerger",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    EntityController.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", OverlayPresenter)],
    EntityController.prototype,
    "_overlayPresenter",
    void 0
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "refreshEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "reloadEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object, Number]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "loadPartialProduct",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object, Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    EntityController.prototype,
    "editEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "publishEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "publishProduct",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "removeRelatedEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "removePartialProduct",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "cloneRelatedEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "saveEntity",
    null
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    EntityController.prototype,
    "savePartialProduct",
    null
  );
  return EntityController;
})();
export { EntityController };
//# sourceMappingURL=EntityController.js.map
