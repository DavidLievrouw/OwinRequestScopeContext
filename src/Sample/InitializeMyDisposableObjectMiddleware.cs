using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DavidLievrouw.OwinRequestScopeContext;

namespace Sample {
  public class InitializeMyDisposableObjectMiddleware {
    readonly Func<IDictionary<string, object>, Task> _next;

    public InitializeMyDisposableObjectMiddleware(Func<IDictionary<string, object>, Task> next) {
      if (next == null) throw new ArgumentNullException(paramName: nameof(next));
      _next = next;
    }

    public async Task Invoke(IDictionary<string, object> environment) {
      var myDisposableObject = new MyDisposableObject {
        Value = DateTime.Now.ToString("HH:mm:ss,fff")
      };

      // Add some setting to the dictionary
      var requestScopeContext = OwinRequestScopeContext.Current;
      requestScopeContext.Items["MyDisposableObject"] = myDisposableObject;
      requestScopeContext.Items["MyNonDisposableObject"] = 42;

      // Register item for disposal
      requestScopeContext.RegisterForDisposal(myDisposableObject);

      await _next.Invoke(environment).ConfigureAwait(false);
    }
  }
}