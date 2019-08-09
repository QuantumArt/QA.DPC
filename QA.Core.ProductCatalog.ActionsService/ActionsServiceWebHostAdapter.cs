using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace QA.Core.ProductCatalog.ActionsService
{
    public class ActionsServiceWebHostAdapter: WebHostService
    {
        private ActionsService _service;
        
        public ActionsServiceWebHostAdapter(IWebHost host, ActionsService service) : base(host)
        {
            _service = service;
        }

        protected override void OnStarted()
        {
            _service.Start();
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            _service.Stop();
            base.OnStopping();
        }
    }
}