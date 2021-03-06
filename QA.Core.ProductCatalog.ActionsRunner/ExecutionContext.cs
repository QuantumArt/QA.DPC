﻿using System;
using System.Threading;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class ExecutionContext : ITaskExecutionContext
    {
        private readonly ITasksRunner _tasksRunner;

        public ExecutionContext(ITasksRunner tasksRunner, int taskId)
        {
            if (tasksRunner == null)
                throw new ArgumentNullException("tasksRunner");

            _tasksRunner = tasksRunner;

            TaskId = taskId;
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
					_tasksRunner.SetTaskProgress(TaskId, progress);

			        _currentProgress = progress;
		        }
	        }
	        finally
	        {
		        Monitor.Exit(_progressLocker);
	        }
        }

        public ActionTaskResult Result { get; set; }

	    private bool _cancellationAlreadyRequested;
        public bool IsCancellationRequested
        {
	        get
	        {
		        if (_cancellationAlreadyRequested)
			        return true;

		        _cancellationAlreadyRequested = _tasksRunner.GetIsCancellationRequested(TaskId);

		        return _cancellationAlreadyRequested;
	        }
        }

        public bool IsCancelled { get; set; }
        public int TaskId { get; }
    }
}
