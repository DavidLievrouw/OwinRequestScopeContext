using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContext : IDisposable {
    IReadOnlyDictionary<string, object> OwinEnvironment { get; }
    IOwinRequestScopeContextItems Items { get; }
    object this[string key] { get; }
  }
}