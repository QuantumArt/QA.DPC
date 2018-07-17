import { inject } from "react-ioc";
import { runInAction } from "mobx";
import { ArticleSnapshot, ArticleObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EditorSettings } from "Models/EditorSettings";
import { RelationSelection } from "Models/RelationSelection";
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
      const nestedObjectTree = this._dataSerializer.deserialize<ArticleSnapshot>(
        await response.text()
      );

      await initSchemaTask;
      const contentName = this._schemaContext.contentSchema.ContentName;

      const flatObjectsByContent = this._dataNormalizer.normalize(nestedObjectTree, contentName);

      this._dataContext.initStore(flatObjectsByContent);
      return this._dataContext.store[contentName].get(String(nestedObjectTree._ClientId));
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

  public async savePartialProduct(
    _article: ArticleObject,
    _contentSchema: ContentSchema,
    _relationSelection?: RelationSelection
  ) {}
}
