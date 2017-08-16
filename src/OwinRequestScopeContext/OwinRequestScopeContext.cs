using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace DavidLievrouw.OwinRequestScopeContext {
  public class OwinRequestScopeContext : IOwinRequestScopeContext {
    const string CallContextKey = "dl.owin.rscopectx";
    readonly List<IDisposable> _disposables;

    internal OwinRequestScopeContext(IDictionary<string, object> owinEnvironment, OwinRequestScopeContextOptions options) {
      if (options == null) throw new ArgumentNullException(paramName: nameof(options));
      Options = options;

      Items = new OwinRequestScopeContextItems(options);

      OwinEnvironment = new ReadOnlyDictionary<string, object>(owinEnvironment);
      _disposables = new List<IDisposable>();
    }

    public static IOwinRequestScopeContext Current {
      get => (IOwinRequestScopeContext) CallContext.LogicalGetData(CallContextKey);
      internal set => CallContext.LogicalSetData(CallContextKey, value);
    }

    internal IEnumerable<IDisposable> Disposables => _disposables;
    internal OwinRequestScopeContextOptions Options { get; }

    public IReadOnlyDictionary<string, object> OwinEnvironment { get; }

    public IOwinRequestScopeContextItems Items { get; }

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

      CallContext.FreeNamedDataSlot(CallContextKey);

      if (disposalExceptions.Any()) {
        throw new AggregateException("One or more exception occurred while disposing items that were registered for disposal.", disposalExceptions);
      }
    }
  }
}