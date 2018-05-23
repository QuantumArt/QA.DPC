// import { linkJsonRefs } from "Services/SchemaLinker";
import dataSerializer from "Services/DataSerializer";
import dataNormalizer from "Services/DataNormalizer";
import dataContext from "Services/DataContext";

export class EditorController {
  private _path = document.location.pathname;
  private _query = document.location.search;
  private _rootUrl = this._path.slice(0, this._path.indexOf("/ProductEditor"));
  private _productDefinitionId = Number(this._path.slice(this._rootUrl.length).split("/")[2]);
  private _articleId = Number(this._path.slice(this._rootUrl.length).split("/")[4]);

  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._articleId > 0) {
      const response = await fetch(
        `${this._rootUrl}/ProductEditor/GetProduct_Test${this._query}&articleId=${this._articleId}`
      );
      if (!response.ok) {
        throw new Error(await response.text());
      }
      const dataTree = dataSerializer.deserialize(await response.text());

      await initSchemaTask;

      const dataSnapshot = dataNormalizer.normalize(dataTree, dataTree.ContentName);

      dataContext.initStore(dataSnapshot);
    } else {
      await initSchemaTask;

      dataContext.initStore({});
    }
  }

  private async initSchema() {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/GetProductSchema_Test${this._query}&productDefinitionId=${
        this._productDefinitionId
      }`
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const schema = await response.json();
    dataNormalizer.initSchema(schema.MergedSchemas);
    dataContext.initSchema(schema.MergedSchemas);
  }
}

export default new EditorController();
