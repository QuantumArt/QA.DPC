import * as tslib_1 from "tslib";
import { inject } from "react-ioc";
import { EditorSettings, EditorQueryParams } from "Models/EditorSettingsModels";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataContext } from "Services/DataContext";
import { SchemaLinker } from "Services/SchemaLinker";
import { SchemaCompiler } from "Services/SchemaCompiler";
import { PublicationTracker } from "Services/PublicationTracker";
import { trace, progress, handleError, modal } from "Utils/Decorators";
import { rootUrl } from "Utils/Common";
import qs from "qs";
var InitializationController = /** @class */ (function() {
  function InitializationController() {}
  InitializationController.prototype.initialize = function() {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var initSchemaTask,
        response,
        _a,
        nestedObjectTree,
        _b,
        _c,
        contentSchema,
        tablesSnapshot,
        entity,
        contentSchema,
        tablesSnapshot,
        entity;
      return tslib_1.__generator(this, function(_d) {
        switch (_d.label) {
          case 0:
            initSchemaTask = this.initSchema();
            if (!(this._editorSettings.ArticleId > 0)) return [3 /*break*/, 6];
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorQuery/GetEditorData?" +
                  qs.stringify(
                    tslib_1.__assign({}, this._queryParams, {
                      productDefinitionId: this._editorSettings
                        .ProductDefinitionId,
                      articleId: this._editorSettings.ArticleId
                    })
                  )
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
            nestedObjectTree = _c.apply(_b, [_d.sent()]);
            return [4 /*yield*/, initSchemaTask];
          case 5:
            contentSchema = _d.sent();
            tablesSnapshot = this._dataNormalizer.normalize(
              nestedObjectTree,
              contentSchema.ContentName
            );
            this._schemaLinker.addPreloadedArticlesToSnapshot(
              tablesSnapshot,
              contentSchema
            );
            this._dataContext.initTables(tablesSnapshot);
            this._schemaLinker.linkSchemaWithPreloadedArticles(contentSchema);
            this._publicationTracker.initStatusTracking();
            entity = this._dataContext.tables[contentSchema.ContentName].get(
              String(nestedObjectTree._ClientId)
            );
            return [
              2 /*return*/,
              { entity: entity, contentSchema: contentSchema }
            ];
          case 6:
            return [4 /*yield*/, initSchemaTask];
          case 7:
            contentSchema = _d.sent();
            tablesSnapshot = {};
            this._schemaLinker.addPreloadedArticlesToSnapshot(
              tablesSnapshot,
              contentSchema
            );
            this._dataContext.initTables(tablesSnapshot);
            this._schemaLinker.linkSchemaWithPreloadedArticles(contentSchema);
            this._publicationTracker.initStatusTracking();
            entity = this._dataContext.createEntity(contentSchema.ContentName);
            return [
              2 /*return*/,
              { entity: entity, contentSchema: contentSchema }
            ];
        }
      });
    });
  };
  InitializationController.prototype.initSchema = function() {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var response, _a, rawSchema, contentSchema, mergedSchemas;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorQuery/GetEditorSchema?" +
                  qs.stringify(this._queryParams) +
                  "&productDefinitionId=" +
                  this._editorSettings.ProductDefinitionId
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
            return [4 /*yield*/, response.json()];
          case 4:
            rawSchema = _b.sent();
            contentSchema = this._schemaLinker.linkNestedSchemas(
              rawSchema.EditorSchema
            );
            mergedSchemas = this._schemaLinker.linkMergedSchemas(
              rawSchema.MergedSchemas
            );
            this._dataContext.initSchema(mergedSchemas);
            this._dataNormalizer.initSchema(mergedSchemas);
            this._schemaCompiler.compileSchemaFunctions(contentSchema);
            return [2 /*return*/, contentSchema];
        }
      });
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorSettings)],
    InitializationController.prototype,
    "_editorSettings",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    InitializationController.prototype,
    "_queryParams",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", SchemaLinker)],
    InitializationController.prototype,
    "_schemaLinker",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", SchemaCompiler)],
    InitializationController.prototype,
    "_schemaCompiler",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataSerializer)],
    InitializationController.prototype,
    "_dataSerializer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataNormalizer)],
    InitializationController.prototype,
    "_dataNormalizer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    InitializationController.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationTracker)],
    InitializationController.prototype,
    "_publicationTracker",
    void 0
  );
  tslib_1.__decorate(
    [
      trace,
      modal,
      progress,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", []),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    InitializationController.prototype,
    "initialize",
    null
  );
  return InitializationController;
})();
export { InitializationController };
//# sourceMappingURL=InitializationController.js.map
