using System.Threading.Tasks;
using Microsoft.Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  internal class OwinRequestScopeContextMiddleware : OwinMiddleware {
    public OwinRequestScopeContextMiddleware(OwinMiddleware next) : base(next) { }

    public override async Task Invoke(IOwinContext context) {
      using (var scopeContext = new OwinRequestScopeContext(context)) {
        OwinRequestScopeContext.Current = scopeContext;
        if (Next != null) await Next.Invoke(context).ConfigureAwait(false);
      }
    }
  }
}