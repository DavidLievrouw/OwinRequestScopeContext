using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContextItems : IDictionary<string, object>, IDisposable {
    [Obsolete("Use another overload of Add e.g. Add(string key, object value) or Add(string key, IDisposable disposable, bool disposeWhenRequestIsCompleted).")]
    new void Add(KeyValuePair<string, object> value);

    [Obsolete("Use another overload of Add that indicates whether the value should be disposed when the request is complete: Add(string key, IDisposable disposable, bool disposeWhenRequestIsCompleted).")]
    void Add(string key, IDisposable value);

    void Add(string key, IDisposable value, bool disposeWhenRequestIsCompleted);
  }
}