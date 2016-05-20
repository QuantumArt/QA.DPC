using System.Linq;
using System.ServiceProcess;
using System.Threading;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsService.Properties;
using QA.Core.ProductCatalog.TaskScheduler;

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

            _actionRunners = new ITasksRunner[Settings.Default.NumberOfThreads];

            for (int i = 0; i < _actionRunners.Length; i++)
            {
                _actionRunners[i] = ObjectFactoryBase.Resolve<ITasksRunner>();

                var actionRunnerThread = new Thread(_actionRunners[i].Run);

                actionRunnerThread.Start();
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
