using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DavidLievrouw.OwinRequestScopeContext;

namespace Sample {
  [RoutePrefix("")]
  public class HomeController : ApiController {
    [Route("")]
    public async Task<HttpResponseMessage> GetHome() {
      await Task.Delay(500).ConfigureAwait(false);

      var myDisposableObject = OwinRequestScopeContext.Current.Items["MyDisposableObject"] as MyDisposableObject;

      var res = Request.CreateResponse(HttpStatusCode.OK);
      res.Content = new StringContent(myDisposableObject?.Value, Encoding.UTF8, "text/plain");
      return res;
    }
  }
}