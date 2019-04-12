using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API
{
    
    public class ConnectionService
    {
        private readonly HttpContext _context;

        private readonly DataOptions _options;
        
        public ConnectionService(IHttpContextAccessor httpContextAccessor, DataOptions options)
        {
            _context = httpContextAccessor.HttpContext;
            _options = options;
        }

        public string GetConnectionString()
        {
            var cc = _context.GetRouteData().Values["customerCode"]?.ToString();
            return cc == null ? (_options.DesignConnectionString ?? _options.FixedConnectionString) : DBConnector.GetConnectionString(cc);
        }
        
        
    }
}