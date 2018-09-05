using Newtonsoft.Json;
using QA.Core.Models.Configuration;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    /// <summary>
    /// Схема для редактирования продукта
    /// </summary>
    public sealed class ProductSchema
    {
        /// <summary>
        /// Схема корневого DPC-контента
        /// </summary>
        public IContentSchema Content { get; set; }

        /// <summary>
        /// Схемы повторяющихся DPC-контентов, сгруппированные по имени контента
        /// </summary>
        public Dictionary<string, ContentSchema> Definitions { get; set; }
            = new Dictionary<string, ContentSchema>();
    }

    /// <summary>
    /// Схема DPC-контента или ссылка на него
    /// </summary>
    public interface IContentSchema
    {
        int ContentId { get; set; }
    }

    /// <summary>
    /// Схема DPC-контента (эквивалентен QP-контенту c неполным набором полей)
    /// </summary>
    public sealed class ContentSchema : IContentSchema
    {
        public int ContentId { get; set; }

        /// <summary>
        /// Путь к контенту в продукте в формате <c>"/FieldName/.../ExtensionContentName/.../FieldName"</c>
        /// </summary>
        public string ContentPath { get; set; }

        /// <summary>
        /// .NET-имя контента
        /// </summary>
        public string ContentName { get; set; }

        /// <summary>
        /// User-Fiendly имя контента
        /// </summary>
        public string ContentTitle { get; set; }

        /// <summary>
        /// Описание контента
        /// </summary>
        public string ContentDescription { get; set; }

        /// <summary>
        /// Название поля, которое используется для отображения контента в шапке редактора или в виде тега
        /// </summary>
        public string DisplayFieldName { get; set; }

        /// <summary>
        /// Используется только в качестве расширения
        /// </summary>
        public bool ForExtension { get; set; }

        /// <summary>
        /// Схемы полей контента по .NET-имени поля
        /// </summary>
        public Dictionary<string, FieldSchema> Fields { get; set; }
            = new Dictionary<string, FieldSchema>();

        internal ContentSchema ShallowCopy()
        {
            return (ContentSchema)MemberwiseClone();
        }
    }

    /// <summary>
    /// Ссылка на схему DPC-контента по Id. Используется только для объединенных DPC-контентов
    /// потому что у двух контентов с одинаковыми Id в дереве продукта могут быть разные наборы полей.
    /// </summary>
    public sealed class ContentSchemaIdRef : IContentSchema
    {
        public int ContentId { get; set; }
    }

    /// <summary>
    /// Ссылка на повторяющуюся схему DPC-контента по его имени. Два контента с одинаковым Id, но разными
    /// наборами полей будут иметь разные ссылки вида <code>[{ $ref: "/MyContent1" }, { $ref: "/MyContent2" }]</code>
    /// </summary>
    public sealed class ContentSchemaJsonRef : IContentSchema
    {
        [JsonIgnore]
        public int ContentId { get; set; }

        /// <summary>
        /// Ссылка на DPC-контент по имени, вида <code>{ $ref: "/MyContent2" }</code>
        /// </summary>
        [JsonProperty("$ref")]
        public string Ref { get; set; }
    }

    /// <summary>
    /// Схема поля контента
    /// </summary>
    public abstract class FieldSchema
    {
        public int FieldId { get; set; }

        /// <summary>
        /// Порядковый номер для сортировки списка полей одного котента
        /// </summary>
        public int FieldOrder { get; set; }

        /// <summary>
        /// .NET-имя поля
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// User-Fiendly имя поля
        /// </summary>
        public string FieldTitle { get; set; }

        /// <summary>
        /// Описание поля
        /// </summary>
        public string FieldDescription { get; set; }

        /// <summary>
        /// QP-тип поля 
        /// </summary>
        public FieldExactTypes FieldType { get; set; }
        
        /// <summary>
        /// Поле является обязательным
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Поле является неизеняемым
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Показывать ли поле в таблице
        /// </summary>
        public bool ViewInList { get; set; }

        /// <summary>
        /// Значение поля по-умолчанию при создании новой статьи
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Названия классов в иерархии наследования
        /// </summary>
        public List<string> ClassNames
        {
            get
            {
                var types = new List<string>();
                Type type = GetType();
                while (type != typeof(FieldSchema))
                {
                    types.Add(type.Name);
                    type = type.BaseType;
                }
                return types;
            }
        }

        /// <summary>
        /// Циклическая сылка на родительский контент. Заполняется на клиенте.
        /// </summary>
        public IContentSchema ParentContent => null;
    }

    /// <summary>
    /// Схема простого поля (не связи или расширения)
    /// </summary>
    public class PlainFieldSchema : FieldSchema
    {
    }

    public sealed class StringFieldSchema : PlainFieldSchema
    {
        /// <summary>
        /// Регулярное выражение для валидации поля
        /// </summary>
        public string RegexPattern { get; set; }
    }

    public sealed class NumericFieldSchema : PlainFieldSchema
    {
        public bool IsInteger { get; set; }
    }

    /// <summary>
    /// Схема поля-классификатора
    /// </summary>
    public sealed class ClassifierFieldSchema : PlainFieldSchema
    {
        /// <summary>
        /// Если false, то поле можно задать при создании статьи,
        /// но при редактировании оно будет неизеняемым
        /// </summary>
        public bool Changeable { get; set; }
    }

    public sealed class FileFieldSchema : PlainFieldSchema
    {
        /// <summary>
        /// Библиотека сайта
        /// </summary>
        public bool UseSiteLibrary { get; set; }
        /// <summary>
        /// URL каталога, где физически хранятся загруженные файлы
        /// </summary>
        public string FolderUrl { get; set; }
    }

    public sealed class EnumFieldSchema : PlainFieldSchema
    {
        /// <summary>
        /// Отображать как набор радио-кнопок или как комбо-бокс
        /// </summary>
        public bool ShowAsRadioButtons { get; set; }

        /// <summary>
        /// Набор значений для выбора
        /// </summary>
        public StringEnumItem[] Items { get; set; } = new StringEnumItem[0];
    }

    /// <summary>
    /// Схема поля-связи
    /// </summary>
    public abstract class RelationFieldSchema : FieldSchema
    {
        /// <summary>
        /// Схема DPC-контента связанных статей
        /// </summary>
        public IContentSchema RelatedContent { get; set; }

        /// <summary>
        /// Поведение поля связи при клонировании родительской статьи
        /// </summary>
        public CloningMode CloningMode { get; set; }

        /// <summary>
        /// Следует ли обновлять связанные статьи при рекурсивном обновлении связей
        /// </summary>
        public UpdatingMode UpdatingMode { get; set; }

        /// <summary>
        /// Является обратным полем
        /// </summary>
        public bool IsDpcBackwardField { get; set; }

        /// <summary>
        /// SQL-условие фильтрации при выборе новых значений для связи
        /// </summary>
        public string RelationCondition { get; set; }

        /// <summary>
        /// Название поелй, которы используются для отображения контента
        /// в строке таблици или в заголовке аккордеона
        /// </summary>
        public string[] DisplayFieldNames { get; set; } = new string[0];
    }

    /// <summary>
    /// Схема поля-связи, представляющая ссылку на одиночную связанную статью
    /// </summary>
    public sealed class SingleRelationFieldSchema : RelationFieldSchema
    {
        internal SingleRelationFieldSchema ShallowCopy()
        {
            return (SingleRelationFieldSchema)MemberwiseClone();
        }
    }

    /// <summary>
    /// Схема поля-связи, представляющая массив ссылок на связанные статьи
    /// </summary>
    public sealed class MultiRelationFieldSchema : RelationFieldSchema
    {
        /// <summary>
        /// .NET-имя поля, используемого для сортировки массива статей при отображении
        /// </summary>
        public string OrderByFieldName { get; set; }

        /// <summary>
        /// Максимально допустимое кол-во статей в массиве (правило валидации)
        /// </summary>
        public int? MaxDataListItemCount { get; set; }

        internal MultiRelationFieldSchema ShallowCopy()
        {
            return (MultiRelationFieldSchema)MemberwiseClone();
        }
    }

    /// <summary>
    /// Схема поля-расширения
    /// </summary>
    public sealed class ExtensionFieldSchema : FieldSchema
    {
        /// <summary>
        /// Если false, то поле можно задать при создании статьи,
        /// но при редактировании оно будет неизеняемым
        /// </summary>
        public bool Changeable { get; set; }

        /// <summary>
        /// Схемы DPC-контентов-расширений по .NET-имени контента
        /// </summary>
        public Dictionary<string, IContentSchema> ExtensionContents { get; set; }
            = new Dictionary<string, IContentSchema>();

        internal ExtensionFieldSchema ShallowCopy()
        {
            return (ExtensionFieldSchema)MemberwiseClone();
        }
    }
}
