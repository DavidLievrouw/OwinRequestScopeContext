# OwinRequestScopeContext

With this Owin middleware, you can use OwinRequestScopeContext.Current like HttpContext.Current, but without a dependency to System.Web.

It uses CallContext internally. Which means that it is accessible when a request is handled on different threads, and using async/await.
More info in this article: http://odetocode.com/Articles/112.aspx

## Status

Binary package: [![NuGet Status](http://img.shields.io/nuget/v/DavidLievrouw.OwinRequestScopeContext.svg?style=flat-square)](https://www.nuget.org/packages/DavidLievrouw.OwinRequestScopeContext/)

Source-only package: [![NuGet Status](http://img.shields.io/nuget/v/DavidLievrouw.OwinRequestScopeContext.Sources.svg?style=flat-square)](https://www.nuget.org/packages/DavidLievrouw.OwinRequestScopeContext.Sources/)

CI: [![Build status](https://ci.appveyor.com/api/projects/status/ty6o1cw7tm4drqfj?svg=true)](https://ci.appveyor.com/project/DavidLievrouw/owinrequestscopecontext-kbcye)

## Usage for .NET framework
```cs
  public class Startup {
    public void Configuration(IAppBuilder app) {
      // Add it to the owin pipeline
      app.UseRequestScopeContext();
    }
  }
  
  public class RequestStartup {
    public void ExtractMyCustomHeaderValueToRequestContext(IOwinRequest owinRequest) {
      // Set a value in the Items dictionary, available throughout the request
      var requestContext = OwinRequestScopeContext.Current;
      requestContext.Items.Add("MyCustomHeaderValue", owinRequest.Headers["MyCustomHeader"]);
    }
  }
  
  public class MyCustomHeaderValueProvider {
    public string ProvideMyCustomHeaderValue() {
      var requestContext = OwinRequestScopeContext.Current;
      if (requestContext == null) return null;
      if (!requestContext.Items.TryGetValue("MyCustomHeaderValue", out object myCustomHeaderValueObj)) return null;
      return myCustomHeaderValueObj as string;
    }
  }
```

## Usage for .NET core

```cs
public class Startup {
    public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
        app
            .UseOwin(pipeline => pipeline
                // Add it to the pipeline
                .UseRequestScopeContext()
                // Set a value in the Items dictionary, available throughout the request
                .Use(environment => {
                    var requestContext = OwinRequestScopeContext.Current;
                    var requestHeaders = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
                    requestContext.Items.Add("MyCustomHeaderValue", requestHeaders["MyCustomHeader"]);
                    return Task.CompletedTask;
                })
            );
    }
}

public class MyCustomHeaderValueProvider {
    public string ProvideMyCustomHeaderValue() {
        var requestContext = OwinRequestScopeContext.Current;
        if (requestContext == null) return null;
        if (!requestContext.Items.TryGetValue("MyCustomHeaderValue", out object myCustomHeaderValueObj)) return null;
        return myCustomHeaderValueObj as string;
    }
}
```

## Disposable

You can also register IDisposable instances for disposal when the request is completed. 
```cs
  public class LocalStorageManager : IDisposable {
    ...
    public void Dispose() { ... }
  }
  
  public class GlobalStorageManager : IDisposable {
    ...
    public void Dispose() { ... }
  }
  
  public void InitializeLocalStorageManagerForRequest() {
    var requestScopeContext = OwinRequestScopeContext.Current;
  
    // Add item to the request scope context, that will be disposed when the request completes
    var localStorageManager_ToDispose = new LocalStorageManager();
    requestScopeContext.Items.Add("LocalStorageManager_ToDispose", localStorageManager_ToDispose, true);
  
    // Add item to the request scope context, that will not be disposed when the request completes
    var globalStorageManager_NotToBeDisposed = new GlobalStorageManager();
    requestScopeContext.Items.Add("GlobalStorageManager_NotToBeDisposed", globalStorageManager_NotToBeDisposed, false);
  
    // Add some other (non-IDisposable) item
    requestScopeContext.Items.Add("MyNonDisposableObject", 42);
  }
```

Remark: When any .Dispose() call fails, the other registered instances are still disposed. Only afterwards, an AggregateException is thrown.

## Sample projects

In the GitHub repository, there is a [.NET framework sample project](https://github.com/DavidLievrouw/OwinRequestScopeContext/tree/master/src/Sample.NetFramework), and also a  [.NET core sample project](https://github.com/DavidLievrouw/OwinRequestScopeContext/tree/master/src/Sample.NetCore).

## Contribute
<a href="https://www.buymeacoffee.com/DavidLievrouw" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
