import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { untracked, runInAction } from "mobx";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataContext } from "Services/DataContext";
import {
  ContentSchema,
  SingleRelationFieldSchema,
  MultiRelationFieldSchema
} from "Models/EditorSchemaModels";
import {
  ArticleObject,
  ExtensionObject,
  ArticleSnapshot,
  MutableStoreSnapshot
} from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { command } from "Utils/Command";
import { ObservableArray } from "../../node_modules/mobx/lib/types/observablearray";

export class RelationController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataContext: DataContext;

  private _query = document.location.search;
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

  public async selectRelation(
    model: ArticleObject | ExtensionObject,
    fieldSchema: SingleRelationFieldSchema
  ) {
    const [selectedArticle] = await this.selectArticles(fieldSchema.Content, false);
    if (selectedArticle) {
      runInAction("selectRelation", () => {
        model[fieldSchema.FieldName] = selectedArticle;
      });
    }
  }

  public async selectRelations(
    model: ArticleObject | ExtensionObject,
    fieldSchema: MultiRelationFieldSchema
  ) {
    const selectedArticles = await this.selectArticles(fieldSchema.Content, true);
    if (selectedArticles.length > 0) {
      runInAction("selectRelations", () => {
        const modelArticles = model[fieldSchema.FieldName] as ObservableArray<ArticleObject>;
        const existingArticles = new Set(modelArticles.peek());
        const articlesToAdd = selectedArticles.filter(article => !existingArticles.has(article));
        modelArticles.push(...articlesToAdd);
      });
    }
  }

  private async selectArticles(contentSchema: ContentSchema, multiple) {
    const options = {
      selectActionCode: multiple ? "multiple_select_article" : "select_article",
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

    return this.getSelectedArticles(contentSchema, selectedArticleIds);
  }

  private getSelectedArticles(contentSchema: ContentSchema, selectedArticleIds: number[]) {
    return untracked(() => {
      const existingArticles = this._dataContext.store[contentSchema.ContentName];
      return selectedArticleIds.map(id => existingArticles.get(String(id)));
    });
  }

  @command
  private async loadSelectedArticles(contentSchema: ContentSchema, articleIdsToLoad: number[]) {
    const existingArticleIds = this.getExistingArticleIdsByContent();

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
          ArticleIds: articleIdsToLoad,
          IgnoredArticleIdsByContent: existingArticleIds
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTrees = this._dataSerializer.deserialize<ArticleSnapshot[]>(await response.text());

    const dataSnapshot = this._dataNormalizer.normalizeAll(dataTrees, contentSchema.ContentName);

    this.removeExistingArticlesFromSnapshot(dataSnapshot, existingArticleIds);

    this._dataContext.mergeArticles(dataSnapshot);
  }

  private getExistingArticleIdsByContent() {
    const existingArticleIds: ArticleIdsByContent = {};

    untracked(() => {
      Object.entries(this._dataContext.store).forEach(([contentName, articlesById]) => {
        existingArticleIds[contentName] = [...articlesById.keys()].map(Number);
      });
    });

    return existingArticleIds;
  }

  private removeExistingArticlesFromSnapshot(
    dataSnapshot: MutableStoreSnapshot,
    existingArticleIds: ArticleIdsByContent
  ) {
    Object.entries(existingArticleIds).forEach(([contentName, articleIds]) => {
      const articleSnapshotsById = dataSnapshot[contentName];
      if (articleSnapshotsById) {
        articleIds.forEach(id => {
          delete articleSnapshotsById[id];
        });
      }
    });
  }
}

interface ArticleIdsByContent {
  [contentName: string]: number[];
}
