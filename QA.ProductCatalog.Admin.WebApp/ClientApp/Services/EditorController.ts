import { inject } from "react-ioc";
import { runInAction } from "mobx";
import { ArticleSnapshot } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { command } from "Utils/Command";

export class EditorController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataContext: DataContext;
  @inject private _schemaContext: SchemaContext;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";

  @command
  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._editorSettings.ArticleId > 0) {
      const response = await fetch(
        `${this._rootUrl}/ProductEditor/GetEditorData_Test${this._query}&articleId=${
          this._editorSettings.ArticleId
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
      return runInAction(() => this._dataContext.createArticle(contentName));
    }
  }

  private async initSchema() {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/GetEditorSchema_Test${this._query}&productDefinitionId=${
        this._editorSettings.ProductDefinitionId
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
