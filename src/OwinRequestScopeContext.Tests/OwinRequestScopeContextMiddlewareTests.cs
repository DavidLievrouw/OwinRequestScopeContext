using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Owin;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextMiddlewareTests {
    private FakeMiddleware _next;
    private OwinRequestScopeContextMiddleware _sut;

    [SetUp]
    public virtual void SetUp() {
      _next = new FakeMiddleware(null);
      _sut = new OwinRequestScopeContextMiddleware(_next);
    }

    [TestFixture]
    public class Construction : OwinRequestScopeContextMiddlewareTests {
      [Test]
      public void AllowsNullNext() {
        Action act = () => new OwinRequestScopeContextMiddleware(null);
        act.ShouldNotThrow();
      }
    }

    [TestFixture]
    public class Invoke : OwinRequestScopeContextMiddlewareTests {
      private IOwinContext _owinContext;

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

      private class OwinRequestScopeContextIsPresentValidatingMiddleware : OwinMiddleware {
        public OwinRequestScopeContextIsPresentValidatingMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context) {
          OwinRequestScopeContext.Current.Should().NotBeNull().And
            .Match<IOwinRequestScopeContext>(_ => _.OwinContext.Equals(context));
          if (Next != null) await Next.Invoke(context).ConfigureAwait(false);
        }
      }
    }

    private class FakeMiddleware : OwinMiddleware {
      public FakeMiddleware(OwinMiddleware next) : base(next) {
        InvokedWith = new List<IOwinContext>();
      }

      public IList<IOwinContext> InvokedWith { get; }

      public override async Task Invoke(IOwinContext context) {
        InvokedWith.Add(context);
        if (Next != null) await Next.Invoke(context).ConfigureAwait(false);
      }
    }
  }
}