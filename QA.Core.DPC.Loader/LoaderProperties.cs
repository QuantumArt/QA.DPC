namespace QA.Core.DPC.Loader
{
    public class LoaderProperties
    {

        public LoaderProperties()
        {
            SettingsSource = SettingsSource.AppSettings;
            DefaultSerializer = DefaultSerializer.Json;
        }
        
        public SettingsSource SettingsSource{ get; set; }
        
        public DefaultSerializer DefaultSerializer { get; set; }
        
        
        public int LoaderWarmUpProductId { get; set; }
        
        public int LoaderWarmUpRepeatInMinutes { get; set; }
        
    }
    
    
    public enum SettingsSource
    {
        AppSettings,
        Content
    }

    public enum DefaultSerializer
    {
        Json,
        Xml
    }


}