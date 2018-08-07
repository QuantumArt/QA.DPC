import { toJS } from "mobx";
import { inject } from "react-ioc";
import { ArticleSnapshot, ArticleObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EditorSettings } from "Models/EditorSettings";
import { DataSerializer, IdMapping } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { command } from "Utils/Command";

export class EditorController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;
  @inject private _dataContext: DataContext;
  @inject private _schemaContext: SchemaContext;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";

  @command
  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._editorSettings.ArticleId > 0) {
      const response = await fetch(
        `${this._rootUrl}/ProductEditor/GetEditorData${this._query}&productDefinitionId=${
          this._editorSettings.ProductDefinitionId
        }&articleId=${this._editorSettings.ArticleId}`
      );
      if (!response.ok) {
        throw new Error(await response.text());
      }
      const nestedObjectTree = this._dataSerializer.deserialize<ArticleSnapshot>(
        await response.text()
      );

      await initSchemaTask;
      const contentName = this._schemaContext.rootSchema.ContentName;

      const flatObjectsByContent = this._dataNormalizer.normalize(nestedObjectTree, contentName);

      this._dataContext.initStore(flatObjectsByContent);
      return this._dataContext.store[contentName].get(String(nestedObjectTree._ClientId));
    } else {
      await initSchemaTask;
      const contentName = this._schemaContext.rootSchema.ContentName;

      this._dataContext.initStore({});
      return this._dataContext.createArticle(contentName);
    }
  }

  private async initSchema() {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/GetEditorSchema${this._query}&productDefinitionId=${
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
  }

  @command
  public async savePartialProduct(article: ArticleObject, contentSchema: ContentSchema) {
    const partialProduct = toJS(article);

    const response = await fetch(
      `${this._rootUrl}/ProductEditor/SavePartialProduct${this._query}`,
      {
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
      }
    );
    if (response.status === 409) {
      const dataTree = this._dataSerializer.deserialize<ArticleSnapshot>(await response.text());
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);
      const { hasMergeConfilicts } = this._dataMerger.mergeArticles(dataSnapshot);
      if (hasMergeConfilicts) {
        // TODO: React confirm dialog
        if (window.confirm(`Данные на сервере были изменены.\nПрименить изменения с сервера?`)) {
          this._dataMerger.overwriteArticles(dataSnapshot);
        }
      }
      return;
    } else if (!response.ok) {
      // TODO: React alert dialog
      alert("При сохранении произошла ошибка!");
      throw new Error(await response.text());
    }

    const okResponse: {
      IdMappings: IdMapping[];
      PartialProduct: Object;
    } = await response.json();

    this._dataSerializer.extendIdMappings(okResponse.IdMappings);

    const dataTreeJson = JSON.stringify(okResponse.PartialProduct);
    const dataTree = this._dataSerializer.deserialize<ArticleSnapshot>(dataTreeJson);
    const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);
    this._dataMerger.overwriteArticles(dataSnapshot);
  }
}
