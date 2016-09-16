using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using QA.Configuration;
using Xunit.Sdk;

namespace QA.Core.DPC.Loader.Tests.Extensions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class XamlDataAttribute : DataAttribute
    {
        private readonly string _fileName;

        private readonly string _expression;

        public XamlDataAttribute(string fileName)
        {
            _fileName = fileName;
        }

        public XamlDataAttribute(string fileName, string expression)
        {
            _fileName = fileName;
            _expression = expression;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            var result = new List<object> { XamlConfigurationParser.CreateFrom(File.ReadAllText(GetFullFilename(_fileName))) };
            if (!string.IsNullOrWhiteSpace(_expression))
            {
                result.Insert(0, _expression);
            }

            return new[]
            {
                result.ToArray()
            };
        }

        private static string GetFullFilename(string filename)
        {
            var executable = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var filePath = Path.Combine(Path.GetDirectoryName(executable) ?? string.Empty, filename);
            return Path.GetFullPath(filePath);
        }
    }
}
