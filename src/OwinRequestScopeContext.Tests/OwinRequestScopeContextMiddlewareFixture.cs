using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Owin;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextMiddlewareFixture {
    FakeMiddleware _next;
    OwinRequestScopeContextMiddleware _sut;

    [SetUp]
    public virtual void SetUp() {
      _next = new FakeMiddleware(null, () => { });
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
      IOwinContext _owinContext;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _owinContext = A.Fake<IOwinContext>();
      }

      [Test]
      public async Task FreesContextSlotAfterwards() {
        await _sut.Invoke(_owinContext).ConfigureAwait(false);
        OwinRequestScopeContext.Current.Should().BeNull();
      }

      [Test]
      public async Task InvokesNext() {
        await _sut.Invoke(_owinContext).ConfigureAwait(false);
        _next.InvokedWith.ShouldBeEquivalentTo(expectation: new[] {_owinContext});
      }

      [Test]
      public async Task SetsNewOwinRequestScopeContext() {
        var sut = new OwinRequestScopeContextMiddleware(
          next: new OwinRequestScopeContextIsPresentValidatingMiddleware(null));
        await sut.Invoke(_owinContext).ConfigureAwait(false);
      }

      [Test]
      public void WhenThereIsNoNext_DoesNotCrash() {
        var sut = new OwinRequestScopeContextMiddleware(null);
        Func<Task> act = () => sut.Invoke(_owinContext);
        act.ShouldNotThrow();
      }

      [Test]
      public async Task DisposesContextAtEnd() {
        var disposable = A.Fake<IDisposable>();
        _next = new FakeMiddleware(null, () => OwinRequestScopeContext.Current.RegisterForDisposal(disposable));
        _sut = new OwinRequestScopeContextMiddleware(_next);
        await _sut.Invoke(_owinContext).ConfigureAwait(false);
        A.CallTo(() => disposable.Dispose()).MustHaveHappened();
      }

      [Test]
      public void DisposesContextAtEnd_EvenWhenPipelineFailed() {
        var disposable = A.Fake<IDisposable>();
        var failureInPipeline = new InvalidOperationException("Failure for unit tests");
        _next = new FakeMiddleware(null, () => {
          OwinRequestScopeContext.Current.RegisterForDisposal(disposable);
          throw failureInPipeline;
        });
        _sut = new OwinRequestScopeContextMiddleware(_next);
        Func<Task> act = () => _sut.Invoke(_owinContext);
        act.ShouldThrow<InvalidOperationException>().Where(_ => _.Equals(failureInPipeline));
        A.CallTo(() => disposable.Dispose()).MustHaveHappened();
      }

      class OwinRequestScopeContextIsPresentValidatingMiddleware : OwinMiddleware {
        public OwinRequestScopeContextIsPresentValidatingMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context) {
          OwinRequestScopeContext.Current.Should().NotBeNull().And
            .Match<IOwinRequestScopeContext>(_ => _.OwinContext.Equals(context));
          if (Next != null) await Next.Invoke(context).ConfigureAwait(false);
        }
      }
    }

    class FakeMiddleware : OwinMiddleware {
      readonly Action _actionWhenInvoked;

      public FakeMiddleware(OwinMiddleware next, Action actionWhenInvoked) : base(next) {
        _actionWhenInvoked = actionWhenInvoked;
        InvokedWith = new List<IOwinContext>();
      }

      public IList<IOwinContext> InvokedWith { get; }

      public override async Task Invoke(IOwinContext context) {
        InvokedWith.Add(context);
        _actionWhenInvoked?.Invoke();
        if (Next != null) await Next.Invoke(context).ConfigureAwait(false);
      }
    }
  }
}