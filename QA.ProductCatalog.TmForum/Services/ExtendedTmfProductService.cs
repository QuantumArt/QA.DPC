using Microsoft.Extensions.Options;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;
using Quantumart.QP8.BLL.Services.API;

namespace QA.ProductCatalog.TmForum.Services;

public class ExtendedTmfProductService : TmfProductService
{
    public ExtendedTmfProductService(IConnectionProvider connectionProvider, ILogger logger, ContentService contentService,
        FieldService fieldService,
        VirtualFieldContextService virtualFieldContextService,
        IRegionTagReplaceService regionTagReplaceService,
        IOptions<LoaderProperties> loaderProperties,
        IHttpClientFactory factory,
        JsonProductServiceSettings settings,
        ISettingsService settingsService,
        IProductAddressProvider productAddressProvider) : base(connectionProvider, logger, contentService,
        fieldService,
        virtualFieldContextService,
        regionTagReplaceService,
        loaderProperties,
        factory,
        settings,
        settingsService,
        productAddressProvider)
    {
    }

    public override Dictionary<string, object> ConvertArticle(Article article, IArticleFilter filter)
    {
        Dictionary<string, object> converted = base.ConvertArticle(article, filter);

        if (converted is null)
        {
            return null;
        }
        
        converted[InternalTmfSettings.InternalIdFieldName] = article.Id;
        converted[InternalTmfSettings.InternalTypeFieldName] = article.ContentDisplayName;

        return converted;
    }
}
