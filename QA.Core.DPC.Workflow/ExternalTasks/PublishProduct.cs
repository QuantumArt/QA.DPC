using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Workflow.Models;
using QA.ProductCatalog.Infrastructure;
using QA.Workflow.Extensions;
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
    
    public Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
    {
        string resultVariable = processInstance.GetVariableByName<string>(InternalSettings.PublishDateVariable);
        int item = processInstance.GetVariableByName<int>(InternalSettings.ProductId);
        DateTime processDate = DateTime.Now;

        _identityProvider.Identity = new(processInstance.TenantId);
        _databaseProductServiceFactory().CustomAction("PublishAction", item, _actionParameters);

        return Task.FromResult<Dictionary<string, object>>(new() { { resultVariable, processDate } });
    }
}
