using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Messaging;

namespace DavidLievrouw.OwinRequestScopeContext {
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
      get => (IOwinRequestScopeContext) CallContext.LogicalGetData(CallContextKey);
      internal set => CallContext.LogicalSetData(CallContextKey, value);
    }

    internal OwinRequestScopeContextOptions Options { get; }

    public IReadOnlyDictionary<string, object> OwinEnvironment { get; }

    public IOwinRequestScopeContextItems Items { get; }

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