import { inject } from "react-ioc";
import { TablesSnapshot, EntitySnapshot } from "ProductEditor/Models/EditorDataModels";
import { EditorSettings, EditorQueryParams } from "ProductEditor/Models/EditorSettingsModels";
import { DataSerializer } from "ProductEditor/Services/DataSerializer";
import { DataNormalizer } from "ProductEditor/Services/DataNormalizer";
import { DataContext } from "ProductEditor/Services/DataContext";
import { SchemaLinker } from "ProductEditor/Services/SchemaLinker";
import { SchemaCompiler } from "ProductEditor/Services/SchemaCompiler";
import { PublicationTracker } from "ProductEditor/Services/PublicationTracker";
import { trace, progress, handleError, modal } from "ProductEditor/Utils/Decorators";
import { rootUrl } from "ProductEditor/Utils/Common";
import qs from "qs";

export class InitializationController {
  @inject private _editorSettings: EditorSettings;
  @inject private _queryParams: EditorQueryParams;
  @inject private _schemaLinker: SchemaLinker;
  @inject private _schemaCompiler: SchemaCompiler;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataContext: DataContext;
  @inject private _publicationTracker: PublicationTracker;

  @trace
  @modal
  @progress
  @handleError
  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._editorSettings.ArticleId > 0) {
      const response = await fetch(
        `${rootUrl}/ProductEditorQuery/GetEditorData?${qs.stringify({
          ...this._queryParams,
          productDefinitionId: this._editorSettings.ProductDefinitionId,
          articleId: this._editorSettings.ArticleId
        })}`
      );
      if (!response.ok) {
        throw new Error(await response.text());
      }
      const nestedObjectTree = this._dataSerializer.deserialize<EntitySnapshot>(
        await response.text()
      );

      const contentSchema = await initSchemaTask;

      const tablesSnapshot = this._dataNormalizer.normalize(
        nestedObjectTree,
        contentSchema.ContentName
      );

      this._schemaLinker.addPreloadedArticlesToSnapshot(tablesSnapshot, contentSchema);

      this._dataContext.initTables(tablesSnapshot);

      this._schemaLinker.linkSchemaWithPreloadedArticles(contentSchema);

      this._publicationTracker.initStatusTracking();

      const entity = this._dataContext.tables[contentSchema.ContentName].get(
        String(nestedObjectTree._ClientId)
      );

      return { entity, contentSchema };
    } else {
      const contentSchema = await initSchemaTask;

      const tablesSnapshot: TablesSnapshot = {};

      this._schemaLinker.addPreloadedArticlesToSnapshot(tablesSnapshot, contentSchema);

      this._dataContext.initTables(tablesSnapshot);

      this._schemaLinker.linkSchemaWithPreloadedArticles(contentSchema);

      this._publicationTracker.initStatusTracking();

      const entity = this._dataContext.createEntity(contentSchema.ContentName);

      return { entity, contentSchema };
    }
  }

  private async initSchema() {
    const response = await fetch(
      `${rootUrl}/ProductEditorQuery/GetEditorSchema?${qs.stringify(
        this._queryParams
      )}&productDefinitionId=${this._editorSettings.ProductDefinitionId}`
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const rawSchema = await response.json();

    const contentSchema = this._schemaLinker.linkNestedSchemas(rawSchema.EditorSchema);
    const mergedSchemas = this._schemaLinker.linkMergedSchemas(rawSchema.MergedSchemas);

    this._dataContext.initSchema(mergedSchemas);
    this._dataNormalizer.initSchema(mergedSchemas);
    this._schemaCompiler.compileSchemaFunctions(contentSchema);

    return contentSchema;
  }
}
