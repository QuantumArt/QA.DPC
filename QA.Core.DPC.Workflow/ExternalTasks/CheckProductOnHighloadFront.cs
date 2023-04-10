using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.Workflow.Models;
using QA.Workflow.Extensions;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;

namespace QA.Core.DPC.Workflow.ExternalTasks
{
    public class CheckProductOnHighloadFront : IExternalTaskHandler
    {
        private const string UrlFormat = "{0}/api/{1}/{2}/{3}/{4}/products/{5}";
        private const string UpdateDateParameter = "UpdateDate";
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegrationProperties _integrationProperties;

        public CheckProductOnHighloadFront(IOptions<IntegrationProperties> integrationProperties, IHttpClientFactory httpClientFactory)
        {
            _integrationProperties = integrationProperties.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
        {
            int productId = processInstance.GetVariableByName<int>(ExternalWorkflowQpDpcSettings.ContentItemId);
            string state = processInstance.GetVariableByName<string>(InternalSettings.HighloadStateName);
            string version = processInstance.GetVariableByName<string>(InternalSettings.HighloadApiVersion);
            string culture = processInstance.GetVariableByName<string>(InternalSettings.Culture);
            string retryCountVariableName = processInstance.GetVariableByName<string>(InternalSettings.RetryCountVariable);
            string resultVariableName = processInstance.GetVariableByName<string>(InternalSettings.ResultVariable);
            string publishDateVariableName = processInstance.GetVariableByName<string>(InternalSettings.PublishDateVariable);

            DateTime publishDate = processInstance.GetVariableByName<DateTime>(publishDateVariableName);
            int retryCount = processInstance.GetVariableByNameOrDefault<int>(retryCountVariableName);

            string url = string.Format(UrlFormat,
                _integrationProperties.HighloadFrontSearchUrl,
                processInstance.TenantId,
                version,
                culture,
                state,
                productId);

            using HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage result = await client.GetAsync(url);

            result.EnsureSuccessStatusCode();

            string body = await result.Content.ReadAsStringAsync();
            JObject jsonResult = JObject.Parse(body);
            bool productUpdated = false;

            if (jsonResult.TryGetValue(UpdateDateParameter, out JToken updateDate))
            {
                productUpdated = DateTime.TryParse(updateDate.ToString(), out DateTime updateDateParsed) && updateDateParsed >= publishDate;
            }

            retryCount = productUpdated ? 0 : retryCount + 1;

            return new()
                {
                    { retryCountVariableName, retryCount },
                    { resultVariableName, productUpdated }
                };
        }
    }
}
