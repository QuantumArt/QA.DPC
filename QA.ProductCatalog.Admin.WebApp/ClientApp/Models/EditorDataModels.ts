import { types as t, IExtendedObservableMap } from "mobx-state-tree";

export interface StoreObject {
  readonly [name: string]: IExtendedObservableMap<ArticleObject>;
}

export interface ArticleObject {
  [field: string]: any;
  readonly Id: number;
  readonly ContentName: string;
  Timestamp: Date;
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

/** Файл, загружаемый в QPublishing */
export const FileType = t.model("FileModel", {
  /** Имя файла */
  Name: t.string,
  /** URL файла */
  AbsoluteUrl: t.string
});
