using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextMiddlewareFixture {
    Func<IDictionary<string, object>, Task> _next;
    OwinRequestScopeContextMiddleware _sut;

    [SetUp]
    public virtual void SetUp() {
      _next = owinEnvironment => Task.CompletedTask;
      _sut = new OwinRequestScopeContextMiddleware(_next);
    }

    [TestFixture]
    public class Construction : OwinRequestScopeContextMiddlewareFixture {
      [Test]
      public void AllowsNullNext() {
        Action act = () => new OwinRequestScopeContextMiddleware(null);
        act.ShouldNotThrow();
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
        await _sut.Invoke(_owinEnvironment).ConfigureAwait(false);
        OwinRequestScopeContext.Current.Should().BeNull();
      }

      [Test]
      public async Task InvokesNext() {
        var nextHasBeenInvoked = false;
        _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
          nextHasBeenInvoked = true;
          return Task.CompletedTask;
        });
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
        });
        await sut.Invoke(_owinEnvironment).ConfigureAwait(false);

        interceptedOwinEnvironmentInNext.ShouldBeEquivalentTo(_owinEnvironment);
      }

      [Test]
      public void WhenThereIsNoNext_DoesNotCrash() {
        var sut = new OwinRequestScopeContextMiddleware(null);
        Func<Task> act = () => sut.Invoke(_owinEnvironment);
        act.ShouldNotThrow();
      }

      [Test]
      public async Task DisposesContextAtEnd() {
        var disposable = A.Fake<IDisposable>();
        _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
          OwinRequestScopeContext.Current.RegisterForDisposal(disposable);
          return Task.CompletedTask;
        });
        await _sut.Invoke(_owinEnvironment).ConfigureAwait(false);
        A.CallTo(() => disposable.Dispose()).MustHaveHappened();
      }

      [Test]
      public void DisposesContextAtEnd_EvenWhenPipelineFailed() {
        var disposable = A.Fake<IDisposable>();
        var failureInPipeline = new InvalidOperationException("Failure for unit tests");
        _sut = new OwinRequestScopeContextMiddleware(owinEnvironment => {
          OwinRequestScopeContext.Current.RegisterForDisposal(disposable);
          throw failureInPipeline;
        });
        Func<Task> act = () => _sut.Invoke(_owinEnvironment);
        act.ShouldThrow<InvalidOperationException>().Where(_ => _.Equals(failureInPipeline));
        A.CallTo(() => disposable.Dispose()).MustHaveHappened();
      }
    }
  }
}