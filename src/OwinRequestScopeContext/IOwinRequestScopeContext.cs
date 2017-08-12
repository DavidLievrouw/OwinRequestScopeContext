using System.Collections.Generic;
using Microsoft.Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public interface IOwinRequestScopeContext {
    IOwinContext OwinContext { get; }
    IDictionary<string, object> Items { get; }
  }
}