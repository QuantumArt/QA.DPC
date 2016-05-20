using System.Runtime.Serialization;

namespace QA.Core.DPC
{
	[DataContract]
	public class NotificationItem
	{
		[DataMember]
		public int ProductId { get; set; }

		[DataMember]
		public string Data { get; set; }

		[DataMember]
		public string[] Channels { get; set; }
	}
}