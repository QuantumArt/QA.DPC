namespace QA.Core.DPC
{
    public class NotificationProperties
    {

        public NotificationProperties()
        {
            SettingsSource = SettingsSource.AppSettings;
            ChannelSource = ChannelSource.Content;
        }

        public SettingsSource SettingsSource { get; set; }
        
        public ChannelSource ChannelSource { get; set; }
        
        public string InstanceId { get; set; }
        
        
    }

    public enum ChannelSource
    {
        Content,
        Configuration
    }

    public enum SettingsSource
    {
        AppSettings,
        Content
    }
}
    
    
