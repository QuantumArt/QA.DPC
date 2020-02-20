using System.Data;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IFreezeService
    {
        FreezeState GetFreezeState(int productId);
        int[] GetUnfrozenProductIds();
        int[] GetFrozenProductIds(int[] productIds);
        void ResetFreezing(params int[] productIds);
    }

    public enum FreezeState
    {
        Frozen,
        Unfrosen,
        Missing
    }
}
