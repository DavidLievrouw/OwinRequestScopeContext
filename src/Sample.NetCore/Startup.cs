using System.Text;
using DavidLievrouw.OwinRequestScopeContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sample {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) { }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app
                .UseOwin(pipeline => pipeline
                    .Use(new ValidateThatThereIsNoCurrentRequestContextBeforeRequestMiddleware().Invoke)
                    .UseRequestScopeContext()
                    .Use(new InitializeMyDisposableObjectMiddleware().Invoke)
                )
                .Run(async context => {
                    // Read data that was set during the request
                    var myDisposableObject_ToBeDisposed = OwinRequestScopeContext.Current.Items["MyDisposableObject_ToBeDisposed"] as MyDisposableObject;
                    var myDisposableObject_NotToBeDisposed = OwinRequestScopeContext.Current.Items["MyDisposableObject_NotToBeDisposed"] as MyDisposableObject;
                    var myNonDisposableObject = OwinRequestScopeContext.Current.Items["MyNonDisposableObject"];
                    var value = myDisposableObject_ToBeDisposed?.Value + "\n" + myDisposableObject_NotToBeDisposed?.Value + "\n" + myNonDisposableObject;

                    await context.Response.WriteAsync(value, Encoding.UTF8);
                });
        }
    }
}