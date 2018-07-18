namespace QA.Core.DPC
{
    public class SettingsInfo
    {        
        public int CheckInterval { get; set; }
        public int ErrorCountBeforeWait { get; set; }
        public int WaitIntervalAfterErrors { get; set; }
        public int PackageSize { get; set; }
        public int TimeOut { get; set; }
        public bool Autopublish { get; set; }
    }
}
