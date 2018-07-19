using System;
using System.Collections.Generic;

namespace QA.Core.ProductCatalog.Actions
{
    [Serializable]
	public class ActionContext
	{
		public Guid BackendSid { get; set; }

		public int ContentId { get; set; }
		
		public int[] ContentItemIds { get; set; }

		public string ActionCode { get; set; }

		public string CustomerCode { get; set; }

		public Dictionary<string,string> Parameters { get; set; }

		public string UserName = "Admin";

		public int UserId = 1;
	}
}