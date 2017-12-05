using System;

namespace QA.Core.DPC.QP.Models
{
    public class FactoryWatcherEventArgs : EventArgs
    {
        public FactoryWatcherEventArgs(string[] deletedCodes, string[] modifiedCodes, string[] newCodes)
        {
            DeletedCodes = deletedCodes;
            ModifiedCodes = modifiedCodes;
            NewCodes = newCodes;
        }

        public string[] DeletedCodes { get; private set; }
        public string[] ModifiedCodes { get; private set; }
        public string[] NewCodes { get; private set; }
    }
}
