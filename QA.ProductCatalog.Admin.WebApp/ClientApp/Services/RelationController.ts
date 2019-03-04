import "wwwroot/js/pmrpc";
import QP8 from "wwwroot/js/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { untracked, runInAction, IObservableArray, isObservableArray } from "mobx";
import { Intent } from "@blueprintjs/core";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import { OverlayPresenter } from "Services/OverlayPresenter";
import {
  ContentSchema,
  SingleRelationFieldSchema,
  MultiRelationFieldSchema,
  RelationFieldSchema,
  PreloadingState,
  isMultiRelationField,
  isSingleRelationField
} from "Models/EditorSchemaModels";
import { ArticleObject, EntitySnapshot, EntityObject } from "Models/EditorDataModels";
import { EditorSettings, EditorQueryParams } from "Models/EditorSettingsModels";
import { trace, modal, progress, handleError } from "Utils/Decorators";
import { isArray } from "Utils/TypeChecks";
import { newUid, rootUrl } from "Utils/Common";

/** Действия со связями */
export class RelationController {
  @inject private _editorSettings: EditorSettings;
  @inject private _queryParams: EditorQueryParams;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;
  @inject private _dataContext: DataContext;
  @inject private _overlayPresenter: OverlayPresenter;

  private _resolvePromise: (articleIds: number[] | typeof CANCEL) => void;
  private _callbackUid = newUid();

  private _observer = new QP8.BackendEventObserver(this._callbackUid, (eventType, args) => {
    if (eventType === QP8.BackendEventTypes.EntitiesSelected && isArray(args.selectedEntityIDs)) {
      this._resolvePromise(args.selectedEntityIDs);
    } else {
      this._resolvePromise(CANCEL);
    }
  });

  public dispose() {
    this._observer.dispose();
  }

  /** Открыть окно QP для выбора единичной связи */
  @trace
  @handleError
  public async selectRelation(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const existingArticleIds = untracked(() => {
      const existingArticle: EntityObject = model[fieldSchema.FieldName];
      return existingArticle && existingArticle._ServerId > 0 ? [existingArticle._ServerId] : [];
    });
    const selectedArticles = await this.selectArticles(existingArticleIds, fieldSchema, false);
    if (selectedArticles !== CANCEL) {
      runInAction("selectRelation", () => {
        model[fieldSchema.FieldName] = selectedArticles[0] || null;
        model.setTouched(fieldSchema.FieldName, true);
      });
    }
  }

