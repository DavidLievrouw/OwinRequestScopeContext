using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  // Internal helper interface for backward compatibility, to be able to acces the Disposables collection in obsolete members
  internal interface IInternalOwinRequestScopeContextItems : IOwinRequestScopeContextItems {
    ICollection<IDisposable> Disposables { get; }
  }
}