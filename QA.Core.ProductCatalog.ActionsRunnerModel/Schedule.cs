//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Schedule
    {
        public Schedule()
        {
            this.Tasks = new HashSet<Task>();
        }
    
        public int ID { get; set; }
        public bool Enabled { get; set; }
        public string CronExpression { get; set; }
    
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
