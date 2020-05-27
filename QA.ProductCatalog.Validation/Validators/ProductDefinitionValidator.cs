using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
#if NETSTANDARD
using Portable.Xaml;
#else
using System.Xaml;
#endif
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Resources;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.ContentProviders;
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
                var message = new ActionTaskResultMessage()
                {
                    ResourceClass = ValidationHelper.ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.FieldNotFound),
                    Parameters = new object[] {FieldXmlDefinition}
                };
                result.Messages.Add(ValidationHelper.ToString(context, message));
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
                    var message = new ActionTaskResultMessage()
                    {
                        ResourceClass = ValidationHelper.ResourceClass,
                        ResourceName = nameof(RemoteValidationMessages.NotValidXamlDefinition),
                        Parameters = new object[] { ex.Message }
                    };
                    result.Messages.Add(ValidationHelper.ToString(context, message));
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
                                var message = new ActionTaskResultMessage()
                                {
                                    ResourceClass = ValidationHelper.ResourceClass,
                                    ResourceName = nameof(RemoteValidationMessages.JsonDefinitionError),
                                    Parameters = new object[] { ex.Message }
                                };
                                result.Messages.Add(ValidationHelper.ToString(context, message));
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