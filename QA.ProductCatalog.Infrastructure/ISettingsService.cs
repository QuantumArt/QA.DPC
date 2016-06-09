using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		PRODUCT_TYPES_FIELD_NAME,	//Название поле продукта, определяющего тип продукта
		REGIONAL_TAGS_CONTENT_ID,    //Идентификатор контента Региональные теги
		REGIONAL_TAGS_VALUES_CONTENT_ID,    //Идентификатор контента Значения региональных тегов
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
        LOCALIZATION_CONTENT_ID,
        LANGUAGES_CONTENT_ID
    }
}
