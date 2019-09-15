using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DavidLievrouw.OwinRequestScopeContext;

namespace Sample {
    public class ValidateThatThereIsNoCurrentRequestContextBeforeRequestMiddleware {
        readonly Func<IDictionary<string, object>, Task> _next;

        public ValidateThatThereIsNoCurrentRequestContextBeforeRequestMiddleware(
            Func<IDictionary<string, object>, Task> next) {
            if (next == null) throw new ArgumentNullException(nameof(next));
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment) {
            // This middleware is triggered before the OwinRequestScopeContext middleware
            // There should be no instance present
            var requestScopeContext = OwinRequestScopeContext.Current;
            if (requestScopeContext != null) throw new InvalidOperationException($"There should not be an {requestScopeContext.GetType().Name} object available here.");

            Debug.WriteLine($"The {typeof(OwinRequestScopeContext).Name} instance needs to be created.");

            await _next.Invoke(environment).ConfigureAwait(false);
        }
    }
}