using QA.Core.DPC.QP.Autopublish.Models;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public interface IAutopublishProcessor
    {
        int Publish(ProductItem item);
    }
}
