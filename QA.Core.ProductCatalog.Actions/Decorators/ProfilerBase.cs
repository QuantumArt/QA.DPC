using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QA.Core.Logger;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
    public class ProfilerBase
    {
        private readonly ILogger _logger;
        protected string Service { get; set; }

        public ProfilerBase(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _logger = logger;
        }


        protected ProfilerToken CallMethod(string name, string format, params object[] args)
        {
            var token = new ProfilerToken() { Service = Service, Method = name };
            token.AddParameters(format, args);
            token.Timer.Start();
            _logger.Info("Call: " + token.FullName + " " + token.Parameters);
            return token;
        }

        protected ProfilerToken CallMethod([CallerMemberName] string name = "")
        {
            return CallMethod(name, null);
        }

        protected void EndMethod(ProfilerToken token, string format, params object[] args)
        {
            token.Timer.Stop();
            token.AddResult(format, args);
            _logger.Info("End: {0} Time:{1:s\\.fff} {2}", token.FullName, token.Timer.Elapsed, token.Result);
        }

        protected void EndMethod(ProfilerToken token)
        {
            EndMethod(token, "");
        }

        protected void EndMethod(ProfilerToken token, string[] result)
        {
            EndMethod(token, "[" + string.Join("], [", result) + "]");
        }

        protected void EndMethod(ProfilerToken token, Dictionary<int, Dictionary<int, List<int>>> result)
        {
            EndMethod(token, "Dictionary");
        }
    }
}
