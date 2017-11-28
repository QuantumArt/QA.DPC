using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsService.Properties;
using QA.Core.ProductCatalog.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace QA.Core.ProductCatalog.ActionsService
{
    public partial class ActionsService : ServiceBase
    {
        public ActionsService()
        {
            InitializeComponent();
        }

        private IFactoryWatcher _factoryWatcher;
        private Dictionary<string, ITasksRunner[]> _runnersDictionary = new Dictionary<string, ITasksRunner[]>();
        private TaskSchedulerRunner _taskSchedulerRunner;

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
            UnityConfig.Configure();

            _factoryWatcher = ObjectFactoryBase.Resolve<IFactoryWatcher>();            
            _factoryWatcher.OnConfigurationModify += _factoryWatcher_OnConfigurationModify;
            _factoryWatcher.Start();            

            if (Settings.Default.EnableSheduleProcess)
            {
                _taskSchedulerRunner = ObjectFactoryBase.Resolve<TaskSchedulerRunner>();

                _taskSchedulerRunner.Start();
            }
        }

        private void _factoryWatcher_OnConfigurationModify(object sender, FactoryWatcherEventArgs e)
        {
            foreach (var code in e.DeletedCodes)
            {
                RemoveCode(code);
            }

            foreach (var code in e.Newcodes)
            {
                AddCode(code);
            }

            var stoppedCustomerCodes = _runnersDictionary.Where(de => de.Value.All(t => t.State == StateEnum.Stopped)).Select(de => de.Key).ToArray();

            foreach (var code in stoppedCustomerCodes)
            {
                _runnersDictionary.Remove(code);
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

                for (int i = 0; i < Math.Max(Settings.Default.NumberOfThreads - runningTasks.Length, 0); i++)
                {
                    runners.Add(StartRunner(customerCode));
                }

                tasks = runners.ToArray();
            }
            else
            {
                runners = new List<ITasksRunner>();

                for (int i = 0; i < Settings.Default.NumberOfThreads; i++)
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
            var actionRunnerThread = new Thread(runner.Run);
            actionRunnerThread.Start(customerCode);
            return runner;
        }

        protected override void OnStop()
        {
            _factoryWatcher.OnConfigurationModify -= _factoryWatcher_OnConfigurationModify;
            _factoryWatcher.Stop();

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

            _runnersDictionary.Clear();
        }
    }
}
