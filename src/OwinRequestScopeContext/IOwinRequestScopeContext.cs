using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContext : IDisposable {
    IOwinContext OwinContext { get; }
    IDictionary<string, object> Items { get; }
    void RegisterForDisposal(IDisposable disposable);
  }
}