using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class ExecutionContext : ITaskExecutionContext
    {
        private readonly ITasksRunner _tasksRunner;
        private readonly int _taskId;
        
        public ExecutionContext(ITasksRunner tasksRunner, int taskId)
        {
            if (tasksRunner == null)
                throw new ArgumentNullException("tasksRunner");

            _tasksRunner = tasksRunner;

            _taskId = taskId;
        }

	    private byte _currentProgress = 0;


	    private readonly object _progressLocker = new object();

        /// <summary>
        /// прогресс в процентах
        /// </summary>
        /// <param name="progress"></param>
        public void SetProgress(byte progress)
        {
	        if (!Monitor.TryEnter(_progressLocker))
		        return;

	        try
	        {
		        if (progress != _currentProgress)
		        {
					_tasksRunner.SetTaskProgress(_taskId, progress);

			        _currentProgress = progress;
		        }
	        }
	        finally
	        {
		        Monitor.Exit(_progressLocker);
	        }
        }

        /// <summary>
        /// сообщение при успешном завершении
        /// не обязательно
        /// при ошибке в бд пойдет текст из эксепшена, это поле не пойдет
        /// </summary>
        public string Message { get; set; }

	    private bool _cancellationAlreadyRequested;
        public bool IsCancellationRequested
        {
	        get
	        {
		        if (_cancellationAlreadyRequested)
			        return true;

		        _cancellationAlreadyRequested = _tasksRunner.GetIsCancellationRequested(_taskId);

		        return _cancellationAlreadyRequested;
	        }
        }

        public bool IsCancelled { get; set; }
    }
}
