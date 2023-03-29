using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Workflow.Models;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;
using QA.Workflow.Extensions;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;

namespace QA.Core.DPC.Workflow.ExternalTasks
{
    public class CheckProductOnReferenceFront : IExternalTaskHandler
    {
        private readonly IIdentityProvider _identityProvider;
        private readonly Func<bool, string, IMonitoringRepository> _monitoringRepository;

        public CheckProductOnReferenceFront(IIdentityProvider identityProvider, Func<bool, string, IMonitoringRepository> monitoringRepository)
        {
            _identityProvider = identityProvider;
            _monitoringRepository = monitoringRepository;
        }

        public Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
        {
            try
            {
                int productId = processInstance.GetVariableByName<int>(InternalSettings.ProductIdParameterName);
                bool isLive = processInstance.GetVariableByName<bool>(InternalSettings.IsLiveParameterName);
                string culture = processInstance.GetVariableByName<string>(InternalSettings.CultureParameterName);
                string retryCountVariableName = processInstance.GetVariableByName<string>(InternalSettings.RetryCountParameterName);
                string resultVariableName = processInstance.GetVariableByName<string>(InternalSettings.ResultParameterName);
                string publishDateVariableName = processInstance.GetVariableByName<string>(InternalSettings.PublishDateParameterName);

                DateTime publishDate = processInstance.GetVariableByName<DateTime>(publishDateVariableName);
                int retryCount = processInstance.GetVariableByNameOrDefault<int>(retryCountVariableName);

                _identityProvider.Identity = new(processInstance.TenantId);

                IMonitoringRepository repository = _monitoringRepository(isLive, culture);
                ProductInfo[] products = repository.GetByIds(new[] { productId });

                ProductInfo product = products.OrderByDescending(p => p.Updated).FirstOrDefault();
                bool productUpdated = false;

                if (product != default)
                {
                    productUpdated = product.Updated >= publishDate;
                }

                retryCount = productUpdated ? 0 : retryCount + 1;

                return Task.FromResult<Dictionary<string, object>>(new()
                {
                    { retryCountVariableName, retryCount },
                    { resultVariableName, productUpdated }
                });
            }
            finally
            {
                _identityProvider.Identity = null;
            }

        }
    }
}
