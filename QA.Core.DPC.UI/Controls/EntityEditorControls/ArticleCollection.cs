using System.Linq;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls.EntityEditorControls
{
    public class ArticleCollection : QPControlBase
    {
        public string Separator { get; set; }
        public bool HideEmptyMultipleFields { get; private set; }
        public bool ShowIcon { get; private set; }
        public bool IsHorizontal { get; private set; }
        public ArticleCollection(IModelObject parent,
            IGetArticles modelObject,
            bool hideEmptyMultipleFields,
            bool showIcon,
            bool isHorizontal,
            string separator = ", ")
        {
            HideEmptyMultipleFields = hideEmptyMultipleFields;
            ShowIcon = showIcon;
            IsHorizontal = isHorizontal;
            CurrentItem = parent;

            var field = modelObject as ArticleField;
            if (field != null)
            {
                PropertyDisplay pd = new PropertyDisplay()
                {
                    Title = field.FieldDisplayName
                };

                var articles = modelObject.GetArticles(null).ToArray();

                UIItemsControl stackPanel = new StackPanel { IsHorizontal = isHorizontal }; ;
                UIItemsControl container;

                if (isHorizontal)
                {
                    container = new StackPanel();
                    container.AddChild(stackPanel);
                }
                else
                {
                    container = stackPanel;
                }

                if (hideEmptyMultipleFields && !articles.Any())
                {
                    return;
                }

                if (showIcon)
                {
                    container.PrependChild(new ActionLink
                    {
                        Title = "ред.",
                        CurrentItem = parent,
                        ShowIcon = true,
                        IconClass = "edit"
                    });
                }

                for (int i = 0; i < articles.Length; i++)
                {
                    var article = articles[i];

                    if (article == null)
                        continue;

                    var valueTitleSource = article.Fields.Values.OfType<IGetFieldStringValue>().FirstOrDefault();

                    stackPanel.AddChild(new ActionLink
                    {
                        CurrentItem = article,
                        Title = valueTitleSource != null ? valueTitleSource.Value : null
                    });

                    if (isHorizontal && !string.IsNullOrEmpty(separator) && i < articles.Length - 1)
                        stackPanel.AddChild(new Label { Title = separator });
                }

                pd.Value = container;

                Content = pd;
            }
        }
    }
}
