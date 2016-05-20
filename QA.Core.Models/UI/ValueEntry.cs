namespace QA.Core.Models.UI
{
    public class ValueEntry
    {
        public object Value { get; private set; }

        public ValueEntry(object value)
        {
            Value = value;
        }
    }
}
