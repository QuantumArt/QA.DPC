using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.Extensions;
using QA.Core.Models.Comparers;
using System.ComponentModel;

namespace QA.Core.DPC.UI
{
    /// <summary>
    /// Базовый класс для контролов, которые имеют биндинг на коллекции.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    [ContentProperty("ItemTemplate")]
    public abstract class ItemListControl<TItem> : UIElement, ITitled<string>
        where TItem : UIElement
    {
        static ItemListControl() { }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Сортировка
        /// </summary>
        [TypeConverter(typeof(CollectionConverter<string>))]
        public string[] OrderBy { get; set; }

        public IEnumerable<object> Items
        {
            get
            {
                var v = (IEnumerable<object>)GetValue(ItemsProperty);
                return v;
            }
            set { SetValue(ItemsProperty, value); }
        }

        public IEnumerable<TItem> GetChildren()
        {
            return GetChildren(Items);
        }

        public virtual IEnumerable<TItem> GetChildren(IEnumerable<object> items)
        {
            if (items != null)
            {
                return new MutableCollection<TItem, object>(ApplyFilter(items), GetTemplate());
            }
            else
            {
                return new TItem[] { };
            }
        }

        protected abstract TItem GetTemplate();

        protected virtual IEnumerable<object> ApplyFilter(IEnumerable<object> items)
        {
            if (OrderBy == null || OrderBy.Length == 0)
                return items;

            var sorted = items.OrderBy(x => GetFilterValue(OrderBy[0].Trim('/'), x), ModelObjectComparer.Default);

            for (int i = 1; i < OrderBy.Length; i++)
            {
                var index = i;
                var orderBy = OrderBy[index].Trim('/');
                sorted = sorted.ThenBy(x =>
                {
                    return GetFilterValue(orderBy, x);
                }, ModelObjectComparer.Default);
            }
            items = sorted;
            return items;
        }



        protected static object GetFilterValue(string filter, object x)
        {
            var article = x as Article;
            if (article != null)
            {
                var field = article.GetField(filter.SplitString('/').FirstOrDefault());
                if (field is PlainArticleField)
                {
                    return (field as PlainArticleField).NativeValue;
                }
                else if (field is SingleArticleField)
                {
                    var f = ((SingleArticleField)field);
                    if (f.Item != null)
                    {
                        if (filter.Contains("/"))
                        {
                            return GetFilterValue(filter.Substring(filter.IndexOf('/') + 1), f.Item);
                        }
                        else
                        {
                            return f.Item;
                        }
                    }
                }
                else if (field is MultiArticleField)
                {
                    var f = ((MultiArticleField)field);
                    return f.Items.Values;
                }
            }

            return "";
        }


        // Using a DependencyProperty as the backing store for CurrentItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<object>), typeof(ItemListControl<TItem>), inherit: true, log: false);


        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ItemListControl<TItem>));

    }
}
