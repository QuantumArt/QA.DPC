namespace QA.Core.Models.Entities
{
    public class DPathFilterData
    {
        public string Expression { get; set; }

        public string Value { get; set; }

        public bool IsInversed { get; set; }

        public bool IsDisjuncted { get; set; }
    }
}
