/** Настройки ProductEditor */
export class EditorSettings {
  /** Id корневой статьи */
  ArticleId?: number;
  /** Id описания продукта */
  ProductDefinitionId: number;
}

/** URL-параметры корневого CustomAction */
export class EditorQueryParams {
  [name: string]: any;
  backend_sid: string;
  customerCode: string;
  hostUID: string;
}
