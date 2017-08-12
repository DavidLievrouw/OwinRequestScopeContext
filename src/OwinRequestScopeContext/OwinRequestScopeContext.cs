using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Microsoft.Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public class OwinRequestScopeContext : IOwinRequestScopeContext {
    const string CallContextKey = "ug.owin.rscopectx";

    public OwinRequestScopeContext(IOwinContext owinContext) {
      OwinContext = owinContext;
      Items = new ConcurrentDictionary<string, object>();
    }

    public IOwinContext OwinContext { get; }

    public static IOwinRequestScopeContext Current {
      get => (IOwinRequestScopeContext)CallContext.LogicalGetData(CallContextKey);
      set => CallContext.LogicalSetData(CallContextKey, value);
    }

    internal static void FreeContextSlot() {
      CallContext.FreeNamedDataSlot(CallContextKey);
    }

    public IDictionary<string, object> Items { get; }
  }
}