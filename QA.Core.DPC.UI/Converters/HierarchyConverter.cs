using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models;

namespace QA.Core.DPC.UI.Converters
{
    public class HierarchyConverter : IValueConverter
    {
        public string ParentFieldName { get; set; }

        public bool ReverseOrder { get; set; }
        public bool ExcludeSelf { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var rootArticle = value as Article;

            var articlesList = new List<Article>();

            FillListFromHierarchy(articlesList, rootArticle, ParentFieldName, ExcludeSelf);

            if (ExcludeSelf)
            {
                articlesList.Remove(rootArticle);
            }

            if (ReverseOrder)
                articlesList.Reverse();

            return articlesList;
        }

        private static void FillListFromHierarchy(List<Article> list, Article article, string fieldName, bool excludeSelf)
        {
            if (article == null)
                return;

            list.Add(article);

            var field = article.Fields.Values.SingleOrDefault(x => x.FieldName == fieldName) as SingleArticleField;

            if (field != null)
                FillListFromHierarchy(list, field.Item, fieldName, excludeSelf);
        }
    }
}
