import { IExtendedObservableMap, IStateTreeNode } from "mobx-state-tree";
import { isObject, isString } from "Utils/TypeChecks";
import { ValidatableObject } from "mst-validation-mixin";

export interface StoreObject {
  readonly [contentName: string]: IExtendedObservableMap<ArticleObject>;
}

/** Объект, содержащий поля нормальной статьи */
export interface ArticleObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  /**
   * Локальный неизменяемый Id статьи на клиенте.
   * Совпадает с `_ServerId` для статей загруженных с сервера.
   * Является отрицательным для статей созданных на клиенте.
   */
  readonly _ClientId: number;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName: string;
  /** Серверный Id статьи, полученный при сохранении в БД */
  _ServerId?: number;
  /** Дата создания или последнего изменения статьи `QA.Core.Models.Entities.Article.Modified` */
  _Modified?: Date;
}

export function isArticleObject(object: any): object is ArticleObject {
  return isObject(object) && isString(object._ContentName) && "_ServerId" in object;
}

/** Объект, содержащий поля статьи-расширения */
export interface ExtensionObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName: string;
}

export function isExtensionObject(object: any): object is ExtensionObject {
  return isObject(object) && isString(object._ContentName) && !("_ServerId" in object);
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
  /**
   * Локальный неизменяемый Id статьи на клиенте.
   * Совпадает с `_ServerId` для статей загруженных с сервера.
   * Является отрицательным для статей созданных на клиенте.
   */
  readonly _ClientId: number;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName?: string;
  /** Серверный Id статьи, полученный при сохранении в БД */
  readonly _ServerId?: number;
  /** Дата создания или последнего изменения статьи `QA.Core.Models.Entities.Article.Modified` */
  readonly _Modified?: Date;
}
