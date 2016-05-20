using QA.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.Extensions
{
    public static class FieldExtensions
    {
        public static Article AddField<TField>(this Article article, TField field)
            where TField : ArticleField
        {
            article.Fields.Add(field.FieldName, field);
            return article;
        }

        public static Article SetField<TField>(this Article article, TField field)
            where TField : ArticleField
        {
            article.Fields[field.FieldName] = field;
            return article;
        }

        public static Article AddField<TField>(this Article article, string name, string value, string fieldDisplayName = "", object nativeValue = null)
            where TField : PlainArticleField
        {
            var field = Activator.CreateInstance<TField>();
            field.FieldName = name;
            field.Value = value;
            field.NativeValue = nativeValue;
            field.FieldDisplayName = fieldDisplayName;
            article.Fields.Add(name, field);
            return article;
        }

        public static Article AddArticle(this Article article, string fieldName, Article child, string fieldDisplayName = "")
        {
            article.AddField(new SingleArticleField() { FieldName = fieldName, Item = child });
            return article;
        }

        internal static Article AddPlainField(this Article article, string name, string value, string fieldDisplayName = "", object nativeValue = null)
        {
            return article.AddField<PlainArticleField>(name, value, fieldDisplayName, nativeValue);
        }

        public static Article AddPlainField(this Article article, string name, object nativeValue, string fieldDisplayName = null)
        {
            return article.AddField<PlainArticleField>(name, nativeValue?.ToString(), fieldDisplayName ?? name, nativeValue);
        }


        public static Article AddField<TField>(this Article article, string name, Action<TField> modify = null, string fieldDisplayName = "")
            where TField : MultiArticleField
        {
            var field = Activator.CreateInstance<TField>();
            field.FieldName = name;
            field.FieldDisplayName = fieldDisplayName;
            article.Fields.Add(name, field);
            if (modify != null)
            {
                modify(field);
            }
            return article;
        }


        public static MultiArticleField AddArticle(this MultiArticleField field, Article child, string fieldDisplayName = "")
        {
            field.Items.Add(child.Id, child);
            return field;
        }

        public static TResult As<TResult>(this IModelObject m)
            where TResult : class, IModelObject
        {
            return m as TResult;
        }
    }
}
