using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using Article = QA.Core.Models.Entities.Article;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using FieldService = Quantumart.QP8.BLL.Services.API.FieldService;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace QA.Core.DPC.Loader
{
    public class TmfProductService : JsonProductService
    {
        public TmfProductService(
            IConnectionProvider connectionProvider,
            ILogger logger,
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldContextService virtualFieldContextService,
            IRegionTagReplaceService regionTagReplaceService,
            IOptions<LoaderProperties> loaderProperties,
            IHttpClientFactory factory,
            JsonProductServiceSettings settings)
            : base(connectionProvider, logger, contentService, fieldService, virtualFieldContextService, regionTagReplaceService, loaderProperties, factory, settings)
        {
        }

        protected override void AssignField(Dictionary<string, object> dict, string name, object value)
        {
            string fieldName = name.Equals(TmfProductDeserializer.TmfIdFieldName, StringComparison.OrdinalIgnoreCase)
                ? nameof(Article.Id)
                : name;

            base.AssignField(dict, fieldName, value);
        }

        protected override IProductDataSource CreateDataSource(IDictionary<string, JToken> tokensDict) =>
            new TmfProductDataSource(tokensDict);
    }
}
