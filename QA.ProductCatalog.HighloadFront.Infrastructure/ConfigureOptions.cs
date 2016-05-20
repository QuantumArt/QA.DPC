using System;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public class ConfigureOptions<TOptions> : IConfigureOptions<TOptions> where TOptions : class
    {
        public ConfigureOptions(Action<TOptions> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Action = action;
        }

        public Action<TOptions> Action { get; private set; }

        public virtual void Configure(TOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Action.Invoke(options);
        }
    }
}