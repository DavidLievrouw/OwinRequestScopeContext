using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContext : IDisposable {
    IReadOnlyDictionary<string, object> OwinEnvironment { get; }
    IOwinRequestScopeContextItems Items { get; }

    [Obsolete("Please use Items.Add(string key, IDisposable disposable, bool disposeWhenRequestIsCompleted).")]
    void RegisterForDisposal(IDisposable disposable);
  }
}