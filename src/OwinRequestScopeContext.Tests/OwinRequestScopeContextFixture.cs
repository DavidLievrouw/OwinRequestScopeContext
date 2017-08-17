using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextFixture {
    IDictionary<string, object> _owinEnvironment;
    OwinRequestScopeContext _sut;
    IOwinRequestScopeContextItems _items;

    [SetUp]
    public virtual void SetUp() {
      _owinEnvironment = new Dictionary<string, object> {{"the meaning of life, the universe, and everything", 42}};
      _items = A.Fake<IOwinRequestScopeContextItems>();
      _sut = new OwinRequestScopeContext(_owinEnvironment, _items, OwinRequestScopeContextOptions.Default);
    }

    [TestFixture]
    public class Construction : OwinRequestScopeContextFixture {
      [Test]
      public void GivenNullOptions_ThrowsArgumentNullException() {
        Action act = () => new OwinRequestScopeContext(_owinEnvironment, _items, null);
        act.ShouldThrow<ArgumentNullException>();
      }

      [Test]
      public void GivenNullOwinEnvironment_DoesNotThrow() {
        Action act = () => new OwinRequestScopeContext(null, _items, OwinRequestScopeContextOptions.Default);
        act.ShouldThrow<ArgumentNullException>();
      }

      [Test]
      public void GivenNullItems_ThrowsArgumentNullException() {
        Action act = () => new OwinRequestScopeContext(_owinEnvironment, null, OwinRequestScopeContextOptions.Default);
        act.ShouldThrow<ArgumentNullException>();
      }
    }

    [TestFixture]
    public class OwinContext : OwinRequestScopeContextFixture {
      [Test]
      public void ReturnsOwinContextFromConstructor() {
        _sut.OwinEnvironment.ShouldBeEquivalentTo(_owinEnvironment);
      }
    }

    [TestFixture]
    public class Items : OwinRequestScopeContextFixture {
      [Test]
      public void DictionaryIsAvailableAfterConstruction() {
        _sut.Items.Should().NotBeNull();
      }

      [Test]
      public void IntializesExpectedProperties() {
        _sut.Items.Should().NotBeNull();
        _sut.OwinEnvironment.ShouldBeEquivalentTo(_owinEnvironment);
        _sut.Disposables.Should().NotBeNull();
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
      public void DisposesItems() {
        A.CallTo(() => _items.Dispose()).MustNotHaveHappened();
        _sut.Dispose();
        A.CallTo(() => _items.Dispose()).MustHaveHappened();
      }

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
        act.ShouldThrow<AggregateException>()
          .Where(_ => _.InnerExceptions.SequenceEqual(new Exception[] {failureReason1, failureReason2}));

        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => secondDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => thirdDisposable.Dispose()).MustHaveHappened();
      }
    }
  }
}