using System.Collections.Generic;
using System.Linq;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public class OptionsManager<TOptions>: IOptions<TOptions> where TOptions : class, new()
    {
        private TOptions _options;
        private IEnumerable<IConfigureOptions<TOptions>> _setups;

        public OptionsManager(IEnumerable<IConfigureOptions<TOptions>> setups)
        {
            _setups = setups;
        }

        public virtual TOptions Value
        {
            get
            {
                return _options ?? (_options = _setups == null
                    ? new TOptions()
                    : _setups.Aggregate(new TOptions(),
                        (options, setup) =>
                        {
                            setup.Configure(options);
                            return options;
                        }));
            }
        }
    }
}