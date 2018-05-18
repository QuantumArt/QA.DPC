import { isObject, isInteger } from "Utils/TypeChecking";

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
    const isArticleVisitedById = Object.create(null);

    return JSON.stringify(object, (key, value) => {
      if (key === "Id" && isInteger(value) && value < 0) {
        return this._idMapping[value] || value;
      } else if (isObject(value) && isInteger(value.Id)) {
        if (isArticleVisitedById[value.Id]) {
          return { Id: value.Id };
        }
        isArticleVisitedById[value.Id] = true;
        return value;
      }
      return value;
    });
  }

  /**
   * Заменяет все серверные Id на соответствующие им отрицательные Id (если они определены)
   */
  public deserialize<T>(json: string): T {
    return JSON.parse(json, (key, value) => {
      if (key === "Id" && isInteger(value)) {
        return this._idMapping[value] || value;
      }
      return value;
    });
  }
}
