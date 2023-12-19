using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using QA.Workflow.Integration.QP;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.TaskWorker.Models;

namespace QA.Core.DPC.Workflow.Services
{
    public class WorkflowTenantWatcherHostedService : WorkflowTenantWatcher
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;
        private readonly IServiceProvider _serviceProvider;

        public WorkflowTenantWatcherHostedService(ILogger<WorkflowTenantWatcher> logger,
            ICustomerProvider customerProvider,
            IOptions<ExtendedCamundaSettings> camundaSettings,
            WorkflowTenants workflowTenants,
            IIdentityProvider identityProvider,
            IServiceProvider serviceProvider)
        : base(logger, camundaSettings.Value, workflowTenants) 
        {
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _serviceProvider = serviceProvider;
        }

        public override Task<List<string>> LoadCustomerCodes()
        {
            Customer[] customerCodes = _customerProvider.GetCustomers();

            return Task.FromResult(customerCodes.Select(c => c.CustomerCode)
               .ToList());
        }

        public override Task<bool> IsExternalWorkflowEnabled(string customerCode)
        {
            try
            {
                _identityProvider.Identity = new(customerCode);
                ISettingsService settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
                string camundaSetting = settingsService.GetSetting(SettingsTitles.EXTERNAL_WORKFLOW);

                return Task.FromResult(!string.IsNullOrWhiteSpace(camundaSetting) &&
                    bool.TryParse(camundaSetting, out bool camundaEnabled) &&
                    camundaEnabled);
            }
            finally
            {
                _identityProvider.Identity = null;
            }
        }
    }
}
