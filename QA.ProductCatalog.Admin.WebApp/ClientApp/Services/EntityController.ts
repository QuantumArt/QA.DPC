import "Scripts/pmrpc";
import QP8 from "Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { isObservableArray, runInAction } from "mobx";
import { DataSerializer, IdMapping } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataValidator } from "Services/DataValidator";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import {
  ContentSchema,
  RelationFieldSchema,
  isMultiRelationField,
  isSingleRelationField
} from "Models/EditorSchemaModels";
import { EntitySnapshot, EntityObject, ArticleObject } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";
import { command } from "Utils/Command";
import { newUid, rootUrl } from "Utils/Common";

export class EntityController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataValidator: DataValidator;
  @inject private _dataMerger: DataMerger;
  @inject private _dataContext: DataContext;

  private _query = document.location.search;
  private _hostUid = qs.parse(document.location.search).hostUID as string;

  public async refreshEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadEntity(model, contentSchema, MergeStrategy.Refresh);
  }

  public async reloadEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadEntity(model, contentSchema, MergeStrategy.Overwrite);
  }

  @command
  private async loadEntity(
    model: EntityObject,
    contentSchema: ContentSchema,
    strategy: MergeStrategy
  ) {
    const response = await fetch(`${rootUrl}/ProductEditor/LoadPartialProduct${this._query}`, {
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
    });
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(await response.text());

    const dataSnapshot = this._dataNormalizer.normalizeAll(dataTrees, contentSchema.ContentName);

    this._dataMerger.mergeTables(dataSnapshot, strategy);
  }

  public editEntity(model: EntityObject, contentSchema: ContentSchema, isWindow = true) {
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
          await this.loadEntity(model, contentSchema, MergeStrategy.ServerWins);
        } else if (args.actionCode === "update_article_and_up") {
          observer.dispose();
          await this.loadEntity(model, contentSchema, MergeStrategy.ServerWins);
        }
      }
    });
  }

  @command
  public async publishEntity(entity: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.collectErrors(entity, contentSchema, false);
    if (errors.length > 0) {
      // TODO: React alert dialog
      window.alert(this._dataValidator.getErrorMessage(errors));
      return;
    }

    const response = await fetch(
      `${rootUrl}/ProductEditor/PublishProduct${this._query}&articleId=${entity._ServerId}`,
      {
        method: "POST",
        credentials: "include"
      }
    );
    if (
      response.status === 500 &&
      response.headers.get("Content-Type").startsWith("application/json")
    ) {
      const errorData = await response.json();
      // TODO: React alert dialog
      window.alert(errorData.Message);
    } else if (!response.ok) {
      throw new Error(await response.text());
    }
  }

  @command
  public async removeRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ) {
    // TODO: React confirm dialog
    if (!window.confirm(`Вы действительно хотите удалить статью ${entity._ServerId} ?`)) {
      return;
    }

    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(`${rootUrl}/ProductEditor/RemovePartialProduct${this._query}`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        ProductDefinitionId: this._editorSettings.ProductDefinitionId,
        ContentPath: contentSchema.ContentPath,
        RemoveArticleId: entity._ServerId
      })
    });
    if (!response.ok) {
      throw new Error(await response.text());
    }

    return runInAction("removeRelatedEntity", () => {
      const relation = parent[fieldSchema.FieldName];
      const wasRelationChanged = parent.isChanged(fieldSchema.FieldName);
      if (isMultiRelationField(fieldSchema) && isObservableArray(relation)) {
        relation.remove(entity);
      } else if (isSingleRelationField(fieldSchema) && !relation) {
        parent[fieldSchema.FieldName] = null;
      }
      // продукт уже удален на сервере, поэтому считаем,
      // что связь уже синхронизирована с бекэндом
      if (!wasRelationChanged) {
        parent.setChanged(fieldSchema.FieldName, false);
      }
    });
  }

  @command
  public async cloneRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ): Promise<EntityObject> {
    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(`${rootUrl}/ProductEditor/ClonePartialProduct${this._query}`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        ProductDefinitionId: this._editorSettings.ProductDefinitionId,
        ContentPath: contentSchema.ContentPath,
        CloneArticleId: entity._ServerId
      })
    });
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());

    return runInAction("cloneRelatedEntity", () => {
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.Refresh);

      const cloneId = String(dataTree._ClientId);
      const clonedEntity = this._dataContext.tables[contentSchema.ContentName].get(cloneId);

      const relation = parent[fieldSchema.FieldName];
      const wasRelationChanged = parent.isChanged(fieldSchema.FieldName);
      if (isMultiRelationField(fieldSchema) && isObservableArray(relation)) {
        relation.push(clonedEntity);
        // клонированный продукт уже сохранен на сервере,
        // поэтому считаем, что связь уже синхронизирована с бекэндом
        if (!wasRelationChanged) {
          parent.setChanged(fieldSchema.FieldName, false);
        }
      }
      return clonedEntity;
    });
  }

  @command
  public async saveEntitySubgraph(entity: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.collectErrors(entity, contentSchema, true);
    if (errors.length > 0) {
      // TODO: React alert dialog
      window.alert(this._dataValidator.getErrorMessage(errors));
      return;
    }

    const partialProduct = this._dataSerializer.serialize(entity, contentSchema);

    const response = await fetch(`${rootUrl}/ProductEditor/SavePartialProduct${this._query}`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        ProductDefinitionId: this._editorSettings.ProductDefinitionId,
        ContentPath: contentSchema.ContentPath,
        PartialProduct: partialProduct
      })
    });
    if (response.status === 409) {
      const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      if (this._dataMerger.tablesHasConflicts(dataSnapshot)) {
        // TODO: React confirm dialog
        const serverWins = window.confirm(
          `Данные на сервере были изменены другим пользователем.\n` +
            `Применить изменения с сервера?`
        );
        if (serverWins) {
          this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);
        } else {
          this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ClientWins);
        }
        // TODO: React alert dialog
        window.alert(`Пожалуйста, проверьте корректность данных и сохраните статью снова.`);
      } else {
        this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ClientWins);
        // TODO: React alert dialog
        window.alert(
          `Данные на сервере были изменены другим пользователем.\n` +
            `Пожалуйста, проверьте корректность данных и сохраните статью снова.`
        );
      }
      return;
    }
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const okResponse: {
      IdMappings: IdMapping[];
      PartialProduct: Object;
    } = await response.json();

    this._dataSerializer.extendIdMappings(okResponse.IdMappings);

    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(okResponse.PartialProduct);
    const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);
    this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);
  }
}
