using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
    [TestFixture]
    public class OwinRequestScopeContextMiddlewareFixture {
        Func<IDictionary<string, object>, Task> _next;
        OwinRequestScopeContextOptions _options;
        OwinRequestScopeContextMiddleware _sut;

        [SetUp]
        public virtual void SetUp() {
            _next = owinEnvironment => Task.CompletedTask;
            _options = new OwinRequestScopeContextOptions {
                ItemKeyEqualityComparer = StringComparer.OrdinalIgnoreCase
            };
            _sut = new OwinRequestScopeContextMiddleware(_next, _options);
        }

        [TestFixture]
        public class Construction : OwinRequestScopeContextMiddlewareFixture {
            [Test]
            public void AllowsNullNext() {
                Action act = () => new OwinRequestScopeContextMiddleware(null, _options);
                act.Should().NotThrow();
            }

            [Test]
            public void AllowsNullOptions() {
                Action act = () => new OwinRequestScopeContextMiddleware(_next, null);
                act.Should().NotThrow();
            }
        }

        [TestFixture]
        public class Invoke : OwinRequestScopeContextMiddlewareFixture {
            IDictionary<string, object> _owinEnvironment;

            [SetUp]
            public override void SetUp() {
                base.SetUp();
                _owinEnvironment = new Dictionary<string, object> {{"the meaning of life, the universe, and everything", 42}};
            }

            [Test]
            public async Task FreesContextSlotAfterwards() {
                _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    OwinRequestScopeContext.Current.Should().NotBeNull();
                    return Task.CompletedTask;
                }, _options);
                await _sut.Invoke(_owinEnvironment).ConfigureAwait(false);
                OwinRequestScopeContext.Current.Should().BeNull();
            }

            [Test]
            public async Task InvokesNext() {
                var nextHasBeenInvoked = false;
                _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    nextHasBeenInvoked = true;
                    return Task.CompletedTask;
                }, _options);
                await _sut.Invoke(_owinEnvironment).ConfigureAwait(false);
                nextHasBeenInvoked.Should().BeTrue();
            }

            [Test]
            public async Task SetsNewOwinRequestScopeContext() {
                IReadOnlyDictionary<string, object> interceptedOwinEnvironmentInNext = null;

                var sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    interceptedOwinEnvironmentInNext = OwinRequestScopeContext.Current.OwinEnvironment;
                    OwinRequestScopeContext.Current.Should().NotBeNull();
                    return Task.CompletedTask;
                }, _options);
                await sut.Invoke(_owinEnvironment).ConfigureAwait(false);

                interceptedOwinEnvironmentInNext.Should().BeEquivalentTo(_owinEnvironment);
            }

            [Test]
            public void WhenThereIsNoNext_DoesNotCrash() {
                var sut = new OwinRequestScopeContextMiddleware(null, _options);
                Func<Task> act = () => sut.Invoke(_owinEnvironment);
                act.Should().NotThrow();
            }

            [Test]
            public void WhenThereIsAnOwinRequestScopeContextAlready_ThrowsInvalidOperationException() {
                _sut = new OwinRequestScopeContextMiddleware(
                    async owinEnvironment => { await new OwinRequestScopeContextMiddleware(null, _options).Invoke(owinEnvironment).ConfigureAwait(false); }, _options);
                Func<Task> act = () => _sut.Invoke(_owinEnvironment);
                act.Should().Throw<InvalidOperationException>();
            }

            [Test]
            public async Task GivenNullOptions_InitializesContextWithDefaultOptions() {
                OwinRequestScopeContextOptions interceptedOptions = null;
                IEqualityComparer<string> interceptedKeyComparer = null;
                var sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    interceptedOptions = ((OwinRequestScopeContext) OwinRequestScopeContext.Current).Options;
                    interceptedKeyComparer = ((OwinRequestScopeContextItems) OwinRequestScopeContext.Current.Items).KeyComparer;
                    return Task.CompletedTask;
                }, null);
                await sut.Invoke(_owinEnvironment).ConfigureAwait(false);
                interceptedOptions.Should().NotBeNull();
                interceptedOptions.ItemKeyEqualityComparer.Should().Be(OwinRequestScopeContextOptions.Default.ItemKeyEqualityComparer);
                interceptedKeyComparer.Should().NotBeNull();
                interceptedKeyComparer.Should().Be(OwinRequestScopeContextOptions.Default.ItemKeyEqualityComparer);
            }

            [Test]
            public async Task GivenOptions_InitializesContextWithGivenOptions() {
                OwinRequestScopeContextOptions interceptedOptions = null;
                IEqualityComparer<string> interceptedKeyComparer = null;
                var sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
                    interceptedOptions = ((OwinRequestScopeContext) OwinRequestScopeContext.Current).Options;
                    interceptedKeyComparer = ((OwinRequestScopeContextItems) OwinRequestScopeContext.Current.Items).KeyComparer;
                    return Task.CompletedTask;
                }, _options);
                await sut.Invoke(_owinEnvironment).ConfigureAwait(false);
                interceptedOptions.Should().NotBeNull();
                interceptedOptions.ItemKeyEqualityComparer.Should().Be(_options.ItemKeyEqualityComparer);
                interceptedKeyComparer.Should().NotBeNull();
                interceptedKeyComparer.Should().Be(_options.ItemKeyEqualityComparer);
            }
        }
    }
}