using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace QA.Core.DPC
{
    public class NotificationSenderWebHostAdapter: WebHostService
    {
        private NotificationSender _service;
        
        public NotificationSenderWebHostAdapter(IWebHost host, NotificationSender service) : base(host)
        {
            _service = service;
        }

        protected override void OnStarted()
        {
            _service.Start();
        }

        protected override void OnStopped()
        {
            _service.Stop();
        }
    }
}