import "../../../wwwroot/js/pmrpc";
import QP8 from "../../../wwwroot/js/qp/QP8BackendApi.Interaction";
import qs from "qs";
import React from "react";
import { inject } from "react-ioc";
import { isObservableArray, runInAction } from "mobx";
import { Intent } from "@blueprintjs/core";
import { DataSerializer, IdMapping } from "ProductEditor/Services/DataSerializer";
import { DataNormalizer } from "ProductEditor/Services/DataNormalizer";
import { DataValidator } from "ProductEditor/Services/DataValidator";
import { DataMerger, MergeStrategy } from "ProductEditor/Services/DataMerger";
import { DataContext } from "ProductEditor/Services/DataContext";
import { ValidationSummay } from "ProductEditor/Components/ValidationSummary/ValidationSummary";
import { OverlayPresenter } from "ProductEditor/Services/OverlayPresenter";
import {
  ContentSchema,
  RelationFieldSchema,
  isMultiRelationField,
  isSingleRelationField
} from "ProductEditor/Models/EditorSchemaModels";
import { EntitySnapshot, EntityObject, ArticleObject } from "ProductEditor/Models/EditorDataModels";
import { EditorSettings, EditorQueryParams } from "ProductEditor/Models/EditorSettingsModels";
import { trace, modal, progress, handleError } from "ProductEditor/Utils/Decorators";
import { newUid, rootUrl } from "ProductEditor/Utils/Common";

/** Действия со статьями */
export class EntityController {
  @inject private _editorSettings: EditorSettings;
  @inject private _queryParams: EditorQueryParams;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataValidator: DataValidator;
  @inject private _dataMerger: DataMerger;
  @inject private _dataContext: DataContext;
  @inject private _overlayPresenter: OverlayPresenter;