  /** Открыть окно QP для выбора множественной связи */
  @trace
  @handleError
  public async selectRelations(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const existingArticleIds = untracked(() => {
      const existingArticles: EntityObject[] = model[fieldSchema.FieldName];
      return existingArticles.map(article => article._ServerId).filter(id => id > 0);
    });
    const selectedArticles = await this.selectArticles(existingArticleIds, fieldSchema, true);
    if (selectedArticles !== CANCEL) {
      runInAction("selectRelations", () => {
        const relatedArticles: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
        const newlyCreatedArticles = relatedArticles.filter(article => article._ServerId === null);

        relatedArticles.replace([...selectedArticles, ...newlyCreatedArticles]);
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

    QP8.openSelectWindow(options, this._queryParams.hostUID, window.parent);

    const selectedArticleIds = await new Promise<number[] | typeof CANCEL>(resolve => {
      this._resolvePromise = resolve;
    });

    if (selectedArticleIds === CANCEL) {
      return CANCEL;
    }

    const articleToLoadIds = selectedArticleIds.filter(id => !existingArticleIds.includes(id));

    if (articleToLoadIds.length === 0) {
      return this.getLoadedArticles(contentSchema, selectedArticleIds);
    }

    await this.loadSelectedArticles(contentSchema, articleToLoadIds);

    return this.getLoadedArticles(contentSchema, selectedArticleIds);
  }

  private getLoadedArticles(contentSchema: ContentSchema, articleIds: number[]) {
    return untracked(() => {
      const contextArticles = this._dataContext.tables[contentSchema.ContentName];
      return articleIds.map(id => contextArticles.get(String(id)));
    });
  }

  @trace
  @modal
  @progress
  @handleError
  private async loadSelectedArticles(contentSchema: ContentSchema, articleToLoadIds: number[]) {
    const response = await fetch(
      `${rootUrl}/ProductEditorQuery/LoadPartialProduct?${qs.stringify(this._queryParams)}`,
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

    this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Refresh);
  }

  /** Перезагрузить с сервера поле единичной связи */
  @trace
  @handleError
  public async reloadRelation(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const relationsJson = await this.loadProductRelationJson(model, fieldSchema);

    runInAction("reloadRelation", () => {
      const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(relationsJson);

      if (dataTree) {
        const dataSnapshot = this._dataNormalizer.normalize(
          dataTree,
          fieldSchema.RelatedContent.ContentName
        );

        this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Overwrite);

        const collection = this._dataContext.tables[fieldSchema.RelatedContent.ContentName];

        model[fieldSchema.FieldName] = collection.get(String(dataTree._ClientId)) || null;
      } else {
        model[fieldSchema.FieldName] = null;
      }
      model.setChanged(fieldSchema.FieldName, false);
    });
  }

  /** Перезагрузить с сервера поле множественной связи */
  @trace
  @handleError
  public async reloadRelations(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const relationsJson = await this.loadProductRelationJson(model, fieldSchema);

    runInAction("reloadRelations", () => {
      const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(relationsJson);

      const dataSnapshot = this._dataNormalizer.normalizeAll(
        dataTrees,
        fieldSchema.RelatedContent.ContentName
      );

      this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Overwrite);

      const loadedArticleIds = dataTrees.map(article => article._ClientId);

      const loadedArticles = this.getLoadedArticles(fieldSchema.RelatedContent, loadedArticleIds);

      const relatedArticles: IObservableArray<EntityObject> = model[fieldSchema.FieldName];

      relatedArticles.replace(loadedArticles);
      model.setChanged(fieldSchema.FieldName, false);
    });
  }

  @modal
  @progress
  private async loadProductRelationJson(model: ArticleObject, fieldSchema: RelationFieldSchema) {
    const response = await fetch(
      `${rootUrl}/ProductEditorQuery/LoadProductRelation?${qs.stringify(this._queryParams)}`,
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

  /**
   * Предзагрузка всех допустимых статей для поля связи.
   * Используется только при @see PreloadingMode.Lazy
   */
  @trace
  @handleError
  public async preloadRelationArticles(fieldSchema: RelationFieldSchema) {
    runInAction("preloadRelationArticles", () => {
      fieldSchema.PreloadingState = PreloadingState.Loading;
    });

    const relationsJson = await this.preloadRelationArticlesJson(fieldSchema);

    runInAction("preloadRelationArticles", () => {
      const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(relationsJson);

      const dataSnapshot = this._dataNormalizer.normalizeAll(
        dataTrees,
        fieldSchema.RelatedContent.ContentName
      );

      this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Refresh);

      const loadedArticleIds = dataTrees.map(article => article._ClientId);

      const loadedArticles = this.getLoadedArticles(fieldSchema.RelatedContent, loadedArticleIds);

      fieldSchema.PreloadingState = PreloadingState.Done;
      fieldSchema.PreloadedArticles = loadedArticles;
    });
  }

  @modal
  @progress
  private async preloadRelationArticlesJson(fieldSchema: RelationFieldSchema) {
    const response = await fetch(
      `${rootUrl}/ProductEditorQuery/PreloadRelationArticles?${qs.stringify(this._queryParams)}`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          ProductDefinitionId: this._editorSettings.ProductDefinitionId,
          ContentPath: fieldSchema.ParentContent.ContentPath,
          RelationFieldName: fieldSchema.FieldName
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    return await response.text();
  }

  /** Создание статьи по образцу, и добавление в поле-связь */
  @trace
  @modal
  @progress
  @handleError
  public async cloneProductPrototype(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema
  ): Promise<EntityObject> {
    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(
      `${rootUrl}/ProductEditorCommand/ClonePartialProductPrototype?${qs.stringify(
        this._queryParams
      )}`,
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
          ParentArticleId: parent._ServerId
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());

    return runInAction("cloneProductPrototype", () => {
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Refresh);

      const cloneId = String(dataTree._ClientId);
      const clonedEntity = this._dataContext.tables[contentSchema.ContentName].get(cloneId);

      const relation = parent[fieldSchema.FieldName];
      const wasRelationChanged = parent.isChanged(fieldSchema.FieldName);
      if (isMultiRelationField(fieldSchema) && isObservableArray(relation)) {
        relation.push(clonedEntity);
      } else if (isSingleRelationField(fieldSchema) && !relation) {
        parent[fieldSchema.FieldName] = clonedEntity;
      }
      // клонированный продукт уже сохранен на сервере,
      // поэтому считаем, что связь уже синхронизирована с бекэндом
      if (!wasRelationChanged) {
        parent.setChanged(fieldSchema.FieldName, false);
      }
      this._overlayPresenter.notify({
        message: `Создана новая статья ${clonedEntity._ServerId}`,
        intent: Intent.SUCCESS
      });
      return clonedEntity;
    });
  }
}

const CANCEL = Symbol();
