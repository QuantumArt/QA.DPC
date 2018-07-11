import qs from "qs";
import { inject } from "react-ioc";
import { untracked } from "mobx";
import { DataContext } from "Services/DataContext";
import { ContentSchema } from "Models/EditorSchemaModels";
import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";

export class RelationController {
  @inject private _dataContext: DataContext;

  // @ts-ignore TODO:
  private _rootUrl = document.head.getAttribute("root-url") || "";
  private _hostUid = qs.parse(document.location.search).hostUID as string;
  private _resolvePromise: (articeIds: number[]) => void;
  private _callbackUid = Math.random()
    .toString(36)
    .slice(2);

  private _observer = new QP8.BackendEventObserver(this._callbackUid, (eventType, args) => {
    if (eventType === QP8.BackendEventTypes.EntitiesSelected) {
      this._resolvePromise(args.selectedEntityIDs || []);
    } else {
      this._resolvePromise([]);
    }
  });

  public dispose() {
    this._observer.dispose();
  }

  public async selectRelations(contentSchema: ContentSchema, multiple = false) {
    const options = {
      selectActionCode: "select_article",
      entityTypeCode: "article",
      parentEntityId: contentSchema.ContentId,
      isMultiple: multiple,
      callerCallback: this._callbackUid
    };

    QP8.openSelectWindow(options, this._hostUid, window.parent);

    const selectedArticleIds = await new Promise<number[]>(resolve => {
      this._resolvePromise = resolve;
    });

    const articleIdsToLoad = untracked(() => {
      const existingArticles = this._dataContext.store[contentSchema.ContentName];
      return selectedArticleIds.filter(id => !existingArticles.has(String(id)));
    });

    if (articleIdsToLoad.length === 0) {
      return this.getSelectedArticles(contentSchema, selectedArticleIds);
    }

    await this.loadSelectedArticles(contentSchema, articleIdsToLoad);

    // return this.getSelectedArticles(contentSchema, selectedArticleIds);
  }

  private async loadSelectedArticles(contentSchema: ContentSchema, articleIdsToLoad: number[]) {
    const existingArticleIdsByContent: {
      [contentName: string]: number[];
    } = {};

    untracked(() => {
      Object.entries(this._dataContext.store).forEach(([contentName, articlesById]) => {
        existingArticleIdsByContent[contentName] = [...articlesById.keys()].map(Number);
      });
    });

    // TODO: call /ProductEditor/LoadPartialProduct
    console.log({ articleIdsToLoad, existingArticleIdsByContent });
    console.log(JSON.stringify(existingArticleIdsByContent).length);
  }

  private getSelectedArticles(contentSchema: ContentSchema, selectedArticleIds: number[]) {
    return untracked(() => {
      const existingArticles = this._dataContext.store[contentSchema.ContentName];
      return selectedArticleIds.map(id => existingArticles.get(String(id)));
    });
  }
}
