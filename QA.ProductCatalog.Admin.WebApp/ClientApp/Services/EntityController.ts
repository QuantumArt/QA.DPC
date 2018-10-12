import "Scripts/pmrpc";
import QP8 from "Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { isObservableArray, runInAction } from "mobx";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
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
  @inject private _dataMerger: DataMerger;

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
  public async removeRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ) {
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
}
