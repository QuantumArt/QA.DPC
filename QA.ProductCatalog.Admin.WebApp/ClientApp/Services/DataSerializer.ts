import { isIsoDateString, isObject, isArray } from "Utils/TypeChecks";
import { ArticleObject, isArticleObject } from "Models/EditorDataModels";

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
   * Заменяет все отрицательные клиентские Id на соответствующие им
   * серверные Id (если они уже определены).
   */
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

  /**
   * Заменяет все серверные Id на соответствующие им
   * клиентские отрицательные Id (если они определены).
   * Преобразует ISO Date strings в Unix time.
   */
  public deserialize<T = any>(json: string): T {
    return JSON.parse(json, (_key, value) => {
      if (isArticleObject(value)) {
        // @ts-ignore
        value._ClientId = this._idMappingDict[value._ServerId] || value._ServerId;
        return value;
      } else if (isIsoDateString(value)) {
        return Number(new Date(value));
      }
      return value;
    });
  }
}
