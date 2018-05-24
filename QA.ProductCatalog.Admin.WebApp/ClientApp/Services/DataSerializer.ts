import { isObservable, toJS } from "mobx";
import { isObject, isInteger, isIsoDateString } from "Utils/TypeChecks";

type IdMapping = { [id: number]: number };

export class DataSerializer {
  private _idMapping: IdMapping = Object.create(null);

  /**
   * Обновление соответствия отрицательные Id => серверные Id
   */
  public updateIdMapping(idMapping: IdMapping) {
    Object.entries(idMapping).forEach(([key, value]: any) => {
      this._idMapping[key] = value;
      this._idMapping[value] = key;
    });
  }

  /**
   * Заменяет все повторные вхождения одной и той же статьи на объект `{ Id: article.Id }`
   * Заменяет все отрицательные Id на соответствующие им серверные Id (если они уже определены)
   */
  public serialize(object: any): string {
    if (isObservable(object)) {
      object = toJS(object);
    }
    const visitedArticles = new Set();

    return JSON.stringify(object, (key, value) => {
      if (key === "Id" && isInteger(value) && value < 0) {
        return this._idMapping[value] || value;
      } else if (isObject(value) && isInteger(value.Id)) {
        if (visitedArticles.has(value)) {
          return { Id: value.Id };
        }
        visitedArticles.add(value);
        return value;
      }
      return value;
    });
  }

  /**
   * Заменяет все серверные Id на соответствующие им отрицательные Id (если они определены)
   */
  public deserialize<T = any>(json: string): T {
    return JSON.parse(json, (key, value) => {
      if (key === "Id" && isInteger(value)) {
        return this._idMapping[value] || value;
      } else if (isIsoDateString(value)) {
        return new Date(value);
      }
      return value;
    });
  }
}
