using QA.Core.DPC.QP.Models;
using System.Collections.Generic;

namespace QA.Core.DPC.QP.Services
{
    public interface IFactory
    {
        T Resolve<T>(string key);
        void Register(string key);
        void Clear(string key);
        void Clear();
        string Validate(string key);
        string[] NotConsolidatedCodes { get; set; }
        Dictionary<string, CustomerContext> CustomerMap { get; }        
    }
}