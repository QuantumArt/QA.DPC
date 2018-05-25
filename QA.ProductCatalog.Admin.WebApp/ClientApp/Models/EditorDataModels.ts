import { types as t, IExtendedObservableMap } from "mobx-state-tree";

export interface StoreObject {
  readonly [name: string]: IExtendedObservableMap<ArticleObject>;
}

export interface ArticleObject {
  readonly Id: number;
  readonly ContentName: string;
  Timestamp: Date;

  [field: string]: any;
}

export interface StoreSnapshot {
  readonly [content: string]: {
    readonly [id: string]: ArticleSnapshot;
  };
}

export interface ArticleSnapshot {
  readonly Id: number;
  readonly ContentName?: string;
  readonly Timestamp?: Date;

  readonly [field: string]: any;
}

/** Файл, загружаемый в QPublishing */
export const FileType = t.model("FileModel", {
  /** Имя файла */
  Name: t.string,
  /** URL файла */
  AbsoluteUrl: t.string
});
