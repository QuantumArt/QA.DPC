using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    /// <summary>
    /// Объект, содержащий поля нормальной статьи или статьи-расширения
    /// </summary>
    public class ArticleObject : Dictionary<string, object>
    {
        /// <summary>
        /// Имя поля для локального неизменяемого Id статьи на клиенте.
        /// Совпадает с <see cref="Models.Entities.Article.Id"/>
        /// для статей загруженных с сервера. Является отрицательным для статей созданных на клиенте.
        /// </summary>
        internal const string _ClientId = "_ClientId";
        /// <summary>
        /// Имя поля для серверного Id статьи, полученного при сохранении в БД.
        /// Совпадает с <see cref="Models.Entities.Article.Id"/>.
        /// </summary>
        internal const string _ServerId = "_ServerId";
        /// <summary>
        /// Имя поля для .NET-названия контента статьи <see cref="Quantumart.QP8.BLL.Content.NetName" />.
        /// </summary>
        internal const string _Content = "_Content";
        /// <summary>
        /// Имя поля для даты создания или последнего изменения статьи <see cref="Models.Entities.Article.Modified"/>.
        /// </summary>
        internal const string _Modified = "_Modified";
        /// <summary>
        /// Признак того, что объект является статьей-расшиернием
        /// </summary>
        internal const string _IsExtension = "_IsExtension";
        /// <summary>
        /// Признак того, что объект не должен быть сохранен на сервере
        /// </summary>
        internal const string _IsVirtual = "_IsVirtual";
        /// <summary>
        /// Имя поля для словаря, группирующего поля расширений по имени контента-расширения.
        /// </summary>
        internal static string _Contents(string prop) => $"{prop}_Contents";
    }
    
    /// <summary>
    /// Словарь, отображающий имя контента-расширения на объект с полями статьи-расширения
    /// </summary>
    public class ExtensionFieldObject : Dictionary<string, ArticleObject>
    {
    }
}
