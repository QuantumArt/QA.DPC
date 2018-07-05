using System.Linq;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.UI.Controls.EntityEditorControls
{
    public class ArticleInfo : QPControlBase
    {
        public bool ShowIcon { get; private set; }

        public bool HideEmptyFields { get; private set; }

        public ArticleInfo(IGetArticle modelObject, bool hideEmptyFields, bool showIcon)
        {
            HideEmptyFields = hideEmptyFields;
            ShowIcon = showIcon;
            CurrentItem = modelObject;

            var field = modelObject as ArticleField;
            if (field != null)
            {
                PropertyDisplay pd = new PropertyDisplay()
                {
                    Title = field.FieldDisplayName
                };

                var article = modelObject.GetItem(null);

                if (hideEmptyFields && article == null)
                {
                    return;
                }

                if (article != null)
                {
                    var valueTitleSource = article.Fields.Values.OfType<IGetFieldStringValue>().FirstOrDefault();
                    pd.Value = new ActionLink
                    {
                        CurrentItem = article,
                        ShowIcon = showIcon,
                        IconClass = "edit",
                        Title = valueTitleSource != null ? valueTitleSource.Value : null
                    };
                }

                Content = pd;
            }
        }

    }
}
