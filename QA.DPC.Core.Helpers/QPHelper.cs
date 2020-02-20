using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace QA.DPC.Core.Helpers
{
    public class QPHelper
    {

        private readonly HttpContext _httpContext;

        private readonly QPOptions _options; 
        
        public QPHelper(IHttpContextAccessor httpContextAccessor, IOptions<QPOptions> options)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _options = options.Value;
        }

        /// <summary>
        /// Код поставщика
        /// </summary>
        public string CustomerCode => _httpContext?.Request.Query[_options.CustomerCodeParamName] ?? "";

        /// <summary>
        /// Идентификатор сайта
        /// </summary>
        public string SiteId => _httpContext?.Request.Query[_options.SiteIdParamName] ?? "";

        /// <summary>
        /// Id бэкенда
        /// </summary>
        public string BackendSid =>  _httpContext?.Request.Query[_options.BackendSidParamName] ?? "";

        /// <summary>
        /// Id хоста
        /// </summary>
        public string HostId => _httpContext?.Request.Query[_options.HostIdParamName] ?? "";

        /// <summary>
        /// Ключ
        /// </summary>
        public string QpKey => CustomerCode + "_" + SiteId;

        /// <summary>
        /// Признак запуска через Custom Action Qp
        /// </summary>
        public bool IsQpMode => !string.IsNullOrEmpty(HostId);
    }
}