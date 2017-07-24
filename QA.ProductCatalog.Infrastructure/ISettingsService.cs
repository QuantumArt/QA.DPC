namespace QA.ProductCatalog.Infrastructure
{
    public interface ISettingsService
    {
        string GetSetting(SettingsTitles title);
        string GetSetting(string title);
        string GetActionCode(string name);
    }

    public enum SettingsTitles
    {
        PRODUCT_CONTROL_CONTENT_ID,
        PRODUCT_DEFINITIONS_CONTENT_ID,  //Идентификатор контента Product Definitions
        PRODUCT_TYPES_CONTENT_ID,    //Идентификатор контента Типы продуктов
        PRODUCT_TYPES_FIELD_NAME,   //Название поле продукта, определяющего тип продукта
        REGIONAL_TAGS_CONTENT_ID,    //Идентификатор контента Региональные теги
        REGIONAL_TAGS_VALUES_CONTENT_ID,    //Идентификатор контента Значения региональных тегов
        REGIONAL_TAGS_RECURSIVE_DEPTH, //Глубина рекурсивных замен внутри самих тэгов,
        REGIONS_CONTENT_ID,    //Идентификатор контента Регионы
        MARKETING_PRODUCT_CONTENT_ID, //Идентификатор контента Марекетинговые регионы
        PRODUCTS_CONTENT_ID,        //Идентификатор контента Продукты
        PRODUCT_RELATIONS_CONTENT_ID, //Идентификатор контента Матрица связей продуктов
        MARKETING_PRODUCT_PRODUCTS_FIELD_NAME, //Название поля маркетингового продукта, связывающего его с продуктами
        PRODUCT_SERVICES_CONTENT_ID,
        PRODUCT_ARTICLES_LIMIT,
        PRODUCT_FREEZE_FIELD_NAME,
        NOTES_CONTENT_ID,
        NOTES_TEXT_FIELD_NAME,
        PRODUCT_PDF_TEMPLATES_CONTENT_ID,
        MARKETING_PRODUCT_TYPES_TO_IGNORE_ALIAS_UNIQUENESS,
        NOTIFICATION_SENDER_CHANNELS_CONTENT_ID,
        NOTIFICATION_SENDER_FORMATTERS_CONTENT_ID,
        NOTIFICATION_SENDER_ERROR_COUNT_BERORE_WAIT,
        NOTIFICATION_SENDER_WAIT_INTERVAL_AFTER_ERRORS,
        NOTIFICATION_SENDER_TIMEOUT,
        NOTIFICATION_SENDER_PACKAGE_SIZE,
        NOTIFICATION_SENDER_CHECK_INTERVAL,
        NOTIFICATION_SENDER_AUTOPUBLISH,
        LOCALIZATION_CONTENT_ID,
        LOCALIZATION_MAP_CONTENT_ID,
        LANGUAGES_CONTENT_ID,
        TARIFF_RELATION_FIELD_NAME, //Название поля услуги, связывающего её с продуктами
        SERVICES_ON_TARIFF_CONTENT_ID, //Идентификатор контента "Услуги на тарифе"
        PRODUCTS_PARAMETERS_CONTENT_ID, //Идентификатор контента "Параметры продуктов"
        MODIFIER_DATA_OPTION_ID, //Идентификатора модификатора "Дата-Опция"
        SERVICE_FIELD_NAME, //Имя поля Услуг в контенте "Услуги на тарифе"
        FIELD_PARENT_NAME,
        //Имена полей, образующих тарифное направление
        ZONE_FIELD_NAME,
        DIRECTION_FIELD_NAME,
        BASE_PARAMETER_MODIFIERS_FIELD_NAME,
        BASE_PARAMETER_FIELD_NAME,
        PUBLISH_BUNDLE_SIZE,
        PUBLISH_DEGREE_OF_PARALLELISM,
        ELASTIC_INDEXES_CONTENT_ID,
        HIGHLOAD_API_USERS_CONTENT_ID,
        HIGHLOAD_API_METHODS_CONTENT_ID,
        HIGHLOAD_API_LIMITS_CONTENT_ID
    }
}
