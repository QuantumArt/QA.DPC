import { isObservable, toJS } from "mobx";
import { isIsoDateString } from "Utils/TypeChecks";
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

  /**
   * Заменяет все повторные вхождения одной и той же статьи на объект `{ _ServerId: article._ServerId }`
   * Заменяет все отрицательные клиентские Id на соответствующие им серверные Id (если они уже определены)
   */
  public serialize(object: ArticleObject): string {
    if (isObservable(object)) {
      object = toJS(object);
    }
    const visitedArticles = new Set<ArticleObject>();

    return JSON.stringify(object, (_key, value) => {
      if (isArticleObject(value)) {
        const serverId = value._ServerId > 0 ? value._ServerId : value._ClientId;
        if (visitedArticles.has(value)) {
          return { _ServerId: serverId };
        }
        visitedArticles.add(value);
        if (value._ServerId !== serverId) {
          return { ...value, _ServerId: serverId };
        }
      }
      return value;
    });
  }

  /** Заменяет все серверные Id на соответствующие им клиентские отрицательные Id (если они определены) */
  public deserialize<T = any>(json: string): T {
    return JSON.parse(json, (_key, value) => {
      if (isArticleObject(value)) {
        const clientId = this.getClientId(value);
        return { ...value, _ClientId: clientId };
      } else if (isIsoDateString(value)) {
        return new Date(value);
      }
      return value;
    });
  }

  private getClientId(article: ArticleObject) {
    const mappingDict = this._idMappingsByContent[article._ContentName];
    return (mappingDict && mappingDict[article._ServerId]) || article._ServerId;
  }
}
