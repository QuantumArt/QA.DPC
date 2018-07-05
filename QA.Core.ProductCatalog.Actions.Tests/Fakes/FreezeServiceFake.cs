using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class FreezeServiceFake : IFreezeService
    {
        public FreezeState GetFreezeState(int productId)
        {
            return FreezeState.Unfrosen;
        }

        public int[] GetFrosenProductIds(int[] productIds)
        {
            return new int[0];
        }

        public int[] GetUnfrosenProductIds()
        {
            return new int[0];
        }

        public void ResetFreezing(params int[] productsId)
        {            
        }
    }
}
