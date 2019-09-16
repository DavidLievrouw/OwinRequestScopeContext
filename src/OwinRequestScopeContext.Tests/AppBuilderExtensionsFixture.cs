#if NETFRAMEWORK
using System;
using System.Diagnostics.CodeAnalysis;
using Owin;
using Xunit;
using FakeItEasy;

namespace DavidLievrouw.OwinRequestScopeContext {
    public class AppBuilderExtensionsFixture {
        public class UseRequestScopeContext : AppBuilderExtensionsFixture {
            readonly IAppBuilder _app;

            public UseRequestScopeContext() {
                _app = A.Fake<IAppBuilder>();
            }

            [Fact]
            public void UsesExpectedMiddlewareWithSpecifiedOptions() {
                var options = new OwinRequestScopeContextOptions {
                    ItemKeyEqualityComparer = StringComparer.Ordinal
                };
                _app.UseRequestScopeContext(options);
                A.CallTo(() => _app.Use(typeof(OwinRequestScopeContextMiddleware), options)).MustHaveHappened();
            }

            [Fact]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullOptions_UsesDefaultOptions() {
                OwinRequestScopeContextOptions nullOptions = null;
                _app.UseRequestScopeContext(nullOptions);
                A.CallTo(() => _app.Use(typeof(OwinRequestScopeContextMiddleware), OwinRequestScopeContextOptions.Default))
                    .MustHaveHappened();
            }

            [Fact]
            public void GivenNoOptions_UsesDefaultOptions() {
                _app.UseRequestScopeContext();
                A.CallTo(() => _app.Use(typeof(OwinRequestScopeContextMiddleware), OwinRequestScopeContextOptions.Default))
                    .MustHaveHappened();
            }
        }
    }
}
#endif