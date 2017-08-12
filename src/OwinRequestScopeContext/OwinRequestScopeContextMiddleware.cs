using System.Threading.Tasks;
using Microsoft.Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public class OwinRequestScopeContextMiddleware : OwinMiddleware {
    public OwinRequestScopeContextMiddleware(OwinMiddleware next) : base(next) { }

    public override async Task Invoke(IOwinContext context) {
      var scopeContext = new OwinRequestScopeContext(context);
      OwinRequestScopeContext.Current = scopeContext;

      try {
        if (Next != null) await Next.Invoke(context).ConfigureAwait(false);
      } finally {
        OwinRequestScopeContext.FreeContextSlot();
      }
    }
  }
}