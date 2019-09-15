#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;

namespace DavidLievrouw.OwinRequestScopeContext {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder UseRequestScopeContext(this IApplicationBuilder app, OwinRequestScopeContextOptions options = null) {
            return app.UseOwin(pipeline => {
                pipeline(next => {
                    var middleware = new OwinRequestScopeContextMiddleware(next, options);
                    return middleware.Invoke;
                });
            });
        }
    }
}
#endif