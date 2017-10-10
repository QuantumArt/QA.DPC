using System;

namespace QA.Core.DPC
{
	public class ConfigurationInfo
	{
		public bool IsActual { get; set; }
        public DateTime Started { get; set; }
        public string NotificationProvider { get; set; }
		public ChannelInfo[] Channels { get; set; }
        public SettingsInfo ActualSettings { get; set; }
        public SettingsInfo CurrentSettings { get; set; }
    }
}