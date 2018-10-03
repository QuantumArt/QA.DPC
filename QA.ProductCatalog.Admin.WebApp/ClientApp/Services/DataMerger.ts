import { inject } from "react-ioc";
import { action, comparer } from "mobx";
import { getSnapshot } from "mobx-state-tree";
import {
  TablesSnapshot,
  ArticleSnapshot,
  ArticleObject,
  isExtensionDictionary
} from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";

export enum MergeStrategy {
  /** Обновить только неизмененные поля. Не менять поле `_Modified`. */
  KeepTimestamp = 1,
  /** Обновить только неизмененные поля. Обновить поле `_Modified`. */
  ClientWins = 2,
  /** Перезаписать поля статей, которые были изменены на сервере. Обновить поле `_Modified`. */
  ServerWins = 3,
  /** Перезаписать с сервера все поля всех статей. Обновить поле `_Modified`. */
  Overwrite = 4
}

export class DataMerger {
  @inject private _dataContext: DataContext;

  public tablesHasConflicts(tablesSnapshot: TablesSnapshot) {
    for (const [contentName, articlesById] of Object.entries(tablesSnapshot)) {
      const collection = this._dataContext.tables[contentName];
      for (const [id, articleSnapshot] of Object.entries(articlesById)) {
        const article = collection.get(id);
        if (article && this.articleHasConfilcts(article, articleSnapshot)) {
          return true;
        }
      }
    }
    return false;
  }

  public articleHasConfilcts(article: ArticleObject, snapshot: ArticleSnapshot) {
    const oldSnapshot = getSnapshot<ArticleSnapshot>(article);

    for (const [name, fieldSnapshot] of Object.entries(snapshot)) {
      const fieldValue = article[name];

      if (name.endsWith(ArticleObject._Contents) && isExtensionDictionary(fieldValue)) {
        for (const [contentName, extensionSnapshot] of Object.entries(fieldSnapshot)) {
          if (this.articleHasConfilcts(fieldValue[contentName], extensionSnapshot)) {
            return true;
          }
        }
      } else if (
        oldSnapshot._Modified < snapshot._Modified &&
        article.isEdited(name) &&
        !comparer.structural(oldSnapshot[name], fieldSnapshot)
      ) {
        return true;
      }
    }
    return false;
  }

  @action
  public mergeTables(snapshot: TablesSnapshot, strategy: MergeStrategy) {
    Object.entries(snapshot).forEach(([contentName, articlesById]) => {
      const collection = this._dataContext.tables[contentName];
      if (collection) {
        Object.entries(articlesById).forEach(([id, articleSnapshot]) => {
          const article = collection.get(id);
          if (article) {
            this.mergeArticle(article, articleSnapshot, strategy);
          } else {
            collection.put(articleSnapshot);
          }
        });
      }
    });
  }

  @action
  public mergeArticle(article: ArticleObject, snapshot: ArticleSnapshot, strategy: MergeStrategy) {
    const oldSnapshot = getSnapshot<ArticleSnapshot>(article);

    Object.entries(snapshot).forEach(([name, fieldSnapshot]) => {
      const fieldValue = article[name];

      if (name === ArticleObject._Modified) {
        if (oldSnapshot._Modified !== snapshot._Modified) {
          switch (strategy) {
            case MergeStrategy.KeepTimestamp:
              break;
            case MergeStrategy.ClientWins:
            case MergeStrategy.ServerWins:
            case MergeStrategy.Overwrite:
              this.setProperty(article, name, fieldSnapshot);
              break;
            default:
              throw new Error("Uncovered MergeStrategy");
          }
        }
      } else if (name.endsWith(ArticleObject._Contents) && isExtensionDictionary(fieldValue)) {
        Object.entries(fieldSnapshot).forEach(([contentName, extensionSnapshot]) => {
          this.mergeArticle(fieldValue[contentName], extensionSnapshot, strategy);
        });
      } else if (comparer.structural(oldSnapshot[name], fieldSnapshot)) {
        article.setChanged(name, false);
      } else if (article.isEdited(name)) {
        switch (strategy) {
          case MergeStrategy.KeepTimestamp:
          case MergeStrategy.ClientWins:
            break;
          case MergeStrategy.ServerWins:
            if (oldSnapshot._Modified < snapshot._Modified) {
              this.setProperty(article, name, fieldSnapshot);
            }
            break;
          case MergeStrategy.Overwrite:
            this.setProperty(article, name, fieldSnapshot);
            break;
          default:
            throw new Error("Uncovered MergeStrategy");
        }
      } else {
        this.setProperty(article, name, fieldSnapshot);
      }
    });
  }

  private setProperty(article: ArticleObject, name: string, value: any) {
    article[name] = value;
    article.setChanged(name, false);
  }
}
