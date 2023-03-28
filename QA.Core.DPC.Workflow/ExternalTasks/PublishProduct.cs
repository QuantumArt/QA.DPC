using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;

namespace QA.Core.DPC.Workflow.ExternalTasks;

public class PublishProduct : IExternalTaskHandler
{
    private readonly Func<IProductAPIService> _databaseProductServiceFactory;
    private readonly IIdentityProvider _identityProvider;

    private readonly Dictionary<string, string> _actionParameters;

    public PublishProduct(Func<IProductAPIService> databaseProductServiceFactory,
        IIdentityProvider identityProvider)
    {
        _databaseProductServiceFactory = databaseProductServiceFactory;
        _identityProvider = identityProvider;

        _actionParameters = new() { { "IgnoredStatus", "Created" } };
    }
    
    public async Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
    {
        if (!processInstance.Variables.TryGetValue("ContentItemId", out object? itemId))
        {
            throw new InvalidOperationException("Unable to find Item id in task variables.");
        }

        if (!int.TryParse((string)itemId, out int item))
        {
            throw new InvalidCastException($"Failed to parse {itemId} as int.");
        }

        _identityProvider.Identity = new(processInstance.TenantId);
        _databaseProductServiceFactory().CustomAction("PublishAction", item, _actionParameters);

        return new();
    }
}
