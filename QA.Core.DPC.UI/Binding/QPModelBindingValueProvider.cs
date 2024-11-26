using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using QA.Core.Extensions;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using NLog.Fluent;
using NLog;
using ILogger = NLog.ILogger;

namespace QA.Core.DPC.UI
{
    public class QPModelBindingValueProvider : ReflectedBindingValueProvider, IBindingValueProvider
    {
        static readonly string[] Tokens = new string[] { "/" };

        public static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static bool ThrowOnErrors = false;
        #region IBindingValueProvider Members

        object IBindingValueProvider.GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source)
        {
            
            try
            {
                if (be.Log)
                {
                    _logger.Debug("Requested binding for {owner}.{name} ({propertyType}) expression: '{expression}'",
                        prop.OwnerType, prop.Name, prop.PropertyType, be.Expression);
                    _logger.Debug("Source: " + source);
                }

                var value = ProvideValueInternal(prop, be, source);
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                _logger.ForWarnEvent().Exception(ex)
                    .Message("ModelBindingError for {owner}.{name} ({propertyType}) expression: '{expression}'", 
                        prop.OwnerType, prop.Name, prop.PropertyType, be.Expression)
                    .Log();

                if (ThrowOnErrors)
                {
                    throw;
                }

                return null;
            }
        }

        private object ProvideValueInternal(DependencyProperty prop, BindingExression be, IDataContextProvider source)
        {

            var ctx = source.GetDataContext(be);

            if (be.Log)
            {
                _logger.Debug("ctx: " + ctx); 
            }

            if (!string.IsNullOrEmpty(be.Expression) && ShouldHook(be, ctx))
            {
                if (be.Log) _logger.Debug("!string.IsNullOrEmpty(be.Expression) && ShouldHook(be, ctx)");

                var context = ctx as IModelObject;

                if (be.Log) _logger.Debug("context as IModelObject: " + context);

                var segments = GetSegments(be);

                IModelObject current = context;

                if (be.Log) _logger.Debug("segments: " + string.Join("->", segments));

                foreach (var segment in segments)
                {
                    object value = null;
                    if (segment.PathType == PathType.Traversing)
                    {
                        string traversingSegment;
                        if (segment.Name.Contains("."))
                        {
                            var array = segment.Name.SplitString('.').ToList();

                            traversingSegment = array[0];
                            array.RemoveAt(0);

                            current = GetNext(current, traversingSegment);
                            value = current;

                            if (value != null)
                            {
                                value = GetHierarchicalMemberInvocationValue(prop, be, value, string.Join(".", array));
                            }

                            if (value == null)
                            {
                                if (be.Log) _logger.Debug("return null");
                                return ConvertToPropertyType(value, prop, be, source); 
                            }


                            if (value is IModelObject)
                            {
                                current = (IModelObject)value;
                            }
                            else if (segment.Latest)
                            {

                                return ConvertToPropertyType(value, prop, be, source);
                            }
                        }
                        else
                        {
                            traversingSegment = segment.Name;
                            current = GetNext(current, traversingSegment);
                        }
                    }

                    if (segment.Latest)
                    {
                        if (be.Log) _logger.Debug("latest {name} {propertyType} {current}", segment.Name, prop.PropertyType, current);

                        if (prop.PropertyType == typeof(string))
                        {
                            if (current is PlainArticleField)
                            {
                                return ApplyConverter(prop, be, ((PlainArticleField)current).Value, source);
                            }

                            if (current is SingleArticleField)
                            {
                                return ApplyConverter(prop, be, ((SingleArticleField)current).Item, source);
                            }

                            return ApplyConverter(prop, be, Convert.ToString(current), source);
                        }
                    }
                }

                return ApplyConverter(prop, be, current, source);
            }

            return ConvertToPropertyType(base.GetValue(prop, be, source), prop, be, source);
        }

        private static object ConvertToPropertyType(object value, DependencyProperty dp, BindingExression be, IDataContextProvider source )
        {
            if (be.Log)
            {
                _logger.Debug("value: " +  value);
            }

            if (be.Converter != null)
            {
                return ApplyConverter(dp, be, value, source);
            }
            if (value == null || (value as string) == string.Empty || value == DependencyProperty.UnsetValue)
            {
                return dp.GetDefaultValue();
            }

	        var converter = TypeDescriptor.GetConverter(value.GetType());

            if (dp.PropertyType is IEnumerable<object> && value is MultiArticleField)
            {
                return ((MultiArticleField)value).Items.Values;
            }

            if (dp.PropertyType.IsInstanceOfType(value))
                return value;

            if (dp.PropertyType == typeof(object))
                return value;

            return converter.ConvertTo(value, dp.PropertyType);

        }

        private static IModelObject GetNext(IModelObject context, string segment)
        {
            if (context is IGetArticleField)
            {
                var article = (IGetArticleField)context;

                var field = article.GetField(segment);
                return field;
            }

            return null;
        }

        private bool ShouldHook(BindingExression be, object dataContext)
        {
            var ctx = dataContext as IModelObject;

            if ((ctx is IModelObject || ctx is IEnumerable<IModelObject>) && CheckModelExpression(be))
            {
                return true;
            }

            return false;
        }

        private static Queue<PathSegment> GetSegments(BindingExression be)
        {
            Queue<PathSegment> segments = new Queue<PathSegment>();
            var items = be.Expression.SplitString('/').ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];


                if (i == items.Length - 1)
                {
                    segments.Enqueue(new PathSegment(item, PathType.Traversing, true));
                }
                else
                {
                    segments.Enqueue(new PathSegment(item, PathType.Traversing, false));
                }
            }

            return segments;
        }

        private bool CheckModelExpression(BindingExression be)
        {
            return Tokens.Any(x => be.Expression.Contains(x));
        }
        #endregion
    }
}
