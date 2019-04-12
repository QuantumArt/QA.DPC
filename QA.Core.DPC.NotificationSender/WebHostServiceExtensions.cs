using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace QA.Core.DPC
{

    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var actionsService = host.Services.GetRequiredService<NotificationSender>();
            var webHostService = new NotificationSenderWebHostAdapter(host, actionsService);
            ServiceBase.Run(webHostService);
        }
    }
}