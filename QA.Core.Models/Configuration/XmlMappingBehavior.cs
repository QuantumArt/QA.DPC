using System;
using Portable.Xaml;



namespace QA.Core.Models.Configuration
{
    public class XmlMappingBehavior
    {
        #region Static members
        static AttachableMemberIdentifier BehaviorName = new AttachableMemberIdentifier(typeof(XmlMappingBehavior), "Behavior");

        public static object GetBehavior(object instance)
        {
            object viewState;
            AttachablePropertyServices.TryGetProperty(instance, BehaviorName, out viewState);
            return viewState;
        }

        public static void SetBehavior(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, BehaviorName, value);
        }

        public static XmlMappingBehavior GetXmlBehavior(object instance)
        {
            return (XmlMappingBehavior)GetBehavior(instance);
        }

        public static void SetXmlBehavior(object instance, XmlMappingBehavior value)
        {
            AttachablePropertyServices.SetProperty(instance, BehaviorName, value);
        }

        #endregion

        static readonly AttachableMemberIdentifier CachePeriodName = new AttachableMemberIdentifier(typeof(XmlMappingBehavior), "CachePeriod");

        public static object GetCachePeriod(object instance)
        {
            object viewState;
            AttachablePropertyServices.TryGetProperty(instance, CachePeriodName, out viewState);
            return viewState;
        }

        public static void SetCachePeriod(object instance, object value)
        {
            if (value == null)
                AttachablePropertyServices.RemoveProperty(instance, CachePeriodName);
            else
                AttachablePropertyServices.SetProperty(instance, CachePeriodName, value);
        }

        public static TimeSpan? RetrieveCachePeriod(Content instance)
        {
            var obj = GetCachePeriod((object)instance);
            if (obj is TimeSpan?)
                return (TimeSpan?)obj;

            if (obj is TimeSpan)
                return (TimeSpan)obj;

            if (obj is string)
            {

                TimeSpan ts;
                if (TimeSpan.TryParse((string)obj, out ts))
                {
                    return ts;
                }
            }

            return null;
        }

        public ExportMode ExportMode { get; set; }
        public string ExportField { get; set; }
    }
}
