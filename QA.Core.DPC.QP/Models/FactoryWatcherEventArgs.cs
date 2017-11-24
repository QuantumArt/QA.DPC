using System;

namespace QA.Core.DPC.QP.Models
{
    public class FactoryWatcherEventArgs : EventArgs
    {
        public FactoryWatcherEventArgs(string[] deletedCodes, string[] modifiedCodes, string[] newcodes)
        {
            DeletedCodes = deletedCodes;
            ModifiedCodes = modifiedCodes;
            Newcodes = newcodes;
        }

        public string[] DeletedCodes { get; private set; }
        public string[] ModifiedCodes { get; private set; }
        public string[] Newcodes { get; private set; }
    }
}
