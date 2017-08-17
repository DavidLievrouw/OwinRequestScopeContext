using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  internal interface IInternalOwinRequestScopeContextItems : IOwinRequestScopeContextItems {
    IDictionary<string, object> InnerDictionary { get; }
    IEqualityComparer<string> KeyComparer { get; }
    ICollection<IDisposable> Disposables { get; }
  }
}