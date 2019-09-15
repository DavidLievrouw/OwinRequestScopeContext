#if NETSTANDARD2_0
using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Builder;

namespace DavidLievrouw.OwinRequestScopeContext {
    [TestFixture]
    public class ApplicationBuilderExtensionsFixture {
        [TestFixture]
        public class UseRequestScopeContext : ApplicationBuilderExtensionsFixture {
            FakeApplicationBuilder _app;

            [SetUp]
            public void SetUp() {
                _app = new FakeApplicationBuilder();
            }

            [Test]
            public async Task CreatesRequestScopeContext() {
                var options = new OwinRequestScopeContextOptions {
                    ItemKeyEqualityComparer = StringComparer.Ordinal
                };
                _app.UseOwin(pipeline => pipeline
                    .Use(environment => {
                        OwinRequestScopeContext.Current.Should().BeNull("No current context should exist before the middleware is called.");
                        return Task.CompletedTask;
                    })
                    .UseRequestScopeContext(options)
                    .Use(environment => {
                        OwinRequestScopeContext.Current.Should().NotBeNull("The middleware should initialize the current context.");
                        return Task.CompletedTask;
                    }));
                var requestDelegate = _app.Build();

                await requestDelegate.Invoke(new DefaultHttpContext());

                OwinRequestScopeContext.Current.Should().BeNull("The current context should be null when not in the context of a request.");
            }

            [Test]
            public async Task UseSpecifiedOptions() {
                var options = new OwinRequestScopeContextOptions {
                    ItemKeyEqualityComparer = StringComparer.Ordinal
                };
                _app.UseOwin(pipeline => pipeline
                    .UseRequestScopeContext(options)
                    .Use(environment => {
                        var typed = OwinRequestScopeContext.Current as OwinRequestScopeContext;
                        typed.Options.Should().Be(options);
                        return Task.CompletedTask;
                    }));
                var requestDelegate = _app.Build();

                await requestDelegate.Invoke(new DefaultHttpContext());
            }

            [Test]
            [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
            public async Task GivenNullOptions_UseDefaultOptions() {
                OwinRequestScopeContextOptions nullOptions = null;
                _app.UseOwin(pipeline => pipeline
                    .UseRequestScopeContext(nullOptions)
                    .Use(environment => {
                        var typed = OwinRequestScopeContext.Current as OwinRequestScopeContext;
                        typed.Options.Should().Be(OwinRequestScopeContextOptions.Default);
                        return Task.CompletedTask;
                    }));
                var requestDelegate = _app.Build();

                await requestDelegate.Invoke(new DefaultHttpContext());
            }

            [Test]
            public async Task GivenNoOptions_UseDefaultOptions() {
                _app.UseOwin(pipeline => pipeline
                    .UseRequestScopeContext()
                    .Use(environment => {
                        var typed = OwinRequestScopeContext.Current as OwinRequestScopeContext;
                        typed.Options.Should().Be(OwinRequestScopeContextOptions.Default);
                        return Task.CompletedTask;
                    }));
                var requestDelegate = _app.Build();

                await requestDelegate.Invoke(new DefaultHttpContext());
            }
        }

        [TestFixture]
        public class Use : ApplicationBuilderExtensionsFixture {
            FakeApplicationBuilder _app;

            [SetUp]
            public void SetUp() {
                _app = new FakeApplicationBuilder();
            }

            [Test]
            public async Task InvokesMiddleware() {
                var invocationCount = 0;
                _app.UseOwin(pipeline => pipeline
                    .Use(environment => {
                        invocationCount++;
                        return Task.CompletedTask;
                    }));
                var requestDelegate = _app.Build();

                await requestDelegate.Invoke(new DefaultHttpContext());

                invocationCount.Should().Be(1);
            }

            [Test]
            public void GivenNullMiddleware_ThrowsArgumentNullException() {
                Action act = () => _app.UseOwin(pipeline => pipeline.Use(null));
                act.Should().Throw<ArgumentNullException>();
            }
            
            [Test]
            public async Task DecoratesNextMiddleware() {
                _app.UseOwin(pipeline => pipeline
                    .Use(environment => {
                        _app.HasInvokedInnermostMiddleware.Should().BeFalse();
                        return Task.CompletedTask;
                    }));
                var requestDelegate = _app.Build();

                await requestDelegate.Invoke(new DefaultHttpContext());
                
                _app.HasInvokedInnermostMiddleware.Should().BeTrue();
            }
        }

        public class FakeApplicationBuilder : IApplicationBuilder {
            readonly IList<Func<RequestDelegate, RequestDelegate>> _components = new List<Func<RequestDelegate, RequestDelegate>>();
        
            public IFeatureCollection ServerFeatures { get; }
            public IDictionary<string, object> Properties { get; }

            public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) {
                _components.Add(middleware);
                return this;
            }

            public IApplicationBuilder New() {
                throw new NotImplementedException();
            }

            public bool HasInvokedInnermostMiddleware { get; set; }

            public RequestDelegate Build() {
                Task App(HttpContext context) {
                    HasInvokedInnermostMiddleware = true;
                    return Task.CompletedTask;
                }

                return _components.Reverse().Aggregate((RequestDelegate) App, (current, component) => component(current));
            }

            public RequestDelegate Build(RequestDelegate requestDelegate) {
                return _components.Reverse().Aggregate(requestDelegate, (current, component) => component(current));
            }

            public IServiceProvider ApplicationServices { get; set; }

            T GetProperty<T>(string key) {
                object value;
                return Properties.TryGetValue(key, out value) ? (T) value : default;
            }

            void SetProperty<T>(string key, T value) {
                Properties[key] = value;
            }
        }
    }
}
#endif