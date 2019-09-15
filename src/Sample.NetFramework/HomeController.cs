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

            // Read data that was set during the request
            var myDisposableObject_ToBeDisposed = OwinRequestScopeContext.Current.Items["MyDisposableObject_ToBeDisposed"] as MyDisposableObject;
            var myDisposableObject_NotToBeDisposed = OwinRequestScopeContext.Current.Items["MyDisposableObject_NotToBeDisposed"] as MyDisposableObject;
            var myNonDisposableObject = OwinRequestScopeContext.Current.Items["MyNonDisposableObject"];
            var value = myDisposableObject_ToBeDisposed?.Value + "\n" + myDisposableObject_NotToBeDisposed?.Value + "\n" + myNonDisposableObject;

            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.Content = new StringContent(value, Encoding.UTF8, "text/plain");
            return res;
        }
    }
}