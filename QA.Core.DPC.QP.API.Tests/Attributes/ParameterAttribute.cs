using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using QA.Core.DPC.QP.API.Tests.Providers;
using System.Collections;

namespace QA.Core.DPC.QP.API.Tests.Attributes
{
    public class ParameterAttribute : Attribute, IParameterDataSource
    {
        private readonly string _parameter;
        private JArray _configuration;

        public ParameterAttribute(string parameter)
        {
            _parameter = parameter;
            _configuration = ProviderBase.GetJson<JArray>($"QA.Core.DPC.QP.API.Tests.configuration.json");
        }

        public IEnumerable GetData(IParameterInfo parameter)
        {
            foreach (var item in _configuration)
            {
                yield return item.Value<string>(_parameter);
            }
        }
    }
}
