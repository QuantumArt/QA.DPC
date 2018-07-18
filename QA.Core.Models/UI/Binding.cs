namespace QA.Core.Models.UI
{
    public class BindingExression
    {
        public string Expression { get; private set; }
        public BindingMode BindingMode { get; private set; }
        public IValueConverter Converter { get; set; }
        public bool Log { get; set; }

        public BindingExression(string expression)
            : this(expression, BindingMode.Readonly)
        { }

        public BindingExression(string expression, BindingMode mode)
        {
            Expression = expression;
            BindingMode = mode;
        }

        public override int GetHashCode()
        {
            return (Expression.GetHashCode() * 3 ^ (BindingMode.GetHashCode()+11));
        }

        public bool IsAbsolute { get; set; }

        public int Offset { get; set; }

		public IBindingValueProvider BindingValueProvider { get; set; }
        public object Parameter { get; internal set; }
    }
}
