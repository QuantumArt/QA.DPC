/** Обище настройки ProductEditor */
var EditorSettings = /** @class */ (function() {
  function EditorSettings() {}
  return EditorSettings;
})();
export { EditorSettings };
/** URL-параметры корневого CustomAction */
var EditorQueryParams = /** @class */ (function() {
  function EditorQueryParams() {}
  return EditorQueryParams;
})();
export { EditorQueryParams };
/** Настройки синхронизации статусов публикации */
var PublicationTrackerSettings = /** @class */ (function() {
  function PublicationTrackerSettings() {
    /**
     * Названия контентов, статус публикации которых нужно просматривать
     * @default ["Product"]
     */
    this.contentNames = ["Product"];
    /**
     * Интервал обновления в милисекундах
     * @default 5000
     */
    this.updateInterval = 5000;
  }
  return PublicationTrackerSettings;
})();
export { PublicationTrackerSettings };
//# sourceMappingURL=EditorSettingsModels.js.map
