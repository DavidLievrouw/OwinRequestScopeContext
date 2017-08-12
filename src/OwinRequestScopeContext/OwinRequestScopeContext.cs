using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public class OwinRequestScopeContext : IOwinRequestScopeContext {
    const string CallContextKey = "dl.owin.rscopectx";
    readonly List<IDisposable> _disposables;

    public OwinRequestScopeContext(IOwinContext owinContext) {
      OwinContext = owinContext;
      Items = new ConcurrentDictionary<string, object>();
      _disposables = new List<IDisposable>();
    }

    public static IOwinRequestScopeContext Current {
      get => (IOwinRequestScopeContext) CallContext.LogicalGetData(CallContextKey);
      set => CallContext.LogicalSetData(CallContextKey, value);
    }

    internal IEnumerable<IDisposable> Disposables => _disposables;

    public IOwinContext OwinContext { get; }

    public IDictionary<string, object> Items { get; }

    public void RegisterForDisposal(IDisposable disposable) {
      if (disposable == null) throw new ArgumentNullException(paramName: nameof(disposable));
      _disposables.Add(disposable);
    }

    public void Dispose() {
      var disposalExceptions = new List<Exception>();

      Disposables.ForEach(disposable => {
        try {
          disposable.Dispose();
        }
        catch (Exception ex) {
          disposalExceptions.Add(ex);
        }
      });

      if (disposalExceptions.Any())
        throw new AggregateException(
          "One or more exception occurred while disposing items that were registered for disposal.",
          disposalExceptions);
    }

    internal static void FreeContextSlot() {
      CallContext.FreeNamedDataSlot(CallContextKey);
    }
  }
}