using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Workflow.Models;
using QA.ProductCatalog.ContentProviders;
using QA.Workflow.TaskWorker;
using QA.Workflow.TaskWorker.Models;

namespace QA.Core.DPC.Workflow.Services
{
    public class WorkflowTenantWatcherHostedService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationToken;
        private readonly ILogger<ExternalTaskHostedService> _logger;
        private readonly ICustomerProvider _customerProvider;
        private readonly ExtendedCamundaSettings _camundaSettings;
        private readonly WorkflowTenants _workflowTenants;
        private readonly IIdentityProvider _identityProvider;
        private readonly IServiceProvider _serviceProvider;
        private Task _executionTask;

        public WorkflowTenantWatcherHostedService(ILogger<ExternalTaskHostedService> logger,
            ICustomerProvider customerProvider,
            IOptions<ExtendedCamundaSettings> camundaSettings,
            WorkflowTenants workflowTenants,
            IIdentityProvider identityProvider,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _cancellationToken = new();
            _customerProvider = customerProvider;
            _camundaSettings = camundaSettings.Value;
            _workflowTenants = workflowTenants;
            _identityProvider = identityProvider;
            _serviceProvider = serviceProvider;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executionTask = Watch(_cancellationToken.Token);

            _logger.LogInformation("Workflow external task worker started.");
            return _executionTask.IsCompleted ? _executionTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executionTask == null)
            {
                return;
            }

            try
            {
                _cancellationToken.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executionTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private async Task Watch(CancellationToken cancellationToken)
        {
            // Wait some time for customer_codes registration
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Customer[]? customers = _customerProvider.GetCustomers();

                    foreach (Customer customer in customers)
                    {
                        try
                        {
                            _identityProvider.Identity = new(customer.CustomerCode);
                            ISettingsService settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
                            string? camundaSetting = settingsService.GetSetting(SettingsTitles.EXTERNAL_WORKFLOW);

                            if (string.IsNullOrWhiteSpace(camundaSetting))
                            {
                                ResolveTenant(customer.CustomerCode, TenantAction.Remove);
                                continue;
                            }

                            if (!bool.TryParse(camundaSetting, out bool camundaEnabled) || !camundaEnabled)
                            {
                                ResolveTenant(customer.CustomerCode, TenantAction.Remove);
                                continue;
                            }

                            ResolveTenant(customer.CustomerCode, TenantAction.Add);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while reading settings for tenant {tenant}", customer.CustomerCode);
                        }
                        finally
                        {
                            _identityProvider.Identity = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while loading tenants for workflow from config.");
                }
                finally
                {
                    _identityProvider.Identity = null;
                    await Task.Delay(_camundaSettings.TenantWatcherWaitTime, cancellationToken);
                }
            }
        }

        private void ResolveTenant(string tenant, TenantAction action)
        {
            switch (action)
            {
                case TenantAction.Add:
                    if (!_workflowTenants.Tenants.Contains(tenant))
                    {
                        _workflowTenants.Tenants.Add(tenant);
                        _logger.LogInformation("Customer code {code} was added to workflow tenant list.", tenant);
                    }
                    break;
                case TenantAction.Remove:
                    if (_workflowTenants.Tenants.Contains(tenant))
                    {
                        _workflowTenants.Tenants.Remove(tenant);
                        _logger.LogInformation("Customer code {code} was removed from workflow tenant list.", tenant);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported action {action} for tenant {tenant}");
            }
        }
    }
}
