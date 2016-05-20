using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QA.Core.DPC.UI.Controls;
using QA.Core.DPC.UI.Controls.EntityEditorControls;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    public class EntityEditor : QPControlBase
    {
        #region Properties
        public bool DisplayAllFields { get; set; }
        public bool DisplayAllPlainFields { get; set; }

        public bool DisplaySingleFields { get; set; }

        public bool DisplayCollections { get; set; }

        public bool MergeExtensions { get; set; }

        [TypeConverter(typeof(CollectionConverter<string>))]
        public string[] ExcludeFields { get; set; }
        public bool HideEmptyFields { get; set; }
        public bool HideEmptyMultipleFields { get; set; }

        public bool HideEmptyPlainFields { get; set; }

        public bool HideEmptyArticleFields { get; set; }

        public bool ShowCollectionsHorizontal { get; set; }

        public string CollectionSeparator { get; set; }

        public bool ShowIcons { get; set; }
        public bool ShowId { get; set; }

        public QPBehavior Behavior { get; set; }

        #endregion

        public EntityEditor()
        {
            // defaults
            ExcludeFields = new string[] { };
            ShowCollectionsHorizontal = true;
            ShowIcons = true;
            CollectionSeparator = ", ";
            MergeExtensions = true;
        }


        #region Public Methods
        public IEnumerable<PlainArticleField> GetAllPlainFields()
        {
            if (CurrentItem != null && (CurrentItem is Article))
            {
                foreach (var item in GetFilteredFields((Article)CurrentItem)
                    .OfType<PlainArticleField>())
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<IModelObject> GetAllFields(IModelObject modelIObject)
        {
            if (modelIObject != null && (modelIObject is Article))
            {
                var filteredFields = ExtractFields(modelIObject);

                foreach (var item in filteredFields)
                {
                    yield return item;

                    if (item is ExtensionArticleField && MergeExtensions)
                    {
                        var extension = ((ExtensionArticleField)item).GetItem(null);
                        if (extension == null)
                            continue;

                        foreach (var nestedField in ExtractFields(extension))
                        {
                            yield return nestedField;
                        }
                    }
                }
            }
        }

        public IEnumerable<UIElement> GetFieldInfo(IModelObject modelObject)
        {
            var fields = GetAllFields(modelObject);

            var article = modelObject as Article;

            if (ShowId && article != null)
            {
                yield return new PropertyDisplay { Title = "Id", Value = article.Id };
            }
            foreach (var field in fields.OfType<ArticleField>())
            {
                if (field is IGetFieldStringValue)
                {
                    var control = new DisplayField()
                    {
                        Value = field as PlainArticleField,
                        HideEmptyPlainFields = HideEmptyFields || HideEmptyPlainFields
                    };

                    yield return control;
                }
                else if (field is IGetArticle)
                {
                    yield return new ArticleInfo((IGetArticle)field,
                        hideEmptyFields: HideEmptyFields || HideEmptyArticleFields,
                        showIcon: ShowIcons);
                }
                else if (field is IGetArticles)
                {
                    yield return new ArticleCollection(modelObject, (IGetArticles)field,
                        hideEmptyMultipleFields: HideEmptyFields || HideEmptyMultipleFields,
                        showIcon: ShowIcons,
                        isHorizontal: ShowCollectionsHorizontal,
                        separator: CollectionSeparator);
                }
            }
        }
        #endregion

        #region Private Methods
        private IEnumerable<ArticleField> ExtractFields(IModelObject modelIObject)
        {
            var filteredFields = GetFilteredFields(modelIObject);

            if (!DisplayCollections && !DisplayAllFields)
                filteredFields = filteredFields.Where(x => !(x is IGetArticles));

            if (!DisplaySingleFields && !DisplayAllFields)
                filteredFields = filteredFields.Where(x => !(x is IGetArticle));

            if (!DisplayAllPlainFields && !DisplayAllFields)
                filteredFields = filteredFields.Where(x => !(x is PlainArticleField));

            return filteredFields;
        }
        private IEnumerable<ArticleField> GetFilteredFields(IModelObject modelIObject)
        {
            return ((Article)modelIObject).Fields
                .Values
                .Where(x => !ExcludeFields.Contains(x.FieldName));
        }

        #endregion
    }
}
