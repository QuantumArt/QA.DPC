import { IExtendedObservableMap, IStateTreeNode } from "mobx-state-tree";
import { isObject, isInteger, isString } from "Utils/TypeChecks";
import { ValidatableObject } from "mst-validation-mixin";

export interface StoreObject {
  readonly [name: string]: IExtendedObservableMap<ArticleObject>;
}

export interface ArticleObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  readonly Id: number;
  readonly ContentName: string;
  Modified: Date;
}

export function isArticleObject(object: any): object is ArticleObject {
  return isObject(object) && isString(object.ContentName) && isInteger(object.Id);
}

export interface ExtensionObject extends ValidatableObject, IStateTreeNode {
  [field: string]: any;
  readonly ContentName: string;
}

export function isExtensionObject(object: any): object is ExtensionObject {
  return isObject(object) && isString(object.ContentName) && !("Id" in object);
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
  readonly Modified?: Date;
}
