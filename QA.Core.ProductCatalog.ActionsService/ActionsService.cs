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
        private Dictionary<string, ITasksRunner[]> _runnersDictionary = new Dictionary<string, ITasksRunner[]>();
        private TaskSchedulerRunner _taskSchedulerRunner;
        private ActionsServiceProperties _options;
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        
        public ActionsService(
            IFactoryWatcher watcher,
            TaskSchedulerRunner schedulerRunner,
            IOptions<ActionsServiceProperties> options
        )
        {
            _factoryWatcher = watcher;
            _taskSchedulerRunner = schedulerRunner;
            _options = options.Value;
        }
        
        public void Start()
        {
            _logger.Info("{serviceName} started", _options.Name);
            
            _factoryWatcher.OnConfigurationModify += _factoryWatcher_OnConfigurationModify;
            _factoryWatcher.Watch();
            _factoryWatcher.Start();

            if (_options.EnableScheduleProcess)
            {
                _logger.Info("Scheduler for {serviceName} started", _options.Name);
                _taskSchedulerRunner.Start();
            }

        }

        private void _factoryWatcher_OnConfigurationModify(object sender, FactoryWatcherEventArgs e)
        {
            var stoppedCustomerCodes = _runnersDictionary.Where(de => de.Value.All(t => t.State == StateEnum.Stopped)).Select(de => de.Key).ToArray();

            foreach (var code in stoppedCustomerCodes)
            {
                _runnersDictionary.Remove(code);
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

            if (_runnersDictionary.TryGetValue(customerCode, out var tasks))
            {
                var stoppingTasks = tasks.Where(t => t.State == StateEnum.WaitingToStop).ToArray();
                var runningTasks = tasks.Where(t => t.State == StateEnum.Running).ToArray();

                runners = new List<ITasksRunner>(stoppingTasks.Union(runningTasks));

                for (int i = 0; i < Math.Max(_options.NumberOfThreads - runningTasks.Length, 0); i++)
                {
                    runners.Add(StartRunner(customerCode));
                }

                tasks = runners.ToArray();
            }
            else
            {
                runners = new List<ITasksRunner>();

                for (int i = 0; i < _options.NumberOfThreads; i++)
                {
                    runners.Add(StartRunner(customerCode));
                }

                _runnersDictionary[customerCode] = runners.ToArray();
            }
        }

        private void RemoveCode(string customerCode)
        {
            if (_runnersDictionary.TryGetValue(customerCode, out var tasks))
            {
                foreach (var task in tasks)
                {
                    task.InitStop();
                }
            }
        }

        private ITasksRunner StartRunner(string customerCode)
        {
            var runner = ObjectFactoryBase.Resolve<ITasksRunner>();
            ThreadPool.QueueUserWorkItem(runner.Run, customerCode);
            return runner;
        }

        public void Stop()
        {
            _logger.Info("{serviceName} stopping...", _options.Name);
            _factoryWatcher.OnConfigurationModify -= _factoryWatcher_OnConfigurationModify;

            foreach (var code in _runnersDictionary.Keys)
            {
                RemoveCode(code);
            }

            if (_taskSchedulerRunner != null)
            {
                _taskSchedulerRunner.Stop();
            }

            //нельзя выходить из этого метода пока не убедимся что все треды закончили работу
            while (_runnersDictionary.Where(de => de.Value.All(t => t.State != StateEnum.Stopped)).Any())
            {
                Thread.Sleep(500);
            }

            _factoryWatcher.Stop();
            _runnersDictionary.Clear();
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
