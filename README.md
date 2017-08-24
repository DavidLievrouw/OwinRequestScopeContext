# OwinRequestScopeContext

With this Owin middleware, you can use OwinRequestScopeContext.Current like HttpContext.Current, but without a dependency to System.Web.

It uses CallContext internally. Which means that it is accessible when a request is handled on different threads, and using async/await.
More info in this article: http://odetocode.com/Articles/112.aspx

## NuGet

Binary package: [![NuGet Status](http://img.shields.io/nuget/v/DavidLievrouw.OwinRequestScopeContext.svg?style=flat-square)](https://www.nuget.org/packages/DavidLievrouw.OwinRequestScopeContext/)

Source-only package: [![NuGet Status](http://img.shields.io/nuget/v/DavidLievrouw.OwinRequestScopeContext.Sources.svg?style=flat-square)](https://www.nuget.org/packages/DavidLievrouw.OwinRequestScopeContext.Sources/)

## Example usage
```cs
  public class Startup {
    public void Configuration(IAppBuilder app) {
      // Add it to the owin pipeline
      app.UseRequestScopeContext();
    }
  }
  
  public class RequestStartup {
    public void ExtractMyCustomHeaderValueToRequestContext(IOwinRequest owinRequest) {
      // Set a value in the Items dictionary, available througout the request
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
  
  public class RequestStartup {
    public void InitializeLocalStorageManagerForRequest() {
      var requestScopeContext = OwinRequestScopeContext.Current;

      // Add item to the request scope context, that will be disposed when the requests completes
      var localStorageManager_ToDispose = new LocalStorageManager();
      requestScopeContext.Items.Add("LocalStorageManager_ToDispose", localStorageManager_ToDispose, true);

      // Add item to the request scope context, that will not be disposed when the requests completes
      var globalStorageManager_NotToBeDisposed = new GlobalStorageManager();
      requestScopeContext.Items.Add("GlobalStorageManager_NotToBeDisposed", globalStorageManager_NotToBeDisposed, false);

      // Add some other (non-IDisposable) item
      requestScopeContext.Items.Add("MyNonDisposableObject", 42);
    }
  }
```

Remark: When any .Dispose() call fails, the other registered instances are still disposed. Only afterwards, an AggregateException is thrown.

## Sample project

In the GitHub repository, there is a [sample project](https://github.com/DavidLievrouw/OwinRequestScopeContext/tree/master/src/Sample).

## Change log

v2.1.0 - 2017-08-24
- Add a more explicit way of adding items to the request scope that need to be disposed.
- Mark the old way of registering for disposal as obsolete.
- Add indexer to OwinRequestScopeContext.
- Update sample and README.

v2.0.0 - 2017-08-13
- Make OwinRequestScopeContext.Current setter internal.
- Changes to v1.1.0 are actually breaking changes, so major version bump needed.

v1.1.0 - 2017-08-13
- Remove dependency to Microsoft.Owin package.
- OwinContext is no longer a member of IOwinRequestScopeContext. It is replaced by OwinEnvironment.
- OwinRequestScopeContext.Current is no longer internal.

v1.0.0 - 2017-08-12
- Initial release (binary and sources).