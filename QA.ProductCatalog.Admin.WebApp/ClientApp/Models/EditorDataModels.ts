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
