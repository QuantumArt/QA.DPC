using Newtonsoft.Json;

namespace QA.Core.ProductCatalog.Actions.Tasks
{
	public class ActionData
	{
		public ActionContext ActionContext { get; set; }

		public string Description { get; set; }

		public string IconUrl { get; set; }

		public static ActionData Deserialize(string data)
		{
			return JsonConvert.DeserializeObject<ActionData>(data);
		}

		public static string Serialize(ActionData data)
		{
			return JsonConvert.SerializeObject(data);
		}
	}
}
