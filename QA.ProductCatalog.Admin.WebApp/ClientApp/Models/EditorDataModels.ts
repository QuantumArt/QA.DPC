import { IExtendedObservableMap, IStateTreeNode } from "mobx-state-tree";
import { isObject, isString } from "Utils/TypeChecks";
import { ValidatableObject } from "mst-validation-mixin";

export interface StoreObject {
  readonly [contentName: string]: IExtendedObservableMap<EntityObject>;
}

/** Объект, содержащий поля нормальной статьи или статьи-расширения */
export interface ArticleObject extends ValidatableObject, IStateTreeNode {
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

export function isArticleObject(object: any): object is ArticleObject {
  return isObject(object) && isString(object._ContentName);
}

/** Объект, содержащий поля нормальной статьи */
export interface EntityObject extends ArticleObject {
  /**
   * Локальный неизменяемый Id статьи на клиенте.
   * Совпадает с `_ServerId` для статей загруженных с сервера.
   * Является отрицательным для статей созданных на клиенте.
   */
  readonly _ClientId: number;
  /** Признак того, что объект не является статьей-расшиернием */
  readonly _IsExtension: false;
}

export function isEntityObject(object: any): object is EntityObject {
  return isArticleObject(object) && !object._IsExtension;
}

/** Объект, содержащий поля статьи-расширения */
export interface ExtensionObject extends ArticleObject {
  /** Признак того, что объект является статьей-расшиернием */
  readonly _IsExtension: true;
}

export function isExtensionObject(object: any): object is ExtensionObject {
  return isArticleObject(object) && object._IsExtension;
}

export interface StoreSnapshot {
  readonly [contentName: string]: {
    readonly [articleId: string]: EntitySnapshot;
  };
}

export interface EntitySnapshot {
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
  /** Признак того, что объект не является статьей-расшиернием */
  readonly _IsExtension?: false;
}
