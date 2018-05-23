import { types as t } from "mobx-state-tree";

export interface StoreSnapshot {
  [content: string]: {
    [id: string]: CollectionSnapshot;
  };
}

export interface CollectionSnapshot<T = ArticleSnapshot> {
  [id: number]: T;
}

export interface ArticleSnapshot {
  Id: number;
  ContentName: string;
  Timestamp: Date;

  [field: string]: any;
}

/** Файл, загружаемый в QPublishing */
export const FileType = t.model("FileModel", {
  /** Имя файла */
  Name: t.string,
  /** URL файла */
  AbsoluteUrl: t.string
});
