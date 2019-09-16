#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Xunit;
using FluentAssertions;

namespace DavidLievrouw.OwinRequestScopeContext {
    public class ApplicationBuilderExtensionsFixture {
        public class UseRequestScopeContext : ApplicationBuilderExtensionsFixture {
            readonly FakeApplicationBuilder _app;

            public UseRequestScopeContext() {
                _app = new FakeApplicationBuilder();
            }

            [Fact]
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

            [Fact]
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

            [Fact]
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

            [Fact]
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

        public class Use : ApplicationBuilderExtensionsFixture {
            readonly FakeApplicationBuilder _app;

            public Use() {
                _app = new FakeApplicationBuilder();
            }

            [Fact]
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

            [Fact]
            public void GivenNullMiddleware_ThrowsArgumentNullException() {
                Action act = () => _app.UseOwin(pipeline => pipeline.Use(null));
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
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

            public bool HasInvokedInnermostMiddleware { get; set; }

            public IFeatureCollection ServerFeatures { get; }
            public IDictionary<string, object> Properties { get; }

            public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) {
                _components.Add(middleware);
                return this;
            }

            public IApplicationBuilder New() {
                throw new NotImplementedException();
            }

            public RequestDelegate Build() {
                Task App(HttpContext context) {
                    HasInvokedInnermostMiddleware = true;
                    return Task.CompletedTask;
                }

                return _components.Reverse().Aggregate((RequestDelegate) App, (current, component) => component(current));
            }

            public IServiceProvider ApplicationServices { get; set; }

            public RequestDelegate Build(RequestDelegate requestDelegate) {
                return _components.Reverse().Aggregate(requestDelegate, (current, component) => component(current));
            }

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