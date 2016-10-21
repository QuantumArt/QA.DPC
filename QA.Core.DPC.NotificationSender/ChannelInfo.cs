using System;

namespace QA.Core.DPC
{
	public class ChannelInfo
	{
		public string Name { get; set; }
		public State State { get; set; }

        public int Count { get; set; }
        public string LastStatus { get; set; }
        public DateTime? LastQueued { get; set; }
        public DateTime? LastPublished { get; set; }
        public int? LastId { get; set; }
    }

	public enum State
	{
		New,
		Actual,
		Chanded,
		Deleted
	}
}
