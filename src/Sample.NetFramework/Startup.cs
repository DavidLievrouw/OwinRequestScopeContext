using System.Web.Http;
using DavidLievrouw.OwinRequestScopeContext;
using Microsoft.Owin;
using Owin;
using Sample;

[assembly: OwinStartup(typeof(Startup))]

namespace Sample {
    public class Startup {
        public static void Configuration(IAppBuilder app) {
            var httpConfiguration = CreateHttpConfiguration();
            app
                .Use(typeof(ValidateThatThereIsNoCurrentRequestContextBeforeRequestMiddleware))
                .UseRequestScopeContext()
                .Use(typeof(InitializeMyDisposableObjectMiddleware))
                .UseWebApi(httpConfiguration);
        }

        public static HttpConfiguration CreateHttpConfiguration() {
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.MapHttpAttributeRoutes();
            return httpConfiguration;
        }
    }
}