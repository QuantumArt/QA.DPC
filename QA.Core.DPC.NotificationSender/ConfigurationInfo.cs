using System;

namespace QA.Core.DPC
{
	public class ConfigurationInfo
	{
		public bool IsAtual { get; set; }
        public DateTime Started { get; set; }
        public string NotificationProvider { get; set; }
		public ChannelInfo[] Channels { get; set; }
	}
}