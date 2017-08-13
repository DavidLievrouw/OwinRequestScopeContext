using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContext : IDisposable {
    IReadOnlyDictionary<string, object> OwinEnvironment { get; }
    IDictionary<string, object> Items { get; }
    void RegisterForDisposal(IDisposable disposable);
  }
}