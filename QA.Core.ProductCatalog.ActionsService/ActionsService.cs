using System.Linq;
using System.ServiceProcess;
using System.Threading;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsService.Properties;
using QA.Core.ProductCatalog.TaskScheduler;
using QA.Core.DPC.QP.Servives;
using System;

namespace QA.Core.ProductCatalog.ActionsService
{
    public partial class ActionsService : ServiceBase
    {
        public ActionsService()
        {
            InitializeComponent();
        }

        private ITasksRunner[] _actionRunners;

	    private TaskSchedulerRunner _taskSchedulerRunner;

        protected override void OnStart(string[] args)
        {
            UnityConfig.Configure();

            var connectionProvider = ObjectFactoryBase.Resolve<IConnectionProvider>();
            var customerProvider = ObjectFactoryBase.Resolve<ICustomerProvider>();
            var customers = customerProvider.GetCustomers();

            _actionRunners = new ITasksRunner[customers.Length * Settings.Default.NumberOfThreads];

            for (int j = 0; j < customers.Length; j++)
            {
                for (int i = 0; i < _actionRunners.Length; i++)
                {
                    var taskRunnerFactory = ObjectFactoryBase.Resolve<Func<string, ITasksRunner>>();
                    var customerCode = customers[j].CustomerCode;
                    var runner = taskRunnerFactory(customerCode);
                    _actionRunners[i + j] = runner;

                    var actionRunnerThread = new Thread(runner.Run);

                    actionRunnerThread.Start(customerCode);
                }
            }

	        if (Settings.Default.EnableSheduleProcess)
	        {
				_taskSchedulerRunner = ObjectFactoryBase.Resolve<TaskSchedulerRunner>();

				_taskSchedulerRunner.Start();
	        }
        }

        protected override void OnStop()
        {
            foreach (ITasksRunner actionRunner in _actionRunners)
                actionRunner.InitStop();

	        if (_taskSchedulerRunner != null)
		        _taskSchedulerRunner.Stop();

            //нельзя выходить из этого метода пока не убедимся что все треды закончили работу
			while (_actionRunners.Any(x => x.State != StateEnum.Stopped))
                Thread.Sleep(500);
        }
    }
}
