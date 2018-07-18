namespace QA.Core.Models.UI
{
    public interface ITitled<out T>
    {
        T Title { get; }
    }
}
