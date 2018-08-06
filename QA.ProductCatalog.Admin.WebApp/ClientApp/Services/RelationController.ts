import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { untracked, runInAction, IObservableArray } from "mobx";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import {
  ContentSchema,
  SingleRelationFieldSchema,
  MultiRelationFieldSchema
} from "Models/EditorSchemaModels";
import { ArticleObject, ExtensionObject, ArticleSnapshot } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { command } from "Utils/Command";
import { isArray } from "Utils/TypeChecks";

export class RelationController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;
  @inject private _dataContext: DataContext;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";
  private _hostUid = qs.parse(document.location.search).hostUID as string;
  private _resolvePromise: (articleIds: number[] | "CANCEL") => void;
  private _callbackUid = Math.random()
    .toString(36)
    .slice(2);

  private _observer = new QP8.BackendEventObserver(this._callbackUid, (eventType, args) => {
    if (eventType === QP8.BackendEventTypes.EntitiesSelected) {
      this._resolvePromise(isArray(args.selectedEntityIDs) ? args.selectedEntityIDs : "CANCEL");
    } else {
      this._resolvePromise("CANCEL");
    }
  });

  public dispose() {
    this._observer.dispose();
  }

  public async selectRelation(
    model: ArticleObject | ExtensionObject,
    fieldSchema: SingleRelationFieldSchema
  ) {
    const existingArticleIds = untracked(() => {
      const existingArticle: ArticleObject = model[fieldSchema.FieldName];
      return existingArticle && existingArticle._ServerId > 0 ? [existingArticle._ServerId] : [];
    });
    const selectedArticles = await this.selectArticles(
      existingArticleIds,
      fieldSchema.Content,
      false
    );
    if (selectedArticles !== "CANCEL") {
      runInAction("selectRelation", () => {
        model[fieldSchema.FieldName] = selectedArticles[0] || null;
      });
    }
  }

  public async selectRelations(
    model: ArticleObject | ExtensionObject,
    fieldSchema: MultiRelationFieldSchema
  ) {
    const existingArticleIds = untracked(() => {
      const existingArticles: ArticleObject[] = model[fieldSchema.FieldName];
      return existingArticles.map(article => article._ServerId).filter(id => id > 0);
    });
    const selectedArticles = await this.selectArticles(
      existingArticleIds,
      fieldSchema.Content,
      true
    );
    if (selectedArticles !== "CANCEL") {
      runInAction("selectRelations", () => {
        const modelArticles: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
        const newlyCreatedArticles = modelArticles.filter(article => article._ServerId === null);

        modelArticles.clear();
        modelArticles.push(...selectedArticles);
        modelArticles.push(...newlyCreatedArticles);
      });
    }
  }

  private async selectArticles(
    existingArticleIds: number[],
    contentSchema: ContentSchema,
    multiple: boolean
  ) {
    const options = {
      selectActionCode: multiple ? "multiple_select_article" : "select_article",
      entityTypeCode: "article",
      parentEntityId: contentSchema.ContentId,
      selectedEntityIDs: existingArticleIds,
      isMultiple: multiple,
      callerCallback: this._callbackUid
    };

    QP8.openSelectWindow(options, this._hostUid, window.parent);

    const selectedArticleIds = await new Promise<number[] | "CANCEL">(resolve => {
      this._resolvePromise = resolve;
    });

    if (selectedArticleIds === "CANCEL") {
      return "CANCEL";
    }

    const articleToLoadIds = selectedArticleIds.filter(id => !existingArticleIds.includes(id));

    if (articleToLoadIds.length === 0) {
      return this.getSelectedArticles(contentSchema, selectedArticleIds);
    }

    await this.loadSelectedArticles(contentSchema, articleToLoadIds);

    return this.getSelectedArticles(contentSchema, selectedArticleIds);
  }

  private getSelectedArticles(contentSchema: ContentSchema, selectedArticleIds: number[]) {
    return untracked(() => {
      const contextArticles = this._dataContext.store[contentSchema.ContentName];
      return selectedArticleIds.map(id => contextArticles.get(String(id)));
    });
  }

  @command
  private async loadSelectedArticles(contentSchema: ContentSchema, articleToLoadIds: number[]) {
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
          ArticleIds: articleToLoadIds
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTrees = this._dataSerializer.deserialize<ArticleSnapshot[]>(await response.text());

    const dataSnapshot = this._dataNormalizer.normalizeAll(dataTrees, contentSchema.ContentName);

    this._dataMerger.mergeArticles(dataSnapshot);
  }
}
