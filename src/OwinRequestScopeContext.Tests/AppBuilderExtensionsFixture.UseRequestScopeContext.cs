#if NETFRAMEWORK
using System;
using FakeItEasy;
using NUnit.Framework;
using Owin;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class AppBuilderExtensionsFixture {
    [TestFixture]
    public class UseRequestScopeContext : AppBuilderExtensionsFixture {
      IAppBuilder _app;

      [SetUp]
      public void SetUp() {
        _app = A.Fake<IAppBuilder>();
      }

      [Test]
      public void UseExpectedMiddlewareWithSpecifiedOptions() {
        var options = new OwinRequestScopeContextOptions {
          ItemKeyEqualityComparer = StringComparer.Ordinal
        };
        _app.UseRequestScopeContext(options);
        A.CallTo(() => _app.Use(typeof(OwinRequestScopeContextMiddleware), options)).MustHaveHappened();
      }

      [Test]
      public void GivenNullOptions_UseExpectedMiddlewareWithDefaultOptions() {
        _app.UseRequestScopeContext(null);
        A.CallTo(() => _app.Use(typeof(OwinRequestScopeContextMiddleware), OwinRequestScopeContextOptions.Default))
          .MustHaveHappened();
      }

      [Test]
      public void GivenNoOptions_UseExpectedMiddlewareWithDefaultOptions() {
        _app.UseRequestScopeContext();
        A.CallTo(() => _app.Use(typeof(OwinRequestScopeContextMiddleware), OwinRequestScopeContextOptions.Default))
          .MustHaveHappened();
      }
    }
  }
}
#endif