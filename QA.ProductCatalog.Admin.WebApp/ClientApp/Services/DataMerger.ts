import { inject } from "react-ioc";
import { action, comparer } from "mobx";
import { getSnapshot } from "mobx-state-tree";
import { ValidatableObject } from "mst-validation-mixin";
import { isObject } from "Utils/TypeChecks";
import { StoreSnapshot, isArticleObject, isExtensionObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";

export class DataMerger {
  @inject private _dataContext: DataContext;
  private _hasMergeConfilicts = false;

  @action
  public mergeArticles(storeSnapshot: StoreSnapshot) {
    this._hasMergeConfilicts = false;

    Object.entries(storeSnapshot).forEach(([contentName, articlesById]) => {
      const collection = this._dataContext.store[contentName];
      if (collection) {
        Object.entries(articlesById).forEach(([id, articleSnapshot]) => {
          const article = collection.get(id);
          if (article) {
            this.mergeArticleSnapshot(article, articleSnapshot);
          } else {
            collection.put(articleSnapshot);
          }
        });
      }
    });

    return { hasMergeConfilicts: this._hasMergeConfilicts };
  }

  private mergeArticleSnapshot(model: Object, snapshot: Object) {
    const modelIsValidatable = isArticleObject(model) || isExtensionObject(model);
    const modelSnapshot = getSnapshot(model);

    Object.entries(snapshot).forEach(([name, valueSnapshot]) => {
      if (name === "_ClientId" || name === "_ContentName") {
        return;
      }
      const fieldSnapshot = modelSnapshot[name];

      if (isObject(fieldSnapshot) && isObject(valueSnapshot)) {
        this.mergeArticleSnapshot(model[name], valueSnapshot);
      } else if (comparer.structural(fieldSnapshot, valueSnapshot)) {
        this.markFieldAsUnchanged(modelIsValidatable, model, name);
      } else if (this.fieldIsEdited(modelIsValidatable, model, name)) {
        this._hasMergeConfilicts = true;
      } else {
        model[name] = valueSnapshot;
        this.markFieldAsUnchanged(modelIsValidatable, model, name);
      }
    });
  }

  @action
  public overwriteArticles(storeSnapshot: StoreSnapshot) {
    Object.entries(storeSnapshot).forEach(([contentName, articlesById]) => {
      const collection = this._dataContext.store[contentName];
      if (collection) {
        Object.entries(articlesById).forEach(([id, articleSnapshot]) => {
          const article = collection.get(id);
          if (article) {
            this.overwriteArticleSnapshot(article, articleSnapshot);
          } else {
            collection.put(articleSnapshot);
          }
        });
      }
    });
  }

  private overwriteArticleSnapshot(model: Object, snapshot: Object) {
    const modelIsValidatable = isArticleObject(model) || isExtensionObject(model);
    const modelSnapshot = getSnapshot(model);

    Object.entries(snapshot).forEach(([name, valueSnapshot]) => {
      if (name === "_ClientId" || name === "_ContentName") {
        return;
      }
      const fieldSnapshot = modelSnapshot[name];

      if (isObject(fieldSnapshot) && isObject(valueSnapshot)) {
        this.overwriteArticleSnapshot(model[name], valueSnapshot);
      } else if (!comparer.structural(fieldSnapshot, valueSnapshot)) {
        model[name] = valueSnapshot;
      }

      this.markFieldAsUnchanged(modelIsValidatable, model, name);
    });
  }

  private fieldIsEdited(modelIsValidatable: boolean, model: Object, name: string) {
    return modelIsValidatable && (model as ValidatableObject).isEdited(name);
  }

  private markFieldAsUnchanged(modelIsValidatable: boolean, model: Object, name: string) {
    if (modelIsValidatable) {
      (model as ValidatableObject).setChanged(name, false);
    }
  }
}
