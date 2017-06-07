using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
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
            private readonly IIdentityProvider _provider;

            public ProcessCustomerCodeFilter(IIdentityProvider provider)
            {
                _provider = provider;
            }

            public void OnResultExecuting(ResultExecutingContext context)
            {
                if (string.IsNullOrEmpty(_provider.Identity?.CustomerCode))
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
