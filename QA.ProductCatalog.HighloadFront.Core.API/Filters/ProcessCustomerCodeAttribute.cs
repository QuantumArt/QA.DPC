using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using QA.Core;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.HighloadFront.Elastic;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{
    public class ProcessCustomerCodeAttribute : TypeFilterAttribute
    {
        public ProcessCustomerCodeAttribute() : base(typeof(ProcessCustomerCodeFilter))
        {

        }

        private class ProcessCustomerCodeFilter: IResultFilter
        {
            private readonly DataOptions _options;
            private readonly IIdentityProvider _provider;
            private const string CustomerCodeKey = "customerCode";

            public ProcessCustomerCodeFilter(IOptions<DataOptions> options, IIdentityProvider provider)
            {
                _options = options.Value;
                _provider = provider;
            }

            public void OnResultExecuting(ResultExecutingContext context)
            {
                var code = _options.FixedCustomerCode;
                code = (!string.IsNullOrEmpty(code)) ? code : context.HttpContext.Request.Query[CustomerCodeKey].FirstOrDefault();
                if (!string.IsNullOrEmpty(code))
                {
                    _provider.Identity = new Identity(code);
                }
                else
                {
                    context.Result = new ContentResult()
                    {
                        Content = @"{""message"":""Customer code is not defined""}",
                        ContentType = "application/json",
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            public void OnResultExecuted(ResultExecutedContext context)
            {
            }
        }
    }
}
