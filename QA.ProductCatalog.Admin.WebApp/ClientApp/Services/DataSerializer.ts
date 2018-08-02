import { isIsoDateString, isObject, isArray } from "Utils/TypeChecks";
import { ArticleObject, isArticleObject } from "Models/EditorDataModels";

/**
 * Отображения положительных серверных Id на  отрицательные клиентские,
 * сгруппированные по имени контента.
 */
export interface IdMappingsByContent {
  [contentName: string]: {
    [serverArticleId: number]: number;
  };
}

export class DataSerializer {
  private _idMappingsByContent: IdMappingsByContent = Object.create(null);

  /** Обновление соответствия положительные серверные Id => отрицательные клиентские Id  */
  public extendIdMappings(idMappingsByContent: IdMappingsByContent) {
    Object.entries(idMappingsByContent).forEach(([contentName, mappingDict]) => {
      let currentDict = this._idMappingsByContent[contentName];
      if (!currentDict) {
        this._idMappingsByContent[contentName] = currentDict = Object.create(null);
      }
      Object.entries(mappingDict).forEach(([serverId, clientId]: any) => {
        currentDict[serverId] = clientId;
      });
    });
  }

  /** Заменяет все отрицательные клиентские Id на соответствующие им серверные Id (если они уже определены). */
  public serialize(article: ArticleObject): string {
    function toObject(argument: any) {
      if (isObject(argument)) {
        const object: { _ServerId?: number } = {};
        Object.keys(argument).forEach(key => {
          object[key] = toObject(argument[key]);
        });
        if (isArticleObject(argument)) {
          object._ServerId = argument._ServerId > 0 ? argument._ServerId : argument._ClientId;
        }
        return object;
      }
      if (isArray(argument)) {
        return argument.map(toObject);
      }
      return argument;
    }

    return JSON.stringify(toObject(article));
  }

  /** Заменяет все серверные Id на соответствующие им клиентские отрицательные Id (если они определены) */
  public deserialize<T = any>(json: string): T {
    return JSON.parse(json, (_key, value) => {
      if (isArticleObject(value)) {
        const clientId = this.getClientId(value);
        return { ...value, _ClientId: clientId };
      } else if (isIsoDateString(value)) {
        return Number(new Date(value));
      }
      return value;
    });
  }

  private getClientId(article: ArticleObject) {
    const mappingDict = this._idMappingsByContent[article._ContentName];
    return (mappingDict && mappingDict[article._ServerId]) || article._ServerId;
  }
}
