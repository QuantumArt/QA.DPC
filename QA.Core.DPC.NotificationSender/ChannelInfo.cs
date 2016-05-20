namespace QA.Core.DPC
{
	public class ChannelInfo
	{
		public string Name { get; set; }
		public State State { get; set; }
	}

	public enum State
	{
		New,
		Actual,
		Chanded,
		Deleted
	}
}
