using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.TaskScheduler;
using NLog;

namespace QA.Core.ProductCatalog.ActionsService
{
    public class ActionsService: IHostedService
    {
        
        private IFactoryWatcher _factoryWatcher;
        private Dictionary<string, ActionsServiceContext> _contextMap = new Dictionary<string, ActionsServiceContext>();
        private Func<TaskSchedulerRunner> _getSchedulerRunner;
        private Func<ITasksRunner> _getRunner;
        private ActionsServiceProperties _options;
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        
        public ActionsService(
            IFactoryWatcher watcher,
            Func<TaskSchedulerRunner> getSchedulerRunner,
            Func<ITasksRunner> getRunner,
            IOptions<ActionsServiceProperties> options
        )
        {
            _factoryWatcher = watcher;
            _getSchedulerRunner = getSchedulerRunner;
            _getRunner = getRunner;
            _options = options.Value;
        }
        
        public void Start()
        {
            _logger.Info("{serviceName} started", _options.Name);
            
            _factoryWatcher.OnConfigurationModify += _factoryWatcher_OnConfigurationModify;
            _factoryWatcher.Start();
            ThreadPool.QueueUserWorkItem(Watch);
        }

        public void Watch(object? stateInfo)
        {
            _factoryWatcher.Watch();
        }
        
        private void _factoryWatcher_OnConfigurationModify(object sender, FactoryWatcherEventArgs e)
        {
            var stoppedCustomerCodes = _contextMap.Where(c => c.Value.IsStopped()).Select(c => c.Key).ToArray();

            foreach (var code in stoppedCustomerCodes)
            {
                _contextMap.Remove(code);
            }

            foreach (var code in e.DeletedCodes)
            {
                RemoveCode(code);
            }

            foreach (var code in e.NewCodes)
            {
                AddCode(code);
            }          
        }

        private void AddCode(string customerCode)
        {
            List<ITasksRunner> runners;

            if (_contextMap.TryGetValue(customerCode, out var context))
            {
                var stoppingTasks = context.Runners.Where(t => t.State == StateEnum.WaitingToStop).ToArray();
                var runningTasks = context.Runners.Where(t => t.State == StateEnum.Running).ToArray();
                runners = new List<ITasksRunner>(stoppingTasks.Union(runningTasks));

                for (int i = 0; i < Math.Max(_options.NumberOfThreads - runningTasks.Length, 0); i++)
                {
                    runners.Add(StartRunner(customerCode));
                }

                context.Runners.Clear();
                context.Runners.AddRange(runners);

                if (_options.EnableScheduleProcess)
                {
                    context.SchedulerRunner = StartScheduler(customerCode);
                }
                else if(context.SchedulerRunner != null && !context.SchedulerRunner.IsRunning)
                {
                    context.SchedulerRunner = null;
                }
            }
            else
            {
                var newContext = new ActionsServiceContext(customerCode);

                if (_options.EnableScheduleProcess)
                {
                    newContext.SchedulerRunner = StartScheduler(customerCode);
                } 

                for (int i = 0; i < _options.NumberOfThreads; i++)
                {
                    newContext.Runners.Add(StartRunner(customerCode));
                }

                _contextMap[customerCode] = newContext;
            }
        }

        private void RemoveCode(string customerCode)
        {
            if (_contextMap.TryGetValue(customerCode, out var context))
            {
                if (context.SchedulerRunner != null)
                {
                    context.SchedulerRunner.Stop();
                }

                foreach (var task in context.Runners)
                {
                    task.InitStop();
                }
            }
        }

        private TaskSchedulerRunner StartScheduler(string customerCode, TaskSchedulerRunner runner = null)
        {
            var schedulerRunner = runner == null ? _getSchedulerRunner() : runner;
            _logger.Info("Scheduler for {customerCode} in {serviceName} started", customerCode, _options.Name);
            schedulerRunner.Start(customerCode);
            return schedulerRunner;
        }

        private ITasksRunner StartRunner(string customerCode)
        {
            var runner = _getRunner();
            ThreadPool.QueueUserWorkItem(runner.Run, customerCode);
            return runner;
        }

        public void Stop()
        {
            _logger.Info("{serviceName} stopping...", _options.Name);
            _factoryWatcher.OnConfigurationModify -= _factoryWatcher_OnConfigurationModify;

            foreach (var code in _contextMap.Keys)
            {
                RemoveCode(code);
            }

            //нельзя выходить из этого метода пока не убедимся что все треды закончили работу
            while (_contextMap.Where(c => !c.Value.IsStopped()).Any())
            {
                Thread.Sleep(500);
            }

            _factoryWatcher.Stop();
            _contextMap.Clear();
            _logger.Info("{serviceName} stopped", _options.Name);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }
    }
}
