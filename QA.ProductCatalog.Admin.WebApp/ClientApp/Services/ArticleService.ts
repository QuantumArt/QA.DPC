import { observable, extendObservable, isObservableArray, runInAction, toJS } from "mobx";

type Article = { Id: number; [x: string]: any };

export class ArticleService {
  public articlesById: { [id: number]: Article } = Object.create(null);
  public rootArticle: Article = null;

  constructor(rootArticle: Article) {
    runInAction(() => {
      this.createArticles(rootArticle);
      this.mergeArticles(rootArticle);
    });

    this.rootArticle = this.articlesById[rootArticle.Id];
  }

  private createArticles = (arg: any) => {
    if (Array.isArray(arg)) {
      arg.forEach(this.createArticles);
    } else if (isObject(arg)) {
      if (isInteger(arg.Id) && !this.articlesById[arg.Id]) {
        this.articlesById[arg.Id] = observable({}) as any;
      }
      Object.values(arg).forEach(this.createArticles);
    }
  };

  private mergeArticles = (arg: any) => {
    if (Array.isArray(arg)) {
      arg.forEach(this.mergeArticles);
    } else if (isObject(arg)) {
      if (isInteger(arg.Id)) {
        const article = this.articlesById[arg.Id];
        Object.entries(arg).forEach(([key, value]: [string, any]) => {
          if (!(key in article)) {
            extendObservable(article, {
              [key]: Array.isArray(value) ? [] : null
            });
          }
          const articleField = article[key];
          if (isObject(value) && isInteger(value.Id)) {
            // M2ORelation
            if (articleField === null) {
              article[key] = this.articlesById[value.Id];
            }
          } else if (Array.isArray(value)) {
            // BackwardRelation, O2MRelation or M2MRelation
            if (value.length > 0 && articleField.length === 0) {
              article[key] = value.map(
                el => (isObject(el) && isInteger(el.Id) ? this.articlesById[el.Id] : el)
              );
            }
          } else if (isObject(value) && "Value" in value && "Contents" in value) {
            // Extension
            if (articleField === null || articleField.Value === null) {
              const contents = {};
              Object.entries(value.Contents).forEach(([name, extension]: [string, any]) => {
                contents[name] = isInteger(extension.Id)
                  ? this.articlesById[extension.Id]
                  : extension;
              });
              article[key] = {
                Value: value.Value,
                Contents: contents
              };
            }
          } else if (articleField === null) {
            article[key] = value;
          }
        });
      }
      Object.values(arg).forEach(this.mergeArticles);
    }
  };

  public addReference(editedArticle: Article, fieldName: string, relatedArticle: Article) {
    if (!this.articlesById[relatedArticle.Id]) {
      // TODO: merge
    }
    const mergedArticle = this.articlesById[relatedArticle.Id];
    const field = editedArticle[fieldName];
    if (isObservableArray(field)) {
      if (!field.find(a => a.Id === mergedArticle.Id)) {
        field.push(mergedArticle);
      }
    } else {
      editedArticle[fieldName] = mergedArticle;
    }
  }

  public removeReference(editedArticle: Article, fieldName: string, relatedArticle: Article) {
    const field = editedArticle[fieldName];
    if (isObservableArray(field)) {
      field.remove(field.find(a => a.Id === relatedArticle.Id));
    } else {
      editedArticle[fieldName] = null;
    }
  }

  public serializeArticle(article: Article) {
    const plainArticle = toJS(article);
    const isArticleVisitedById = Object.create(null);

    return JSON.stringify(plainArticle, (_key, value) => {
      if (isObject(value) && isInteger(value.Id)) {
        if (isArticleVisitedById[value.Id]) {
          return { Id: value.Id };
        }
        isArticleVisitedById[value.Id] = true;
        return value;
      }
      return value;
    });
  }
}

function isInteger(arg): arg is number {
  return typeof arg === "number" && /^-?[0-9]+$/.test(String(arg));
}

function isObject(arg: any): arg is Object {
  return typeof arg === "object" && arg !== null && !Array.isArray(arg);
}