  /** Обновление статьи с сервера (несохраненные изменения остаются) */
  @trace
  @handleError
  public async refreshEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadPartialProduct(model, contentSchema, MergeStrategy.Refresh);
  }

  /** Перезагрузка статьи с сервера (несохраненные изменения отбрасываются) */
  @trace
  @handleError
  public async reloadEntity(model: EntityObject, contentSchema: ContentSchema) {
    await this.loadPartialProduct(model, contentSchema, MergeStrategy.Overwrite);
  }

  @trace
  @modal
  @progress
  @handleError
  protected async loadPartialProduct(
    model: EntityObject,
    contentSchema: ContentSchema,
    strategy: MergeStrategy
  ) {
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
          ArticleIds: [model._ServerId]
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTrees = this._dataSerializer.deserialize<EntitySnapshot[]>(await response.text());

    const dataSnapshot = this._dataNormalizer.normalizeAll(dataTrees, contentSchema.ContentName);

    this._dataMerger.mergeTables(dataSnapshot, strategy);
  }

  /** Открытие модального окна QP для редактирования статьи */
  @trace
  @handleError
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

    QP8.executeBackendAction(executeOptions, this._queryParams.hostUID, window.parent);

    const observer = new QP8.BackendEventObserver(callbackUid, async (eventType, args) => {
      if (eventType === QP8.BackendEventTypes.HostUnbinded) {
        observer.dispose();
      } else if (eventType === QP8.BackendEventTypes.ActionExecuted) {
        if (args.actionCode === "update_article") {
          await this.loadPartialProduct(model, contentSchema, MergeStrategy.ServerWins);
        } else if (args.actionCode === "update_article_and_up") {
          observer.dispose();
          await this.loadPartialProduct(model, contentSchema, MergeStrategy.ServerWins);
        }
      }
    });
  }

  /** Опубликовать продграф статей начиная с заданной, согласно XML ProductDefinition  */
  @trace
  @handleError
  public async publishEntity(entity: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.collectErrors(entity, contentSchema, false);
    if (errors.length > 0) {
      await this._overlayPresenter.alert(<ValidationSummay errors={errors} />, "OK");
    } else {
      await this.publishProduct(entity);
    }
  }

  @trace
  @modal
  @progress
  @handleError
  private async publishProduct(entity: EntityObject) {
    const response = await fetch(
      `${rootUrl}/ProductEditorCommand/PublishProduct?${qs.stringify({
        ...this._queryParams,
        articleId: entity._ServerId
      })}`,
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
      await this._overlayPresenter.alert(<pre>{errorData.Message}</pre>, "OK");
    } else if (!response.ok) {
      throw new Error(await response.text());
    } else {
      this._overlayPresenter.notify({
        message: `Статья ${entity._ServerId} успешно опубликована`,
        intent: Intent.SUCCESS
      });
    }
  }

  /** Удаление с статьи с сервера */
  @trace
  @handleError
  public async removeRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ) {
    const confirmed = await this._overlayPresenter.confirm(
      <>Вы действительно хотите удалить статью {entity._ServerId} ?</>,
      "Удалить",
      "Отмена"
    );
    if (confirmed) {
      await this.removePartialProduct(parent, fieldSchema, entity);
    }
    return confirmed;
  }

  @trace
  @modal
  @progress
  @handleError
  private async removePartialProduct(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ) {
    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(
      `${rootUrl}/ProductEditorCommand/RemovePartialProduct?${qs.stringify(this._queryParams)}`,
      {
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
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }

    runInAction("removeRelatedEntity", () => {
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
      this._overlayPresenter.notify({
        message: `Статья ${entity._ServerId} успешно удалена`,
        intent: Intent.WARNING
      });
    });
  }

  /** Клонирование подграфа статей начиная с заданной, согласно XML ProductDefinition */
  @trace
  @modal
  @progress
  @handleError
  public async cloneRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ): Promise<EntityObject> {
    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(
      `${rootUrl}/ProductEditorCommand/ClonePartialProduct?${qs.stringify(this._queryParams)}`,
      {
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
      }
    );
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
      this._overlayPresenter.notify({
        message: `Создана новая статья ${clonedEntity._ServerId}`,
        intent: Intent.SUCCESS
      });
      return clonedEntity;
    });
  }

  /** Сохранение подграфа статей начиная с заданной, согласно XML ProductDefinition */
  @trace
  @handleError
  public async saveEntity(entity: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.collectErrors(entity, contentSchema, true);
    if (errors.length > 0) {
      await this._overlayPresenter.alert(<ValidationSummay errors={errors} />, "OK");
    } else {
      await this.savePartialProduct(entity, contentSchema);
    }
  }

  @trace
  @modal
  @progress
  @handleError
  private async savePartialProduct(entity: EntityObject, contentSchema: ContentSchema) {
    const partialProduct = this._dataSerializer.serialize(entity, contentSchema);

    const response = await fetch(
      `${rootUrl}/ProductEditorCommand/SavePartialProduct?${qs.stringify(this._queryParams)}`,
      {
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
      }
    );
    if (response.status === 409) {
      const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      if (this._dataMerger.tablesHasConflicts(dataSnapshot)) {
        const serverWins = await this._overlayPresenter.confirm(
          <>
            Данные на сервере были изменены другим пользователем.
            <br />
            Применить изменения с сервера?
          </>,
          "Применить",
          "Отмена"
        );
        if (serverWins) {
          this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);
        } else {
          this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ClientWins);
        }
        await this._overlayPresenter.alert(
          `Пожалуйста, проверьте корректность данных и сохраните статью снова.`,
          "OK"
        );
      } else {
        this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ClientWins);
        await this._overlayPresenter.alert(
          <>
            Данные на сервере были изменены другим пользователем.
            <br />
            Пожалуйста, проверьте корректность данных и сохраните статью снова.
          </>,
          "OK"
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

    this._overlayPresenter.notify({
      message: `Статья ${entity._ServerId} успешно сохранена`,
      intent: Intent.SUCCESS
    });
  }
}
