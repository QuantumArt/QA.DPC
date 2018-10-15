import { inject } from "react-ioc";
import { EntityObject, TablesSnapshot, EntitySnapshot } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EditorSettings } from "Models/EditorSettings";
import { DataSerializer, IdMapping } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataValidator } from "Services/DataValidator";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { command } from "Utils/Command";
import { rootUrl } from "Utils/Common";
import { DataSchemaLinker } from "Services/DataSchemaLinker";

export class ProductController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSchemaLinker: DataSchemaLinker;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;
  @inject private _dataValidator: DataValidator;
  @inject private _dataContext: DataContext;
  @inject private _schemaContext: SchemaContext;

  private _query = document.location.search;

  @command
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

      this._dataSchemaLinker.addPreloadedArticlesToSnapshot(
        tablesSnapshot,
        this._schemaContext.rootSchema
      );

      this._dataContext.initTables(tablesSnapshot);

      this._dataSchemaLinker.linkPreloadedArticles(contentSchema);

      return this._dataContext.tables[contentSchema.ContentName].get(
        String(nestedObjectTree._ClientId)
      );
    } else {
      const contentSchema = await initSchemaTask;

      const teblesSnapshot: TablesSnapshot = {};

      this._dataSchemaLinker.addPreloadedArticlesToSnapshot(teblesSnapshot, contentSchema);

      this._dataContext.initTables(teblesSnapshot);

      return this._dataContext.createEntity(contentSchema.ContentName);
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
    const schema = await response.json();
    this._schemaContext.initSchema(schema.EditorSchema, schema.MergedSchemas);
    this._dataContext.initSchema(this._schemaContext.contentSchemasById);
    this._dataNormalizer.initSchema(this._schemaContext.contentSchemasById);

    return this._schemaContext.rootSchema;
  }

  @command
  public async savePartialProduct(entity: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.collectErrors(entity, contentSchema, true);
    if (errors.length > 0) {
      // TODO: React alert dialog
      window.alert(this._dataValidator.getErrorMessage(errors));
      return;
    }

    const partialProduct = this._dataSerializer.serialize(entity, contentSchema);

    const response = await fetch(`${rootUrl}/ProductEditor/SavePartialProduct${this._query}`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        ProductDefinitionId: this._editorSettings.ProductDefinitionId,
        ContentPath: contentSchema.ContentPath,
        PartialProduct: partialProduct
      })
    });
    if (response.status === 409) {
      const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      if (this._dataMerger.tablesHasConflicts(dataSnapshot)) {
        // TODO: React confirm dialog
        const serverWins = window.confirm(
          `Данные на сервере были изменены другим пользователем.\n` +
            `Применить изменения с сервера?`
        );
        if (serverWins) {
          this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);
        } else {
          this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ClientWins);
        }
        // TODO: React alert dialog
        window.alert(`Пожалуйста, проверьте корректность данных и сохраните статью снова.`);
      } else {
        this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ClientWins);
        // TODO: React alert dialog
        window.alert(
          `Данные на сервере были изменены другим пользователем.\n` +
            `Пожалуйста, проверьте корректность данных и сохраните статью снова.`
        );
      }
      return;
    }
    if (!response.ok) {
      // TODO: React alert dialog
      window.alert("При сохранении произошла ошибка!");
      throw new Error(await response.text());
    }
    const okResponse: {
      IdMappings: IdMapping[];
      PartialProduct: Object;
    } = await response.json();

    this._dataSerializer.extendIdMappings(okResponse.IdMappings);

    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(okResponse.PartialProduct);
    const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);
    this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);
  }
}
