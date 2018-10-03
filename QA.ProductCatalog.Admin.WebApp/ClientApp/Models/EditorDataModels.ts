import { IMSTMap, IStateTreeNode } from "mobx-state-tree";
import { isObject, isString } from "Utils/TypeChecks";
import { ValidatableObject } from "mst-validation-mixin";

export interface TablesObject {
  readonly [contentName: string]: IMSTMap<any, any, EntityObject>;
}

/** Объект, содержащий поля нормальной статьи или статьи-расширения */
export interface ArticleObject
  extends ValidatableObject,
    IStateTreeNode<ArticleSnapshot, ArticleSnapshot> {
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

export class ArticleObject {
  static _ClientId = "_ClientId";
  static _ServerId = "_ServerId";
  static _ContentName = "_ContentName";
  static _Modified = "_Modified";
  static _IsExtension = "_IsExtension";
  static _Contents = "_Contents";
  static _IsVirtual = "_IsVirtual";
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
  /** Признак того, что объект не должен быть сохранен на сервере */
  _IsVirtual: boolean;
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

/**
 * Словарь, содержащий отображение названий контентов на статьи-расширения.
 * Используется в полях вида `Type_Contents`.
 */
export interface ExtensionDictionary {
  [contentName: string]: ExtensionObject;
}

export function isExtensionDictionary(object: any): object is ExtensionObject {
  return isObject(object) && Object.values(object).every(isExtensionObject);
}

export interface TablesSnapshot {
  readonly [contentName: string]: {
    readonly [articleId: string]: EntitySnapshot;
  };
}

export interface ArticleSnapshot {
  readonly [field: string]: any;
  /** .NET-название контента статьи `Quantumart.QP8.BLL.Content.NetName` */
  readonly _ContentName?: string;
  /** Серверный Id статьи, полученный при сохранении в БД */
  readonly _ServerId?: number;
  /** Дата создания или последнего изменения статьи `QA.Core.Models.Entities.Article.Modified` */
  readonly _Modified?: number;
  /** Признак того, что объект является статьей-расшиернием */
  readonly _IsExtension?: boolean;
}

export interface EntitySnapshot extends ArticleSnapshot {
  /**
   * Локальный неизменяемый Id статьи на клиенте.
   * Совпадает с `_ServerId` для статей загруженных с сервера.
   * Является отрицательным для статей созданных на клиенте.
   */
  readonly _ClientId: number;
  /** Признак того, что объект не является статьей-расшиернием */
  readonly _IsExtension?: false;
  /** Признак того, что объект не должен быть сохранен на сервере */
  readonly _IsVirtual?: boolean;
}

export interface ExtensionSnapshot extends ArticleSnapshot {
  /** Признак того, что объект является статьей-расшиернием */
  readonly _IsExtension?: true;
}
