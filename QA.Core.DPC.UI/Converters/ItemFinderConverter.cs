using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Converters
{
    public class ItemFinderConverter : DependencyObject, IValueConverter
    {
        public object Parameter
        {
            get { return GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        public string FieldNameToFilterBy { get; set; }

        public string SourceFieldNameToFilterBy { get; set; }

        public ItemFinderMultiplicity Multiplicity { get; set; }

        static ItemFinderConverter()
        {
            ParameterProperty = DependencyProperty.Register("Parameter", typeof(object), typeof(ItemFinderConverter));
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var param = Parameter as IEnumerable<IModelObject>;
            if (param == null)
                return null;

            IEnumerable<Article> items = param.OfType<Article>();

            items = items.Concat(param.OfType<IGetArticle>().Select(x => x.GetItem(null)));
            items = items.Concat(param.OfType<IGetArticles>().SelectMany(x => x.GetArticles(null)));

            if (value is SingleArticleField)
                value = ((SingleArticleField)value).Item;

            object valueToCompare = value == null
                ? string.Empty
                : value is Article ? ((Article)value).Id : (object)value.ToString();

            IEnumerable<Article> result;

            if (string.Equals("Id", FieldNameToFilterBy, StringComparison.InvariantCultureIgnoreCase))
            {
                result = items.Where(x => object.Equals(x.Id, valueToCompare));
            }
            else
            {

                result = items.Where(
                    x => x.Fields.Any(y => (y.Value.FieldName == FieldNameToFilterBy
                                        && ((SingleArticleField)y.Value) != null
                                        && ((SingleArticleField)y.Value).Item != null
                                    && (valueToCompare.Equals(y.Value is SingleArticleField ? ((SingleArticleField)y.Value).Item.Id : (object)y.Value.ToString())))
                            ));
            }
            switch (Multiplicity)
            {
                case ItemFinderMultiplicity.Collection: return result.ToArray();
                case ItemFinderMultiplicity.SingleResult: return result.FirstOrDefault();
                case ItemFinderMultiplicity.Scalar: var r = result.FirstOrDefault();
                    return r != null ? r.ToString() : (string)null;
                default:
                    return null;
            }
        }

        public static readonly DependencyProperty ParameterProperty;
    }
}
