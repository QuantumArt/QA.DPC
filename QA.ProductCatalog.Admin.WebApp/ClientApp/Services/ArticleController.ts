import { inject } from "react-ioc";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EntitySnapshot, EntityObject } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { command } from "Utils/Command";

export class ArticleController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";

  public async refreshEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadArticle(model, contentSchema, MergeStrategy.UpdateIfNewer);
  }

  public async reloadEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadArticle(model, contentSchema, MergeStrategy.ServerWins);
  }

  @command
  private async loadArticle(
    model: EntityObject,
    contentSchema: ContentSchema,
    strategy: MergeStrategy
  ) {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/LoadPartialProduct${this._query}`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          ProductDefinitionId: this._editorSettings.ProductDefinitionId,
          ContentPath: contentSchema.ContentPath,
          ArticleIds: [model._ServerId]
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(await response.text());

    const dataSnapshot = this._dataNormalizer.normalizeAll(dataTrees, contentSchema.ContentName);

    this._dataMerger.mergeStore(dataSnapshot, strategy);
  }
}
