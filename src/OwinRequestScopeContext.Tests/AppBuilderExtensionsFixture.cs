#if NETFRAMEWORK
using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Owin;
using FakeItEasy;

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
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public void GivenNullOptions_UseDefaultOptions() {
                OwinRequestScopeContextOptions nullOptions = null;
                _app.UseRequestScopeContext(nullOptions);
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