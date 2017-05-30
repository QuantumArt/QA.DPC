namespace QA.Core.ProductCatalog.ActionsRunner
{
    public interface ITasksRunner
    {
        void Run(object customerCode);

        void InitStop();

        StateEnum State { get; }

        void SetTaskProgress(int taskId, byte progress);

        bool GetIsCancellationRequested(int taskId);
    }

    public enum StateEnum : byte
    {
        Stopped,
        Running,
        WaitingToStop
    }
}
