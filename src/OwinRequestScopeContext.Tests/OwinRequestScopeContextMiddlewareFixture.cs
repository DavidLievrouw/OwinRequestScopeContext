﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DavidLievrouw.OwinRequestScopeContext {
    public class OwinRequestScopeContextMiddlewareFixture {
        readonly Func<IDictionary<string, object>, Task> _next;
        readonly OwinRequestScopeContextOptions _options;
        OwinRequestScopeContextMiddleware _sut;

        public OwinRequestScopeContextMiddlewareFixture() {
            _next = owinEnvironment => Task.CompletedTask;
            _options = new OwinRequestScopeContextOptions {
                ItemKeyEqualityComparer = StringComparer.OrdinalIgnoreCase
            };
            _sut = new OwinRequestScopeContextMiddleware(_next, _options);
        }

        public class Construction : OwinRequestScopeContextMiddlewareFixture {
            [Fact]
            public void AllowsNullNext() {
                Action act = () => new OwinRequestScopeContextMiddleware(null, _options);
                act.Should().NotThrow();
            }

            [Fact]
            public void AllowsNullOptions() {
                Action act = () => new OwinRequestScopeContextMiddleware(_next, null);
                act.Should().NotThrow();
            }
        }

        public class Invoke : OwinRequestScopeContextMiddlewareFixture {
            readonly IDictionary<string, object> _owinEnvironment;

            public Invoke() {
                _owinEnvironment = new Dictionary<string, object> {{"the meaning of life, the universe, and everything", 42}};
            }

            [Fact]
            public async Task FreesContextSlotAfterwards() {
                _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    OwinRequestScopeContext.Current.Should().NotBeNull();
                    return Task.CompletedTask;
                }, _options);
                await _sut.Invoke(_owinEnvironment);
                OwinRequestScopeContext.Current.Should().BeNull();
            }

            [Fact]
            public async Task InvokesNext() {
                var nextHasBeenInvoked = false;
                _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    nextHasBeenInvoked = true;
                    return Task.CompletedTask;
                }, _options);
                await _sut.Invoke(_owinEnvironment);
                nextHasBeenInvoked.Should().BeTrue();
            }

            [Fact]
            public async Task SetsNewOwinRequestScopeContext() {
                IReadOnlyDictionary<string, object> interceptedOwinEnvironmentInNext = null;

                var sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    interceptedOwinEnvironmentInNext = OwinRequestScopeContext.Current.OwinEnvironment;
                    OwinRequestScopeContext.Current.Should().NotBeNull();
                    return Task.CompletedTask;
                }, _options);
                await sut.Invoke(_owinEnvironment);

                interceptedOwinEnvironmentInNext.Should().BeEquivalentTo(_owinEnvironment);
            }

            [Fact]
            public void WhenThereIsNoNext_DoesNotCrash() {
                var sut = new OwinRequestScopeContextMiddleware(null, _options);
                Func<Task> act = () => sut.Invoke(_owinEnvironment);
                act.Should().NotThrow();
            }

            [Fact]
            public void WhenThereIsAnOwinRequestScopeContextAlready_ThrowsInvalidOperationException() {
                _sut = new OwinRequestScopeContextMiddleware(
                    async owinEnvironment => { await new OwinRequestScopeContextMiddleware(null, _options).Invoke(owinEnvironment); }, _options);
                Func<Task> act = () => _sut.Invoke(_owinEnvironment);
                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public async Task GivenNullOptions_InitializesContextWithDefaultOptions() {
                OwinRequestScopeContextOptions interceptedOptions = null;
                IEqualityComparer<string> interceptedKeyComparer = null;
                var sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    interceptedOptions = ((OwinRequestScopeContext) OwinRequestScopeContext.Current).Options;
                    interceptedKeyComparer = ((OwinRequestScopeContextItems) OwinRequestScopeContext.Current.Items).KeyComparer;
                    return Task.CompletedTask;
                }, null);
                await sut.Invoke(_owinEnvironment);
                interceptedOptions.Should().NotBeNull();
                interceptedOptions.ItemKeyEqualityComparer.Should().Be(OwinRequestScopeContextOptions.Default.ItemKeyEqualityComparer);
                interceptedKeyComparer.Should().NotBeNull();
                interceptedKeyComparer.Should().Be(OwinRequestScopeContextOptions.Default.ItemKeyEqualityComparer);
            }

            [Fact]
            public async Task GivenOptions_InitializesContextWithGivenOptions() {
                OwinRequestScopeContextOptions interceptedOptions = null;
                IEqualityComparer<string> interceptedKeyComparer = null;
                var sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    interceptedOptions = ((OwinRequestScopeContext) OwinRequestScopeContext.Current).Options;
                    interceptedKeyComparer = ((OwinRequestScopeContextItems) OwinRequestScopeContext.Current.Items).KeyComparer;
                    return Task.CompletedTask;
                }, _options);
                await sut.Invoke(_owinEnvironment);
                interceptedOptions.Should().NotBeNull();
                interceptedOptions.ItemKeyEqualityComparer.Should().Be(_options.ItemKeyEqualityComparer);
                interceptedKeyComparer.Should().NotBeNull();
                interceptedKeyComparer.Should().Be(_options.ItemKeyEqualityComparer);
            }
        }
    }
}