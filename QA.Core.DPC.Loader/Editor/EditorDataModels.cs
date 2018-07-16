using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public class ArticleObject : Dictionary<string, object>
    {
        /// <summary>
        /// Имя поля для локального неизменяемого Id статьи на клиенте.
        /// Совпадает с <see cref="Models.Entities.Article.Id"/>
        /// для статей загруженных с сервера. Является отрицательным для статей созданных на клиенте.
        /// </summary>
        internal const string _ClientId = "_ClientId";
        /// <summary>
        /// Имя поля для глобального Id статьи, полученного при сохранении в БД.
        /// Совпадает с <see cref="Models.Entities.Article.Id"/>.
        /// </summary>
        internal const string _ServerId = "_ServerId";
        /// <summary>
        /// Имя поля для .NET-названия контента статьи <see cref="Quantumart.QP8.BLL.Content.NetName" />.
        /// </summary>
        internal const string _ContentName = "_ContentName";
        /// <summary>
        /// Имя поля для даты создания или последнего изменения статьи <see cref="Models.Entities.Article.Modified"/>.
        /// </summary>
        internal const string _Modified = "_Modified";
        /// <summary>
        /// Имя поля для словаря, группирующего поля расширений по имени контента-расширения.
        /// </summary>
        internal static string _Contents(string prop) => $"{prop}_Contents";
    }

    public class FileFieldObject
    {
        public string Name { get; set; }
        public string AbsoluteUrl { get; set; }
    }

    public class ExtensionFieldObject : Dictionary<string, ArticleObject>
    {
    }
}
