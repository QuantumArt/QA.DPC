import { inject } from "react-ioc";
import { ArticleSnapshot } from "Models/EditorDataModels";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { command } from "Utils/Command";

export class EditorController {
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataContext: DataContext;
  @inject private _schemaContext: SchemaContext;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";
  private _productDefinitionId: number;
  private _articleId: number | null;

  @command
  public async initialize(productDefinitionId: number, articleId: number | null) {
    this._productDefinitionId = productDefinitionId;
    this._articleId = articleId;

    const initSchemaTask = this.initSchema();

    if (this._articleId > 0) {
      const response = await fetch(
        `${this._rootUrl}/ProductEditor/GetEditorData_Test${this._query}&articleId=${
          this._articleId
        }`
      );
      if (!response.ok) {
        throw new Error(await response.text());
      }
      const dataTree = this._dataSerializer.deserialize<ArticleSnapshot>(await response.text());

      await initSchemaTask;
      const contentName = this._schemaContext.contentSchema.ContentName;

      const dataSnapshot = this._dataNormalizer.normalize(dataTree, dataTree.ContentName);

      this._dataContext.initStore(dataSnapshot);
      return this._dataContext.store[contentName].get(String(dataTree.Id));
    } else {
      await initSchemaTask;
      const contentName = this._schemaContext.contentSchema.ContentName;

      this._dataContext.initStore({});
      return this._dataContext.createArticle(contentName);
    }
  }

  private async initSchema() {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/GetEditorSchema_Test${this._query}&productDefinitionId=${
        this._productDefinitionId
      }`
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const schema = await response.json();
    this._dataNormalizer.initSchema(schema.MergedSchemas);
    this._dataContext.initSchema(schema.MergedSchemas);
    this._schemaContext.initSchema(schema.EditorSchema);
  }
}
