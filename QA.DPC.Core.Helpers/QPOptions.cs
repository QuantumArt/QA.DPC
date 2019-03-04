namespace QA.DPC.Core.Helpers
{
    public class QPOptions
    {
        public QPOptions()
        {
            CustomerCodeParamName = "CustomerCode";
            SiteIdParamName = "SiteId";
            BackendSidParamName = "BackendSid";
            HostIdParamName = "hostUID";            
        }
        
        public string CustomerCodeParamName { get; set; }
        
        public string SiteIdParamName { get; set; }
        
        public string BackendSidParamName { get; set; }
        
        public string HostIdParamName { get; set; }
        
    }
}