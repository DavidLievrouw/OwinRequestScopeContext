using Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public static class AppBuilderExtensions {
    public static IAppBuilder UseRequestScopeContext(this IAppBuilder app) {
      return app.Use(typeof(OwinRequestScopeContextMiddleware));
    }
  }
}