using System;
using System.Globalization;

namespace QA.Core.DPC.QP.Autopublish.Models
{
    [Serializable]
    public class ChannelItem
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public bool IsStage { get; set; }
        public string Filter { get; set; }
        public CultureInfo Culture { get; set; }
    }
}
