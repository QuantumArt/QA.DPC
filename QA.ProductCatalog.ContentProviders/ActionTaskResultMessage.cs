using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using Newtonsoft.Json;
using QA.Core.DPC.Resources;

namespace QA.ProductCatalog.ContentProviders
{
    public class ActionTaskResultMessage
    {
        public string ResourceName { get; set; }
        
        public string ResourceClass { get; set; }
        
        public string Message { get; set; }
        
        public object[] Parameters { get; set; }
        
        public string Extra { get; set; }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentUICulture);
        }

        public string ToString(CultureInfo ci)
        {
            var sb = new StringBuilder();
            var result = Message;
            if (!string.IsNullOrEmpty(ResourceName) && !string.IsNullOrEmpty(ResourceClass))
            {
                var rm = new ResourceManager("QA.Core.DPC.Resources." + ResourceClass, typeof(TaskStrings).Assembly);
                result = rm.GetString(ResourceName, ci) ?? ResourceName;
            }
            
            if (Parameters != null && Parameters.Any())
            {
                sb.Append(string.Format(result, Parameters));
            }
            else
            {
                sb.Append(result);
            }

            if (!string.IsNullOrEmpty(Extra))
            {
                sb.Append(": ");
                sb.Append(Extra);
            }

            return sb.ToString();
        }
        
        public static ActionTaskResultMessage FromString(string str)
        {
            ActionTaskResultMessage result = null;

            try
            {
                result = JsonConvert.DeserializeObject<ActionTaskResultMessage>(str);
            }
            catch (Exception)
            {
                result = new ActionTaskResultMessage() {Message = str};
            }

            return result;
        }
    }
}