using System;
using System.Reflection;
using System.Windows.Markup;
using QA.Core.Logger;

namespace QA.Core.Models.UI
{
    public class BindingExtension : MarkupExtension
    {
        /// <summary>
        /// Ключ
        /// </summary>
        [ConstructorArgument("path")]
        public string Path { get; set; }
        
		[ConstructorArgument("mode")]
        public BindingMode Mode { get; set; }
        
		public bool IsAbsolute { get; set; }
        
		public int Offset { get; set; }
        
		public IValueConverter Converter { get; set; }
        
		public bool Log { get; set; }

        public object Parameter { get; set; }

		public IBindingValueProvider BindingValueProvider{get; set; }

        public BindingExtension() : this(null) { }
        public BindingExtension(string path) : this(path, BindingMode.Readonly) { }
        public BindingExtension(string path, BindingMode mode)
        {
            Path = path;
            Mode = mode;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ILogger logger = null;
            if (Log) logger = ObjectFactoryBase.Resolve<ILogger>();

            if (Log) logger.Debug(string.Format("Binding for path = {0}", Path));

            var ipvt = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            var targetObject = ipvt.TargetObject as DependencyObject;

            string name = ResolveName(ipvt);

            if (name != null && targetObject != null)
            {
                var be = CreateExpression();

                // зарегистрируем биндинг
                var dp = targetObject.GetDependencyProperty(name);

                if (dp != null)
                {
                    if (Log) logger.Debug(string.Format("Register binding with path {0} for dp {1}.{2} {3}", Path, dp.OwnerType, dp.PropertyType, dp));

                    targetObject.RegisterBinding(dp, be);
                }
                else
                {
                    if (Log) logger.Debug(string.Format("DP is not found for path {0} obj {1} prop {2}", Path, ipvt.TargetObject, ipvt.TargetProperty));
                }
            }
            else if (ipvt.TargetObject is BindingExtension && name != null)
            {
                var be = CreateExpression();

                return be;
            }
            else
            {
                if (Log) logger.Debug(string.Format("Something is failed {0} {1}", ipvt.TargetObject, ipvt.TargetProperty));
            }

            return null;
        }

        private BindingExression CreateExpression()
        {
            var be = new BindingExression(Path, Mode)
            {
                IsAbsolute = IsAbsolute,
                Offset = Offset,
                Converter = Converter,
                Parameter = Parameter,
                Log = Log,
				BindingValueProvider = BindingValueProvider
            };
            return be;
        }

        private static string ResolveName(IProvideValueTarget ipvt)
        {
            string name = null;

            var propertyInfo = ipvt.TargetProperty as PropertyInfo;
            if (propertyInfo != null)
            {
                name = propertyInfo.Name;
            }
            else
            {
                var runtimeMethodInfo = ipvt.TargetProperty as MethodInfo;

                if (runtimeMethodInfo != null)
                {
                    name = runtimeMethodInfo.Name;
                    if (name != null && name.StartsWith("Set") && name.Length > 3)
                    {
                        name = name.Substring(3);
                    }
                }
            }
            return name;
        }

    }
}
