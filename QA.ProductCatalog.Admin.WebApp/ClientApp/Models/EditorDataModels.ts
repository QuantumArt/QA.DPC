import { IExtendedObservableMap, IStateTreeNode } from "mobx-state-tree";
import { isObject, isInteger, isString } from "Utils/TypeChecks";
import { ValidatableObject } from "mst-validation-mixin";

export interface StoreObject {
  readonly [contentName: string]: IExtendedObservableMap<ArticleObject>;
}

export interface ArticleObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  readonly Id: number;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName: string;
  /** Дата создания или последнего изменения статьи `QA.Core.Models.Entities.Article.Modified` */
  _Modified: Date;
}

export function isArticleObject(object: any): object is ArticleObject {
  return isObject(object) && isString(object._ContentName) && isInteger(object.Id);
}

export interface ExtensionObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName: string;
}

export function isExtensionObject(object: any): object is ExtensionObject {
  return isObject(object) && isString(object._ContentName) && !("Id" in object);
}

export interface StoreSnapshot {
  readonly [contentName: string]: {
    readonly [articleId: string]: ArticleSnapshot;
  };
}

export interface MutableStoreSnapshot {
  [contentName: string]: {
    [articleId: string]: ArticleSnapshot;
  };
}

export interface ArticleSnapshot {
  readonly [field: string]: any;
  readonly Id: number;
  readonly _ContentName?: string;
  readonly _Modified?: Date;
}
