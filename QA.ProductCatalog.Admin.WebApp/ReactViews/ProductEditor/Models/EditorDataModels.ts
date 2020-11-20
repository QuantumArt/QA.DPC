import { ObservableMap, IObservableArray } from "mobx";
import { IStateTreeNode, IType } from "mobx-state-tree";
import { isObject } from "ProductEditor/Utils/TypeChecks";
import { ValidatableObject } from "ProductEditor/Packages/mst-validation-mixin";

export interface IArray<T extends ArticleObject> extends IObservableArray<T> {}

export interface IMap<T extends ArticleObject> extends ObservableMap<string, T> {
  put(value: T | object): T;
}

export interface TablesObject {
  readonly [contentName: string]: IMap<EntityObject>;
}

/** Объект, содержащий поля нормальной статьи или статьи-расширения */
export interface ArticleObject extends ValidatableObject, IStateTreeNode<ArticleSnapshot> {
  [field: string]: any;
  /** Серверный Id статьи, полученный при сохранении в БД */
  _ServerId?: number;
  /** Дата создания или последнего изменения статьи `QA.Core.Models.Entities.Article.Modified` */
  _Modified?: Date;
  /** Признак того, что объект является статьей-расшиернием */
  readonly _IsExtension: boolean;
}

export class ArticleObject {
  static _ClientId = "_ClientId";
  static _ServerId = "_ServerId";
  static _Modified = "_Modified";
  static _IsExtension = "_IsExtension";
  static _Extension = "_Extension";
  static _IsVirtual = "_IsVirtual";
}

export function isArticleObject(object: any): object is ArticleObject {
  return isObject(object) && ArticleObject._ServerId in object;
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
 * Используется в полях вида `Type_Extension`.
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

export interface ArticleSnapshot extends IType<any, any, any> {
  readonly [field: string]: any;
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
  /** Признак того, что объект не является статьей-расширением */
  readonly _IsExtension?: false;
  /** Признак того, что объект не должен быть сохранен на сервере */
  readonly _IsVirtual?: boolean;
}

export interface ExtensionSnapshot extends ArticleSnapshot {
  /** Признак того, что объект является статьей-расширением */
  readonly _IsExtension?: true;
}
