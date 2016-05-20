using System;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.Extensions.Rules.Remote;

namespace QA.Validation.Extensions.Tests.Stubs
{
    /// <summary>
    /// Тестовый http-хендлер с возможностью вставки любой логики и проверок
    /// </summary>
    public class StubValidationHandler : ValidationHandlerBase
    {
        private Action<RemoteValidationContext, RemoteValidationResult> _stubAction;

        public StubValidationHandler(Action<RemoteValidationContext, RemoteValidationResult> stubAction)
        {
            _stubAction = stubAction;
        }
        protected override void OnValidation(RemoteValidationContext model, RemoteValidationResult context)
        {
            _stubAction(model, context);
        }

        public override bool IsReusable
        {
            get { return true; }
        }
    }
}
