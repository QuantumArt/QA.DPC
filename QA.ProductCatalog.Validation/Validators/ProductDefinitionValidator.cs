using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using QA.Core.DPC.Formatters.Services;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
    public class ProductDefinitionValidator : IRemoteValidator2, IRemoteValidator
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
                result.Messages.Add("Не найдено поле " + FieldXmlDefinition);
            }

            var xaml = context.ProvideValueExact<string>(xmlDefinition);
            if (!string.IsNullOrWhiteSpace(xaml))
            {
                Content definition = null;
                try
                {
                    definition = (Content)XamlServices.Load(xaml);
                }
                catch (Exception ex)
                {
                    result.Messages.Add($"Текст не является валидным Xaml-документом валидатора. Ошибка: {ex.Message}");
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
                                var json = reader.ReadToEnd();
                            }
                            catch (Exception ex)
                            {
                                result.Messages.Add($"Текст не является валидным Xaml-документом валидатора. Ошибка: {ex.Message}");
                                return result;
                            }
                        }
                    }
                }
            }

            return result;
        }

        [Obsolete]
        public void Validate(RemoteValidationContext context, ref ValidationContext result)
        {
            var adapter = new RemoteValidationResult();

            Validate(context, adapter);

            result.Messages = adapter.Messages;
            result.Result = adapter.Result;
        }
    }
}