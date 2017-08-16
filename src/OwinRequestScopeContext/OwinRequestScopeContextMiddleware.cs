using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DavidLievrouw.OwinRequestScopeContext {
  internal class OwinRequestScopeContextMiddleware {
    readonly Func<IDictionary<string, object>, Task> _next;

    public OwinRequestScopeContextMiddleware(Func<IDictionary<string, object>, Task> next) {
      _next = next;
    }

    public async Task Invoke(IDictionary<string, object> environment) {
      if (OwinRequestScopeContext.Current != null) throw new InvalidOperationException($"There is already an {OwinRequestScopeContext.Current.GetType().Name} for the current request scope.");
      using (var scopeContext = new OwinRequestScopeContext(environment)) {
        OwinRequestScopeContext.Current = scopeContext;
        if (_next != null) await _next.Invoke(environment).ConfigureAwait(false);
      }
    }
  }
}