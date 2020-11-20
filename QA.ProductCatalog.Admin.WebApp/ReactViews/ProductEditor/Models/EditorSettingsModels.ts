/** Обище настройки ProductEditor */
export class EditorSettings {
  /** Id корневой статьи */
  ArticleId?: number;
  /** Id описания продукта */
  ProductDefinitionId: number;
  /**
   * ISO-код локали текущего пользователя
   * @example "en-US", "ru-RU", "kk-KZ"
   */
  UserLocale: string;
}

/** URL-параметры корневого CustomAction */
export class EditorQueryParams {
  [name: string]: any;
  backend_sid: string;
  customerCode: string;
  hostUID: string;
}

/** Настройки синхронизации статусов публикации */
export class PublicationTrackerSettings {
  /**
   * Названия контентов, статус публикации которых нужно просматривать
   * @default ["Product"]
   */
  contentNames = ["Product"];
  /**
   * Интервал обновления в милисекундах
   * @default 5000
   */
  updateInterval = 5000;
}
