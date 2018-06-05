import { IExtendedObservableMap } from "mobx-state-tree";
import { isObject, isInteger, isString } from "Utils/TypeChecks";

export interface StoreObject {
  readonly [name: string]: IExtendedObservableMap<ArticleObject>;
}

export interface ArticleObject {
  [field: string]: any;
  readonly Id: number;
  readonly ContentName: string;
  Timestamp: Date;
}

export function isArticleObject(object: any): object is ArticleObject {
  return isObject(object) && isInteger(object.Id) && isString(object.ContentName);
}

export interface StoreSnapshot {
  readonly [content: string]: {
    readonly [id: string]: ArticleSnapshot;
  };
}

export interface ArticleSnapshot {
  readonly [field: string]: any;
  readonly Id: number;
  readonly ContentName?: string;
  readonly Timestamp?: Date;
}
