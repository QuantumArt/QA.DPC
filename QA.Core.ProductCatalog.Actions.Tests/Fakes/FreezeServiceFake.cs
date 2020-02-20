using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class FreezeServiceFake : IFreezeService
    {
        public FreezeState GetFreezeState(int productId)
        {
            return FreezeState.Unfrosen;
        }

        public int[] GetFrozenProductIds(int[] productIds)
        {
            return new int[0];
        }

        public int[] GetUnfrozenProductIds()
        {
            return new int[0];
        }

        public void ResetFreezing(params int[] productsId)
        {            
        }
    }
}
