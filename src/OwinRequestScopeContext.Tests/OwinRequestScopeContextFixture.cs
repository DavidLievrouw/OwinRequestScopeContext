using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Owin;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextFixture {
    IOwinContext _owinContext;
    OwinRequestScopeContext _sut;

    [SetUp]
    public virtual void SetUp() {
      _owinContext = A.Fake<IOwinContext>();
      _sut = new OwinRequestScopeContext(_owinContext);
    }

    [TestFixture]
    public class OwinContext : OwinRequestScopeContextFixture {
      [Test]
      public void ReturnsOwinContextFromConstructor() {
        _sut.OwinContext.Should().Be(_owinContext);
      }
    }

    [TestFixture]
    public class Items : OwinRequestScopeContextFixture {
      [Test]
      public void DictionaryIsAvailableAfterConstruction() {
        _sut.Items.Should().NotBeNull();
      }

      [Test]
      public void IsThreadSafeCollection() {
        _sut.Items.Should().BeAssignableTo<ConcurrentDictionary<string, object>>();
      }
    }

    [TestFixture]
    public class RegisterForDisposal : OwinRequestScopeContextFixture {
      IDisposable _disposable;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _disposable = A.Fake<IDisposable>();
      }

      [Test]
      public void HasNoDisposablesByDefault() {
        _sut.Disposables.Should().NotBeNull().And.BeEmpty();
      }

      [Test]
      public void GivenNullDisposable_ThrowsArgumentNullException() {
        Action act = () => _sut.RegisterForDisposal(null);
        act.ShouldThrow<ArgumentNullException>();
      }

      [Test]
      public void AddsItemToListOfDisposables() {
        _sut.RegisterForDisposal(_disposable);
        _sut.Disposables.ShouldBeEquivalentTo(expectation: new[] {_disposable});
      }

      [Test]
      public void AddsItemToListOfDisposables_EvenIfItIsRegisteredAlready() {
        _sut.RegisterForDisposal(_disposable);
        _sut.RegisterForDisposal(_disposable);
        _sut.Disposables.ShouldBeEquivalentTo(expectation: new[] {_disposable, _disposable});
      }
    }

    [TestFixture]
    public class Dispose : OwinRequestScopeContextFixture {
      [Test]
      public void WhenNoItemsAreRegisteredForDisposal_DoesNotThrow() {
        Action act = () => _sut.Dispose();
        act.ShouldNotThrow();
      }

      [Test]
      public void DisposesAllRegisteredItems() {
        var firstDisposable = A.Fake<IDisposable>();
        var secondDisposable = A.Fake<IDisposable>();
        _sut.RegisterForDisposal(firstDisposable);
        _sut.RegisterForDisposal(secondDisposable);
        _sut.Dispose();
        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => secondDisposable.Dispose()).MustHaveHappened();
      }

      [Test]
      public void WhenADisposableIsRegisteredMultipleTimes_ItIsCalledThatAmountOfTimes() {
        var firstDisposable = A.Fake<IDisposable>();
        var secondDisposable = A.Fake<IDisposable>();
        _sut.RegisterForDisposal(firstDisposable);
        _sut.RegisterForDisposal(secondDisposable);
        _sut.RegisterForDisposal(firstDisposable);
        _sut.Dispose();
        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened(Repeated.Exactly.Twice);
        A.CallTo(() => secondDisposable.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
      }

      [Test]
      public void WhenDisposalOfAnItemFails_StillDisposesOthers_ThrowsAggregateException() {
        var firstDisposable = A.Fake<IDisposable>();
        var secondDisposable = A.Fake<IDisposable>();
        var thirdDisposable = A.Fake<IDisposable>();
        _sut.RegisterForDisposal(firstDisposable);
        _sut.RegisterForDisposal(secondDisposable);
        _sut.RegisterForDisposal(thirdDisposable);

        var failureReason1 = new InvalidOperationException("I am the cause of the failure of the first disposable");
        A.CallTo(() => firstDisposable.Dispose()).Throws(failureReason1);

        var failureReason2 = new InvalidDataException("I am the cause of the failure of the second disposable");
        A.CallTo(() => secondDisposable.Dispose()).Throws(failureReason2);

        Action act = () => _sut.Dispose();
        act.ShouldThrow<AggregateException>().Where(_ => _.InnerExceptions.SequenceEqual(new Exception[] { failureReason1, failureReason2 }));

        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => secondDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => thirdDisposable.Dispose()).MustHaveHappened();
      }
    }
  }
}