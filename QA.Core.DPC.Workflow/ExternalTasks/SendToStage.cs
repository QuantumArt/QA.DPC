using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;
using QA.Workflow.Extensions;
using QA.Core.DPC.Workflow.Models;

namespace QA.Core.DPC.Workflow.ExternalTasks;

public class SendToStage : IExternalTaskHandler
{
    private readonly Func<IProductAPIService> _databaseProductServiceFactory;
    private readonly IIdentityProvider _identityProvider;

    private readonly Dictionary<string, string> _actionParameters;

    public SendToStage(Func<IProductAPIService> databaseProductServiceFactory, IIdentityProvider identityProvider)
    {
        _databaseProductServiceFactory = databaseProductServiceFactory;
        _identityProvider = identityProvider;

        _actionParameters = new()
        {
            { "IgnoredStatus", "Created" },
            { "skipPublishing", "True" },
            { "skipLive", "True" }
        };
    }

    public Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
    {
        string resultVariable = processInstance.GetVariableByName<string>(InternalSettings.PublishDateVariable);
        int item = processInstance.GetVariableByName<int>(InternalSettings.ProductId);
        DateTime processDate = DateTime.Now;

        _identityProvider.Identity = new(processInstance.TenantId);
        _databaseProductServiceFactory().CustomAction("SendProductAction", item, _actionParameters);

        return Task.FromResult<Dictionary<string, object>>(new() { { resultVariable, processDate } });
    }
}
