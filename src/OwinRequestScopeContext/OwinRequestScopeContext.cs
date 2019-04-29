using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DavidLievrouw.OwinRequestScopeContext
{
    public class OwinRequestScopeContext : IOwinRequestScopeContext {
    const string CallContextKey = "dl.owin.rscopectx";

    internal OwinRequestScopeContext(
      IDictionary<string, object> owinEnvironment,
      IOwinRequestScopeContextItems items,
      OwinRequestScopeContextOptions options) {
      if (items == null) throw new ArgumentNullException(paramName: nameof(items));
      if (options == null) throw new ArgumentNullException(paramName: nameof(options));

      Items = items;
      Options = options;

      OwinEnvironment = new ReadOnlyDictionary<string, object>(owinEnvironment);
    }

    public static IOwinRequestScopeContext Current {
      get => (IOwinRequestScopeContext) CallContext.GetData(CallContextKey);
      internal set => CallContext.SetData(CallContextKey, value);
    }

    internal OwinRequestScopeContextOptions Options { get; }

    public IReadOnlyDictionary<string, object> OwinEnvironment { get; }

    public IOwinRequestScopeContextItems Items { get; }

    public object this[string key] {
      get {
        if (key == null) throw new ArgumentNullException(paramName: nameof(key));
        return Items.TryGetValue(key, out object value) ? value : null;
      }
    }

    public void RegisterForDisposal(IDisposable disposable) {
      if (disposable == null) throw new ArgumentNullException(paramName: nameof(disposable));
      ((IInternalOwinRequestScopeContextItems)Items).Disposables.Add(disposable);
    }

    public void Dispose() {
      CallContext.FreeNamedDataSlot(CallContextKey);
      Items.Dispose();
    }
  }
}