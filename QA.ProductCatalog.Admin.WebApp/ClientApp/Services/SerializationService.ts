import { isObservable, toJS } from "mobx";
import { isObject, isInteger, isString } from "Utils/TypeChecks";

type IdMapping = { [id: number]: number };

export class SerializationService {
  private _nextId = -1;
  private _idMapping: IdMapping = Object.create(null);

  /**
   * Получение следующего отрицательного Id
   */
  public nextId(): number {
    return this._nextId--;
  }

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
      } else if (isString(value) && dateRegex.test(value)) {
        return new Date(value);
      }
      return value;
    });
  }
}

// Примеры для проверки
// 2018-03-12T10:46:32
// 2018-03-12T10:46:32Z
// 2018-03-12T10:46:32+03:00
// 2018-03-12T10:46:32.123
// 2018-03-12T10:46:32.123Z
// 2018-03-12T10:46:32.123+03:00
// 2018-02-10T09:42:14.4575689
// 2018-02-10T09:42:14.4575689Z
// 2018-02-10T09:42:14.4575689+03:00
const dateRegex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|\+\d{2}:\d{2})?/;
