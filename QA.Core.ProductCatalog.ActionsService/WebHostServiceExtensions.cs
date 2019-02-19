using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace QA.Core.ProductCatalog.ActionsService
{

    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var actionsService = host.Services.GetRequiredService<ActionsService>();
            var webHostService = new ActionsServiceWebHostAdapter(host, actionsService);
            ServiceBase.Run(webHostService);
        }
    }
}