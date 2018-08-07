import { IExtendedObservableMap, IStateTreeNode } from "mobx-state-tree";
import { isObject, isString } from "Utils/TypeChecks";
import { ValidatableObject } from "mst-validation-mixin";

export interface StoreObject {
  readonly [contentName: string]: IExtendedObservableMap<ArticleObject>;
}

/** Объект, содержащий поля нормальной статьи или статьи-расширения */
export interface EntityObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  /** Серверный Id статьи, полученный при сохранении в БД */
  _ServerId: number;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName: string;
  /** Дата создания или последнего изменения статьи `QA.Core.Models.Entities.Article.Modified` */
  _Modified?: Date;
  /** Признак того, что объект является статьей-расшиернием */
  readonly _IsExtension: boolean;
}

export function isEntityObject(object: any): object is EntityObject {
  return isObject(object) && isString(object._ContentName);
}

/** Объект, содержащий поля нормальной статьи */
export interface ArticleObject extends EntityObject {
  /**
   * Локальный неизменяемый Id статьи на клиенте.
   * Совпадает с `_ServerId` для статей загруженных с сервера.
   * Является отрицательным для статей созданных на клиенте.
   */
  readonly _ClientId: number;
  /** Признак того, что объект не является статьей-расшиернием */
  readonly _IsExtension: false;
}

export function isArticleObject(object: any): object is ArticleObject {
  return isEntityObject(object) && !object._IsExtension;
}

/** Объект, содержащий поля статьи-расширения */
export interface ExtensionObject extends EntityObject {
  /** Признак того, что объект является статьей-расшиернием */
  readonly _IsExtension: true;
}

export function isExtensionObject(object: any): object is ExtensionObject {
  return isEntityObject(object) && object._IsExtension;
}

export interface StoreSnapshot {
  readonly [contentName: string]: {
    readonly [articleId: string]: ArticleSnapshot;
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
