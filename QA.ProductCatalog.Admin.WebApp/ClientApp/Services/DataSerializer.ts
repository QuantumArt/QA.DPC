import { isObservable, toJS } from "mobx";
import { isIsoDateString } from "Utils/TypeChecks";
import { ArticleObject, isArticleObject } from "Models/EditorDataModels";

interface IdMapping {
  [id: number]: number;
}

interface IdMappings {
  [content: string]: IdMapping;
}

export class DataSerializer {
  private _idMappings: IdMappings = Object.create(null);

  /**
   * Обновление соответствия отрицательные Id => серверные Id
   */
  public updateIdMapping(idMappings: IdMappings) {
    Object.entries(idMappings).forEach(([name, idMapping]) => {
      let currentMapping = this._idMappings[name];
      if (!currentMapping) {
        this._idMappings[name] = currentMapping = Object.create(null);
      }
      Object.entries(idMapping).forEach(([idLeft, idRight]: any) => {
        currentMapping[idLeft] = idRight;
        currentMapping[idRight] = idLeft;
      });
    });
  }

  public getServerId(article: ArticleObject) {
    if (article.Id < 0) {
      const idMapping = this._idMappings[article.ContentName];
      if (idMapping) {
        return idMapping[article.Id] || article.Id;
      }
    }
    return article.Id;
  }

  private getClientId(article: ArticleObject) {
    const idMapping = this._idMappings[article.ContentName];
    return (idMapping && idMapping[article.Id]) || article.Id;
  }

  /**
   * Заменяет все повторные вхождения одной и той же статьи на объект `{ Id: article.Id }`
   * Заменяет все отрицательные Id на соответствующие им серверные Id (если они уже определены)
   */
  public serialize(object: ArticleObject): string {
    if (isObservable(object)) {
      object = toJS(object);
    }
    const visitedArticles = new Set<ArticleObject>();

    return JSON.stringify(object, (_key, value) => {
      if (isArticleObject(value)) {
        const id = this.getServerId(value);
        if (visitedArticles.has(value)) {
          return { Id: id };
        }
        visitedArticles.add(value);
        if (value.Id !== id) {
          return { ...value, Id: id };
        }
      }
      return value;
    });
  }

  /**
   * Заменяет все серверные Id на соответствующие им отрицательные Id (если они определены)
   */
  public deserialize<T = any>(json: string): T {
    return JSON.parse(json, (_key, value) => {
      if (isArticleObject(value)) {
        const id = this.getClientId(value);
        if (value.Id !== id) {
          return { ...value, Id: id };
        }
      } else if (isIsoDateString(value)) {
        return new Date(value);
      }
      return value;
    });
  }
}
