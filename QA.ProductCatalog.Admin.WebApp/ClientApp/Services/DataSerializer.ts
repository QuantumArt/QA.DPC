import { isIsoDateString, isString } from "Utils/TypeChecks";
import { isEntityObject, ArticleObject, EntityObject } from "Models/EditorDataModels";
import {
  ContentSchema,
  isExtensionField,
  UpdatingMode,
  isSingleRelationField,
  isMultiRelationField
} from "Models/EditorSchemaModels";

/**
 * Отображения положительных серверных Id на  отрицательные клиентские,
 * сгруппированные по имени контента.
 */
export interface IdMapping {
  ClientId: number;
  ServerId: number;
}

export class DataSerializer {
  private _idMappingDict: { [serverId: number]: number } = Object.create(null);

  /** Обновление соответствия положительные серверные Id => отрицательные клиентские Id  */
  public extendIdMappings(idMappings: IdMapping[]) {
    idMappings.forEach(pair => {
      this._idMappingDict[pair.ServerId] = pair.ClientId;
    });
  }

  /**
   * Заменяет все серверные Id на соответствующие им
   * клиентские отрицательные Id (если они определены).
   * Преобразует ISO Date strings в Unix time.
   */
  public deserialize<T = any>(object: T): T;
  public deserialize<T = any>(object: object): T;
  public deserialize<T = any>(json: string): T;
  public deserialize<T = any>(argument: any): T {
    const json = isString(argument) ? argument : JSON.stringify(argument);
    return JSON.parse(json, (_key, value) => {
      if (isEntityObject(value)) {
        // @ts-ignore
        value._ClientId = this.getClientId(value._ServerId);
        return value;
      } else if (isIsoDateString(value)) {
        return Number(new Date(value));
      }
      return value;
    });
  }

  /**
   * Статья может быть создана на клиенте: `{ _ClientId: -1, _ServerId: -1 }`,
   * затем сохранена: `{ _ClientId: -1, _ServerId: 100500 }, _idMappingDict === { 100500: -1 }`,
   * затем удалена параллельно другим пользователем, и наконец, вновьс охранена  на клиенте
   * `{ _ClientId: -1, _ServerId: 100501 }, _idMappingDict === { 100501: 100500, 100500: -1 }`.
   * Таким образом, для получения исходного `_ClientId` мы должны пройти по цепочке отображения
   * _ServerId --> _ClientId.
   */
  private getClientId(serverId: number) {
    for (let i = 0; i < 9999; i++) {
      const clientId = this._idMappingDict[serverId];
      if (!clientId) {
        return serverId;
      }
      serverId = clientId;
    }
    throw new Error("There is a cycle in DataSerializer._idMappingDict");
  }

  public serialize(article: ArticleObject, contentSchema: ContentSchema) {
    const snapshot = {
      _ServerId: article._ServerId,
      _Modified: article._Modified
    };

    for (const fieldName in contentSchema.Fields) {
      const fieldValue = article[fieldName];
      if (fieldValue == null) {
        continue;
      }

      const fieldSchema = contentSchema.Fields[fieldName];
      if (isSingleRelationField(fieldSchema)) {
        const relatedEntity = fieldValue as EntityObject;
        if (relatedEntity._IsVirtual) {
          snapshot[fieldName] = null;
        } else if (fieldSchema.UpdatingMode === UpdatingMode.Ignore) {
          snapshot[fieldName] = { _ServerId: relatedEntity._ServerId };
        } else {
          snapshot[fieldName] = this.serialize(relatedEntity, fieldSchema.RelatedContent);
        }
      } else if (isMultiRelationField(fieldSchema)) {
        const relatedCollection = fieldValue as EntityObject[];
        if (fieldSchema.UpdatingMode === UpdatingMode.Ignore) {
          snapshot[fieldName] = relatedCollection
            .filter(entity => !entity._IsVirtual)
            .map(entity => ({ _ServerId: entity._ServerId }));
        } else {
          snapshot[fieldName] = relatedCollection
            .filter(entity => !entity._IsVirtual)
            .map(entity => this.serialize(entity, fieldSchema.RelatedContent));
        }
      } else if (isExtensionField(fieldSchema)) {
        const extensionFieldName = `${fieldName}${ArticleObject._Extension}`;
        const extensionArticle = article[extensionFieldName][fieldValue] as ArticleObject;
        const extensionContentSchema = fieldSchema.ExtensionContents[fieldValue];
        snapshot[fieldName] = fieldValue;
        snapshot[extensionFieldName] = {
          [fieldValue]: this.serialize(extensionArticle, extensionContentSchema)
        };
      } else {
        snapshot[fieldName] = fieldValue;
      }
    }

    return snapshot as { [x: string]: any } & typeof snapshot;
  }
}
