import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { untracked, runInAction, IObservableArray } from "mobx";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import {
  ContentSchema,
  SingleRelationFieldSchema,
  MultiRelationFieldSchema,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { ArticleObject, EntitySnapshot, EntityObject } from "Models/EditorDataModels";
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

  public async selectRelation(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const existingArticleIds = untracked(() => {
      const existingArticle: EntityObject = model[fieldSchema.FieldName];
      return existingArticle && existingArticle._ServerId > 0 ? [existingArticle._ServerId] : [];
    });
    const selectedArticles = await this.selectArticles(existingArticleIds, fieldSchema, false);
    if (selectedArticles !== "CANCEL") {
      runInAction("selectRelation", () => {
        model[fieldSchema.FieldName] = selectedArticles[0] || null;
        model.setTouched(fieldSchema.FieldName, true);
      });
    }
  }

  public async selectRelations(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const existingArticleIds = untracked(() => {
      const existingArticles: EntityObject[] = model[fieldSchema.FieldName];
      return existingArticles.map(article => article._ServerId).filter(id => id > 0);
    });
    const selectedArticles = await this.selectArticles(existingArticleIds, fieldSchema, true);
    if (selectedArticles !== "CANCEL") {
      runInAction("selectRelations", () => {
        const relatedArticles: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
        const newlyCreatedArticles = relatedArticles.filter(article => article._ServerId === null);

        relatedArticles.clear();
        relatedArticles.push(...selectedArticles);
        relatedArticles.push(...newlyCreatedArticles);
        model.setTouched(fieldSchema.FieldName, true);
      });
    }
  }

  private async selectArticles(
    existingArticleIds: number[],
    fieldSchema: RelationFieldSchema,
    multiple: boolean
  ) {
    const contentSchema = fieldSchema.RelatedContent;
    const options: QP8.OpenSelectWindowOptions = {
      selectActionCode: multiple ? "multiple_select_article" : "select_article",
      entityTypeCode: "article",
      parentEntityId: contentSchema.ContentId,
      selectedEntityIDs: existingArticleIds,
      isMultiple: multiple,
      callerCallback: this._callbackUid
    };

    if (fieldSchema.RelationCondition) {
      options.options = { filter: fieldSchema.RelationCondition };
    }

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
    const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(await response.text());

    const dataSnapshot = this._dataNormalizer.normalizeAll(dataTrees, contentSchema.ContentName);

    this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.KeepTimestamp);
  }

  public async reloadRelation(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const relationsJson = await this.loadProductRelationJson(model, fieldSchema);

    runInAction("reloadRelation", () => {
      const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(relationsJson);

      if (dataTree) {
        const dataSnapshot = this._dataNormalizer.normalize(
          dataTree,
          fieldSchema.RelatedContent.ContentName
        );

        this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.Overwrite);

        const collection = this._dataContext.store[fieldSchema.RelatedContent.ContentName];

        model[fieldSchema.FieldName] = collection.get(String(dataTree._ClientId)) || null;
      } else {
        model[fieldSchema.FieldName] = null;
      }
      model.setChanged(fieldSchema.FieldName, false);
    });
  }

  public async reloadRelations(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const relationsJson = await this.loadProductRelationJson(model, fieldSchema);

    runInAction("reloadRelations", () => {
      const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(relationsJson);

      const dataSnapshot = this._dataNormalizer.normalizeAll(
        dataTrees,
        fieldSchema.RelatedContent.ContentName
      );

      this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.Overwrite);

      const loadedArticleIds = dataTrees.map(article => article._ClientId);

      const loadedArticles = this.getSelectedArticles(fieldSchema.RelatedContent, loadedArticleIds);

      const relatedArticles: IObservableArray<EntityObject> = model[fieldSchema.FieldName];

      relatedArticles.clear();
      relatedArticles.push(...loadedArticles);
      model.setChanged(fieldSchema.FieldName, false);
    });
  }

  @command
  private async loadProductRelationJson(model: ArticleObject, fieldSchema: RelationFieldSchema) {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/LoadProductRelation${this._query}`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          ProductDefinitionId: this._editorSettings.ProductDefinitionId,
          ContentPath: fieldSchema.ParentContent.ContentPath,
          RelationFieldName: fieldSchema.FieldName,
          ParentArticleId: model._ServerId
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    return await response.text();
  }
}
