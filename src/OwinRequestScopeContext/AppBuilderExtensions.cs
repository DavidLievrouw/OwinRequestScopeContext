using Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  public static class AppBuilderExtensions {
    public static IAppBuilder UseRequestScopeContext(this IAppBuilder app, OwinRequestScopeContextOptions options = null) {
      return app.Use(typeof(OwinRequestScopeContextMiddleware), options ?? OwinRequestScopeContextOptions.Default);
    }
  }
}