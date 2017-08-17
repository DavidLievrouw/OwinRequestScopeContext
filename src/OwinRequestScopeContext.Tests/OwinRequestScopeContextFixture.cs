using System;
using System.Collections.Generic;
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
      _items = A.Fake<IInternalOwinRequestScopeContextItems>();
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
        ((IInternalOwinRequestScopeContextItems)_sut.Items).Disposables.Should().NotBeNull().And.BeEmpty();
      }

      [Test]
      public void GivenNullDisposable_ThrowsArgumentNullException() {
        Action act = () => _sut.RegisterForDisposal(null);
        act.ShouldThrow<ArgumentNullException>();
      }

      [Test]
      public void AddsItemToListOfDisposables() {
        _sut.RegisterForDisposal(_disposable);
        A.CallTo(() => ((IInternalOwinRequestScopeContextItems)_sut.Items).Disposables.Add(_disposable))
          .MustHaveHappened();
      }

      [Test]
      public void AddsItemToListOfDisposables_EvenIfItIsRegisteredAlready() {
        _sut.RegisterForDisposal(_disposable);
        _sut.RegisterForDisposal(_disposable);
        A.CallTo(() => ((IInternalOwinRequestScopeContextItems)_sut.Items).Disposables.Add(_disposable))
          .MustHaveHappened(Repeated.Exactly.Times(2));
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
      public void FreesContextSlotAfterwards() {
        OwinRequestScopeContext.Current = _sut;
        _sut.Dispose();
        OwinRequestScopeContext.Current.Should().BeNull();
      }
    }
  }
}