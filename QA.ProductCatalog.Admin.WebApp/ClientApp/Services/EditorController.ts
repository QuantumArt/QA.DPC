// import { linkJsonRefs } from "Services/SchemaLinker";
import dataSerializer, { DataSerializer } from "Services/DataSerializer";
import dataNormalizer, { DataNormalizer } from "Services/DataNormalizer";
import dataContext, { DataContext } from "Services/DataContext";

export class EditorController {
  private _path = document.location.pathname;
  private _query = document.location.search;
  private _rootUrl = this._path.slice(0, this._path.indexOf("/ProductEditor"));
  private _productDefinitionId = Number(this._path.slice(this._rootUrl.length).split("/")[2]);
  private _articleId = Number(this._path.slice(this._rootUrl.length).split("/")[4]);

  constructor(
    private _dataSerializer: DataSerializer,
    private _dataNormalizer: DataNormalizer,
    private _dataContext: DataContext
  ) {}

  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._articleId > 0) {
      const response = await fetch(
        `${this._rootUrl}/ProductEditor/GetProduct_Test${this._query}&articleId=${this._articleId}`
      );
      if (!response.ok) {
        throw new Error(await response.text());
      }
      const dataTree = this._dataSerializer.deserialize(await response.text());

      await initSchemaTask;

      const dataSnapshot = this._dataNormalizer.normalize(dataTree, dataTree.ContentName);

      this._dataContext.initStore(dataSnapshot);
    } else {
      await initSchemaTask;

      this._dataContext.initStore({});
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
    this._dataNormalizer.initSchema(schema.MergedSchemas);
    this._dataContext.initSchema(schema.MergedSchemas);
  }
}

export default new EditorController(dataSerializer, dataNormalizer, dataContext);
