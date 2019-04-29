using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContextItems : IDictionary<string, object>, IDisposable {
    void Add(string key, IDisposable value, bool disposeWhenRequestIsCompleted);
  }
}