using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DavidLievrouw.OwinRequestScopeContext {
  internal class OwinRequestScopeContextMiddleware {
    readonly Func<IDictionary<string, object>, Task> _next;
    readonly OwinRequestScopeContextOptions _options;

    public OwinRequestScopeContextMiddleware(Func<IDictionary<string, object>, Task> next, OwinRequestScopeContextOptions options) {
      _next = next;
      _options = options;
    }

    public async Task Invoke(IDictionary<string, object> environment) {
      if (OwinRequestScopeContext.Current != null) throw new InvalidOperationException($"There is already an {OwinRequestScopeContext.Current.GetType().Name} for the current request scope.");

      var keyComparer = _options?.ItemKeyEqualityComparer ?? OwinRequestScopeContextOptions.Default.ItemKeyEqualityComparer;

      using (var scopeContext = new OwinRequestScopeContext(
        environment, 
        new OwinRequestScopeContextItems(keyComparer), 
        _options ?? OwinRequestScopeContextOptions.Default)) {
        OwinRequestScopeContext.Current = scopeContext;
        if (_next != null) await _next.Invoke(environment).ConfigureAwait(false);
      }
    }
  }
}