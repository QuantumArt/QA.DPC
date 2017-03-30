using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public abstract class BaseCallsImpactCalculator : BaseImpactCalculator
    {
        protected BaseCallsImpactCalculator(string parameterModifierName, string linkModifierName, string linkName, bool consolidateCallGroups) : base(parameterModifierName, linkModifierName, linkName)
        {
            ConsolidateCallGroups = consolidateCallGroups;
        }

        protected bool ConsolidateCallGroups { get; private set; }
       

        public abstract IEnumerable<JToken> FilterProductParameters(JArray root, string code);

        public virtual void FilterServicesParameters(JObject[] services, string region)
        {
            foreach (var service in services)
            {
                var root = (JArray)service.SelectToken("Parameters");
                var countryParams = FilterProductParameters(root, region).ToArray();
                countryParams = AppendParents(root, countryParams);
                service["Parameters"] = new JArray((IEnumerable<JToken>)countryParams);
            }
        }

        protected virtual void ChangeGroupNamesForIcin(JToken[] countryParams)
        {
            foreach (var p in countryParams)
            {
                var group = p.SelectToken("Group");
                var icinTitle = group?.SelectToken("TitleForIcin");
                if (icinTitle != null)
                {
                    group["Title"] = icinTitle.ToString();
                }
            }
        }

        protected virtual string GenerateNewTitle(JToken p)
        {
            var newTitle = p["Title"].ToString();
            var roamingTitle = p["TitleForIcin"]?.ToString();
            var pos = newTitle.IndexOf(" (", StringComparison.InvariantCulture);
            var bpTitle = p.SelectToken("BaseParameter.Title").ToString();
            newTitle = roamingTitle ?? ((pos == -1) ? bpTitle : bpTitle + newTitle.Substring(pos));
            return newTitle;
        }

        protected void СonsolidateGroupForCalls(JToken[] inParams)
        {
            var outgoingCallsGroup =
                inParams.FirstOrDefault(n => n.SelectToken("BaseParameter.Alias")?.ToString() == "OutgoingCalls")?.SelectToken("Group");

            if (outgoingCallsGroup != null)
            {
                var callParams = inParams.Where(n => new[] {"OutgoingCalls", "IncomingCalls"}.Contains(n.SelectToken("BaseParameter.Alias")?.ToString()));
                foreach (var callParam in callParams)
                {
                    callParam["Group"] = outgoingCallsGroup;
                }
            }

        }
    }
}
