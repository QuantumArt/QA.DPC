using System;
using System.IO;
using System.Linq;
#if NETSTANDARD
using Portable.Xaml;
#else
using System.Xaml;
#endif
using QA.Core.DPC.Formatters.Services;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
    public class ProductDefinitionValidator : IRemoteValidator2
    {
        // All fields: Title XmlDefinition ApplyToTypes Content JsonDefinition
        private const string FieldXmlDefinition = "XmlDefinition";
        private const string FieldJsonDefinition = "JsonDefinition";
        private readonly JsonDefinitionSchemaFormatter _formatter;


        public ProductDefinitionValidator(JsonDefinitionSchemaFormatter formatter)
        {
            _formatter = formatter;
        }
        public RemoteValidationResult Validate(RemoteValidationContext context, RemoteValidationResult result)
        {
            var xmlDefinition = context.Definitions.FirstOrDefault(x => x.Alias == FieldXmlDefinition);
            if (xmlDefinition == null)
            {
                result.Messages.Add("Field not found " + FieldXmlDefinition);
            }

            var xaml = context.ProvideValueExact<string>(xmlDefinition);
            if (!string.IsNullOrWhiteSpace(xaml))
            {
                Content definition;
                try
                {
                    definition = (Content)XamlServices.Parse(xaml);
                }
                catch (Exception ex)
                {
                    result.Messages.Add($"Text that have been received is not a valid XAML product definition. Error: {ex.Message}");
                    return result;
                }

                var jsonDefinition = context.Definitions.FirstOrDefault(x => x.Alias == FieldJsonDefinition);
                if (jsonDefinition != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            try
                            {
                                _formatter.Write(stream, definition);
                                stream.Position = 0;                                
                                context.SetValue(result, jsonDefinition, reader.ReadToEnd());
                            }
                            catch (Exception ex)
                            {
                                result.Messages.Add($"An error occurs while receiving JSON-definition. Error: {ex.Message}");
                                return result;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}