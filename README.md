# OwinRequestScopeContext

With this Owin middleware, you can use OwinRequestScopeContext.Current like HttpContext.Current, but without a dependency to System.Web.

## Example usage
```cs
  public class Startup {
    public void Configuration(IAppBuilder app) {
	  // Add it to the owin pipeline
      app.UseRequestScopeContext();
    }
  }
  
  public class RequestParametersFromNancyRequestConfigurator {
    public void Configure(Nancy.Request nancyRequest) {
	  // Set a value in the Items dictionary, available througout the request
      var requestContext = OwinRequestScopeContext.Current;
      requestContext.Items["MyCustomHeaderValue"] = nancyRequest.Headers["MyCustomHeader"].FirstOrDefault();
    }
  }
  
  public class RequestParametersProviderFromOwinRequestScope {
    public string ProvideMyCustomHeaderValue() {
      var requestContext = OwinRequestScopeContext.Current;
      if (requestContext == null) return null;
      if (!requestContext.Items.TryGetValue("MyCustomHeaderValue", out string myCustomHeaderValue)) return null;
      return myCustomHeaderValue as string;
    }
  }
```

## NuGet
Url: TBA

## Change log

v1.0.0 - 2017-08-12
- Initial release.