import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EntitySnapshot, EntityObject } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { command } from "Utils/Command";
import { newUid } from "Utils/Uid";

export class ArticleController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";
  private _hostUid = qs.parse(document.location.search).hostUID as string;
  private _callbackUid = Math.random()
    .toString(36)
    .slice(2);

  private _observer = new QP8.BackendEventObserver(this._callbackUid, (eventType, args) => {
    console.log({ eventType, args });
  });

  public dispose() {
    this._observer.dispose();
  }

  public async refreshEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadArticle(model, contentSchema, MergeStrategy.ServerWins);
  }

  public async reloadEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadArticle(model, contentSchema, MergeStrategy.Overwrite);
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

  public editArticle(model: EntityObject, contentSchema: ContentSchema, isWindow = true) {
    const callbackUid = newUid();

    const articleOptions: QP8.ArticleFormState = {
      // убираем кнопку Save в модальном окне
      disabledActionCodes: isWindow ? ["update_article"] : []
    };

    const executeOptions: QP8.ExecuteActionOptions = {
      isWindow,
      actionCode: "edit_article",
      entityTypeCode: "article",
      parentEntityId: contentSchema.ContentId,
      entityId: model._ServerId > 0 ? model._ServerId : 0,
      callerCallback: callbackUid,
      changeCurrentTab: false,
      options: articleOptions
    };

    QP8.executeBackendAction(executeOptions, this._hostUid, window.parent);

    const observer = new QP8.BackendEventObserver(callbackUid, async (eventType, args) => {
      if (eventType === QP8.BackendEventTypes.HostUnbinded) {
        observer.dispose();
      } else if (eventType === QP8.BackendEventTypes.ActionExecuted) {
        if (args.actionCode === "update_article") {
          await this.loadArticle(model, contentSchema, MergeStrategy.ServerWins);
        } else if (args.actionCode === "update_article_and_up") {
          observer.dispose();
          await this.loadArticle(model, contentSchema, MergeStrategy.ServerWins);
        }
      }
    });
  }
}
