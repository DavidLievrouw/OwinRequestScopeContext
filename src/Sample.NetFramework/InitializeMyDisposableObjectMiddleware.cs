using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DavidLievrouw.OwinRequestScopeContext;

namespace Sample {
    public class InitializeMyDisposableObjectMiddleware {
        readonly Func<IDictionary<string, object>, Task> _next;

        public InitializeMyDisposableObjectMiddleware(Func<IDictionary<string, object>, Task> next) {
            if (next == null) throw new ArgumentNullException(nameof(next));
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment) {
            var requestScopeContext = OwinRequestScopeContext.Current;

            // Add item to the request scope context, that will be disposed when the requests completes
            var myDisposableObject_ToBeDisposed = new MyDisposableObject {Value = "To dispose: " + DateTime.Now.ToString("HH:mm:ss,fff")};
            requestScopeContext.Items.Add("MyDisposableObject_ToBeDisposed", myDisposableObject_ToBeDisposed, true);

            // Add item to the request scope context, that will not be disposed when the requests completes
            var myDisposableObject_NotToBeDisposed = new MyDisposableObject {Value = "Not to dispose: " + DateTime.Now.ToString("HH:mm:ss,fff")};
            requestScopeContext.Items.Add("MyDisposableObject_NotToBeDisposed", myDisposableObject_NotToBeDisposed, false);

            // Add some other (non-IDisposable) item
            requestScopeContext.Items.Add("MyNonDisposableObject", 42);

            await _next.Invoke(environment).ConfigureAwait(false);
        }
    }
}