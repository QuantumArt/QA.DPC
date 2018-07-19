using QA.Core.DPC.QP.Services;
using Unity;

namespace QA.Core.DPC.QP.Models
{
    public class FactoryBuilder
    {
        public IUnityContainer Container { get; set; }
        public string FactoryName { get; set; }
        public IFactory Factory { get; set; }
    }
}
