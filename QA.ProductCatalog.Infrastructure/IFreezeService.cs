namespace QA.ProductCatalog.Infrastructure
{
    public interface IFreezeService
    {
        FreezeState GetFreezeState(int productId);
        int[] GetUnfrosenProductIds();
        int[] GetFrosenProductIds(int[] productIds);
        void ResetFreezing(params int[] productIds);
    }

    public enum FreezeState
    {
        Frozen,
        Unfrosen,
        Missing
    }
}
