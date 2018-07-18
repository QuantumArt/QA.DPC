using QA.Core.Models.UI;
namespace QA.Core.DPC.UI
{
    public interface IProductControlProvider
    {
        UIElement GetControlForProduct(QA.Core.Models.Entities.Article product);
    }
}
