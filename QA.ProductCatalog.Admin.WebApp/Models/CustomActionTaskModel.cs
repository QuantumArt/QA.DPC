﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.ProductCatalog.Actions.Tasks;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class CustomActionTaskModel:TaskModel
    {
        public CustomActionTaskModel(Task task):base(task)
        {
			var actionData = ActionData.Deserialize(task.Data);

            IconUrl = actionData.IconUrl;

            Description = actionData.Description;
        }

        [ScaffoldColumn(false)]
        public string IconUrl { get; set; }

        [DisplayName("Описание")]
        public string Description { get; set; }
    }
}