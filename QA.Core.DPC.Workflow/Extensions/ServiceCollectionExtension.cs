using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QA.Core.DPC.Workflow.ExternalTasks;
using QA.Core.DPC.Workflow.Services;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.TaskWorker.Extensions;
using QA.Workflow.TaskWorker.Interfaces;

namespace QA.Core.DPC.Workflow.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterWorkflow(this IServiceCollection services, IConfiguration configuration)
    {
        ExtendedCamundaSettings settings = new();
        configuration.GetSection("Camunda").Bind(settings);

        if (!settings.IsEnabled)
        {
            return services;
        }

        services.AddSingleton(Options.Create(settings));

        services.AddHostedService<WorkflowTenantWatcherHostedService>();

        // Add camunda worker
        IExternalTaskCollection taskCollection = services.RegisterCamundaExternalTaskWorker(configuration);
        // Register publish task
        services.AddSingleton<PublishProduct>();
        taskCollection.Register<PublishProduct>();
        // Register send product on stage task
        services.AddSingleton<SendToStage>();
        taskCollection.Register<SendToStage>();
        // Register check product on reference front task
        services.AddSingleton<CheckProductOnReferenceFront>();
        taskCollection.Register<CheckProductOnReferenceFront>();
        // Register check product on HL front task
        services.AddSingleton<CheckProductOnHighloadFront>();
        taskCollection.Register<CheckProductOnHighloadFront>();

        return services;
    }
}
