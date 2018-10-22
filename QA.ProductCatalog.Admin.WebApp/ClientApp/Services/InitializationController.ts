import { inject } from "react-ioc";
import { TablesSnapshot, EntitySnapshot } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataContext } from "Services/DataContext";
import { SchemaLinker } from "Services/SchemaLinker";
import { SchemaCompiler } from "Services/SchemaCompiler";
import { PublicationTracker } from "Services/PublicationTracker";
import { trace, progress, handleError, modal } from "Utils/Decorators";
import { rootUrl } from "Utils/Common";

export class InitializationController {
  @inject private _editorSettings: EditorSettings;
  @inject private _schemaLinker: SchemaLinker;
  @inject private _schemaCompiler: SchemaCompiler;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataContext: DataContext;
  @inject private _publicationTracker: PublicationTracker;

  private _query = document.location.search;

  @trace
  @modal
  @progress
  @handleError
  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._editorSettings.ArticleId > 0) {
      const response = await fetch(
        `${rootUrl}/ProductEditor/GetEditorData${this._query}&productDefinitionId=${
          this._editorSettings.ProductDefinitionId
        }&articleId=${this._editorSettings.ArticleId}`
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
      `${rootUrl}/ProductEditor/GetEditorSchema${this._query}&productDefinitionId=${
        this._editorSettings.ProductDefinitionId
      }`
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
