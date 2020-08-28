namespace QA.Core.DPC.QP.Models
{
    public class IntegrationProperties
    {
        public string WcfNotificationUrl { get; set; }
        
        public string RestNotificationUrl { get; set; }
        
        public string HighloadFrontSyncUrl { get; set; }
        
        public string TarantoolSyncUrl { get; set; }
        
        public string DpcWebApiUrl { get; set; }
        
        public string TarantoolApiUrl { get; set; }
        
        public string[] ExtraValidationLibraries { get; set; }
        public string ConfigurationServiceUrl { get; set; }
        public string ConfigurationServiceToken { get; set; }
        
        public bool UseSameSiteNone { get; set; }
    }
}