namespace QA.ProductCatalog.ContentProviders
{
    public interface ITaskExecutionContext
    {
        /// <summary>
        /// прогресс в процентах
        /// </summary>
        /// <param name="progress"></param>
        void SetProgress(byte progress);

        /// <summary>
        /// сообщение при успешном завершении
        /// не обязательно
        /// при ошибке в бд пойдет текст из эксепшена, это поле не пойдет
        /// </summary>
        ActionTaskResult Result { get; set; }

        bool IsCancellationRequested { get; }
        bool IsCancelled { get; set; }
        
        int TaskId { get; }
    }
}