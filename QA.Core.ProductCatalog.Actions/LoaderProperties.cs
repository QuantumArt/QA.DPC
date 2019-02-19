namespace QA.Core.ProductCatalog.Actions
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