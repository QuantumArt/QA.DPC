using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QA.Core.ProductCatalog.ActionsRunnerModel;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class TasksPageInfo
    {
        public bool ShowOnlyMine { get; set; }

        private bool _notify;
        public bool Notify { get { return ShowOnlyMine && _notify; } set { _notify = value; } }

		public Dictionary<int, string> States { get; set; }
	    public bool AllowSchedule { get; set; }
    }
}