using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextItemsFixture {
    OwinRequestScopeContextItems _sut;
    IEqualityComparer<string> _keyComparer;

    [SetUp]
    public void SetUp() {
      _keyComparer = StringComparer.InvariantCulture;
      _sut = new OwinRequestScopeContextItems(_keyComparer) {{"A", 1}, {"B", 2}};
    }

    [TestFixture]
    public class Construction : OwinRequestScopeContextItemsFixture {
      [Test]
      public void GivenNullKeyComparer_Throws() {
        Action act = () => new OwinRequestScopeContextItems(null);
        act.Should().Throw<ArgumentNullException>();
      }

      [Test]
      public void UsesSpecifiedComparer() {
        var sut1 = new OwinRequestScopeContextItems(StringComparer.Ordinal) {{"abc", 1}};
        sut1.ContainsKey("ABC").Should().BeFalse();
        var sut2 = new OwinRequestScopeContextItems(StringComparer.OrdinalIgnoreCase) {{"abc", 1}};
        sut2.ContainsKey("ABC").Should().BeTrue();
      }
    }

    [TestFixture]
    public class Dispose : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenNoItemsAreRegisteredForDisposal_DoesNotThrow() {
        Action act = () => _sut.Dispose();
        act.Should().NotThrow();
      }

      [Test]
      public void DisposesOnlyItemsThatWereRequestedToBeDisposed() {
        var firstDisposable = A.Fake<IDisposable>();
        var secondDisposable = A.Fake<IDisposable>();
        var nonDisposable = A.Fake<object>();
        _sut.Add("D1", firstDisposable, true);
        _sut.Add("D2", secondDisposable, false);
        _sut.Add("O1", nonDisposable);
        _sut.Dispose();
        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => secondDisposable.Dispose()).MustNotHaveHappened();
        A.CallTo(nonDisposable).Where(_ => _.Method.Name == "Dispose").MustNotHaveHappened();
      }

      [Test]
      public void DisposesAllRegisteredItems() {
        var firstDisposable = A.Fake<IDisposable>();
        var secondDisposable = A.Fake<IDisposable>();
        _sut.Add("D1", firstDisposable, true);
        _sut.Add("D2", secondDisposable, true);
        _sut.Dispose();
        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => secondDisposable.Dispose()).MustHaveHappened();
      }

      [Test]
      public void WhenDisposalOfAnItemFails_StillDisposesOthers_ThrowsAggregateException() {
        var firstDisposable = A.Fake<IDisposable>();
        var secondDisposable = A.Fake<IDisposable>();
        var thirdDisposable = A.Fake<IDisposable>();
        _sut.Add("D1", firstDisposable, true);
        _sut.Add("D2", secondDisposable, true);
        _sut.Add("D3", thirdDisposable, true);

        var failureReason1 = new InvalidOperationException("I am the cause of the failure of the first disposable");
        A.CallTo(() => firstDisposable.Dispose()).Throws(failureReason1);

        var failureReason2 = new InvalidDataException("I am the cause of the failure of the second disposable");
        A.CallTo(() => secondDisposable.Dispose()).Throws(failureReason2);

        Action act = () => _sut.Dispose();
        act.Should().Throw<AggregateException>()
          .Where(_ => _.InnerExceptions.SequenceEqual(new Exception[] {failureReason1, failureReason2}));

        A.CallTo(() => firstDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => secondDisposable.Dispose()).MustHaveHappened();
        A.CallTo(() => thirdDisposable.Dispose()).MustHaveHappened();
      }
    }

    [TestFixture]
    public class GetEnumerator : OwinRequestScopeContextItemsFixture {
      [Test]
      public void EnumeratesAllKeyValuePairs() {
        var actualItems = new List<KeyValuePair<string, object>>();
        using (var enumerator = _sut.GetEnumerator()) {
          while (enumerator.MoveNext()) {
            actualItems.Add(enumerator.Current);
          }
        }

        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2)
        };
        actualItems.Should().BeEquivalentTo(expectedItems);
      }
    }

    [TestFixture]
    public class Clear : OwinRequestScopeContextItemsFixture {
      [Test]
      public void ClearsAllItemsFromDictionary() {
        _sut.Count.Should().BeGreaterThan(0);
        _sut.Clear();
        _sut.Count.Should().Be(0);
      }

      [Test]
      public void ClearsDisposableRegistrations() {
        _sut.Add("D1", new MyDisposableObject(), true);
        _sut.Clear();
        _sut.Disposables.Count.Should().Be(0);
      }
    }

    [TestFixture]
    public class Contains : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsItem_ReturnsTrue() {
        var itemToFind = _sut.InnerDictionary.ElementAt(0);
        _sut.Contains(itemToFind).Should().BeTrue();
      }

      [Test]
      public void WhenDictionaryDoesNotContainItem_ReturnsFalse() {
        var itemToFind = new KeyValuePair<string, object>("C", 3);
        _sut.Contains(itemToFind).Should().BeFalse();
      }
    }

    [TestFixture]
    public class CopyTo : OwinRequestScopeContextItemsFixture {
      [Test]
      public void GivenNullArrayToCopyTo_ThrowsArgumentNullException() {
        Action act = () => _sut.CopyTo(null, 0);
        act.Should().Throw<ArgumentNullException>();
      }

      [Test]
      public void GivenIndexOutOfRange_ThrowsArgumentException() {
        var inputArray = new KeyValuePair<string, object>[2];
        Action act = () => _sut.CopyTo(inputArray, inputArray.Length);
        act.Should().Throw<ArgumentException>();
      }

      [Test]
      public void CopiesToSpecifiedArray() {
        var inputArray = new KeyValuePair<string, object>[2];
        _sut.CopyTo(inputArray, 0);

        var expectedArray = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2)
        };
        inputArray.Should().BeEquivalentTo(expectedArray);
      }
    }

    [TestFixture]
    public class Remove : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsItem_RemovesItemAndReturnsTrue() {
        var itemToRemove = _sut.InnerDictionary.ElementAt(0);
        var itemThatIsStillThere = _sut.InnerDictionary.ElementAt(1);
        _sut.Remove(itemToRemove).Should().BeTrue();

        var expectedItems = new[] {
          itemThatIsStillThere
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void WhenDictionaryDoesNotContainItem_DoesNotRemoveItemAndReturnsFalse() {
        var itemToRemove = new KeyValuePair<string, object>("C", 3);
        _sut.Remove(itemToRemove).Should().BeFalse();

        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2)
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void WhenItemIsRemovedThatIsMarkedForDisposal_AlsoRemovesThatMark() {
        var disposableValue = new MyDisposableObject();

        _sut.Add("D1", disposableValue, true);
        _sut.Disposables.Should().BeEquivalentTo(expectation: new[] {disposableValue});
        var itemToRemove1 = _sut.Single(_ => _.Key == "D1");
        _sut.Remove(itemToRemove1);
        _sut.Disposables.Should().BeEmpty();

        _sut.Add("D1", disposableValue, false);
        _sut.Disposables.Should().BeEquivalentTo(Enumerable.Empty<IDisposable>());
        var itemToRemove2 = _sut.Single(_ => _.Key == "D1");
        _sut.Remove(itemToRemove2);
        _sut.Disposables.Should().BeEquivalentTo(Enumerable.Empty<IDisposable>());
      }
    }

    [TestFixture]
    public class Count : OwinRequestScopeContextItemsFixture {
      [Test]
      public void ReturnsNumberOfItemsInDictionary() {
        _sut.Count.Should().Be(2);
      }
    }

    [TestFixture]
    public class IsReadOnly : OwinRequestScopeContextItemsFixture {
      [Test]
      public void ReturnsFalse() {
        _sut.IsReadOnly.Should().BeFalse();
      }
    }

    [TestFixture]
    public class ContainsKey : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsKey_ReturnsTrue() {
        var itemToFind = _sut.InnerDictionary.ElementAt(0).Key;
        _sut.ContainsKey(itemToFind).Should().BeTrue();
      }

      [Test]
      public void WhenDictionaryDoesNotContainKey_ReturnsFalse() {
        var itemToFind = "C";
        _sut.ContainsKey(itemToFind).Should().BeFalse();
      }
    }

    [TestFixture]
    public class RemoveKey : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsKey_RemovesItemAndReturnsTrue() {
        var itemToRemove = _sut.InnerDictionary.ElementAt(0).Key;
        var itemThatIsStillThere = _sut.InnerDictionary.ElementAt(1);
        _sut.Remove(itemToRemove).Should().BeTrue();

        var expectedItems = new[] {
          itemThatIsStillThere
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void WhenDictionaryDoesNotContainKey_DoesNotRemoveItemAndReturnsFalse() {
        var itemToRemove = "C";
        _sut.Remove(itemToRemove).Should().BeFalse();

        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2)
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void WhenItemIsRemovedThatIsMarkedForDisposal_AlsoRemovesThatMark() {
        var disposableValue = new MyDisposableObject();

        _sut.Add("D1", disposableValue, true);
        _sut.Disposables.Should().BeEquivalentTo(expectation: new[] {disposableValue});
        _sut.Remove("D1");
        _sut.Disposables.Should().BeEmpty();

        _sut.Add("D1", disposableValue, false);
        _sut.Disposables.Should().BeEquivalentTo(Enumerable.Empty<IDisposable>());
        _sut.Remove("D1");
        _sut.Disposables.Should().BeEquivalentTo(Enumerable.Empty<IDisposable>());
      }
    }

    [TestFixture]
    public class TryGetValue : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsKey_SetsOutputItemAndReturnsTrue() {
        var itemToFind = _sut.InnerDictionary.ElementAt(0).Key;
        _sut.TryGetValue(itemToFind, value: out object actualItem).Should().BeTrue();
        var expectedItem = _sut.InnerDictionary.ElementAt(0).Value;
        actualItem.Should().Be(expectedItem);
      }

      [Test]
      public void WhenDictionaryDoesNotContainKey_DoesNotSetOutputItemAndReturnsFalse() {
        var itemToFind = "C";
        _sut.TryGetValue(itemToFind, value: out object actualItem).Should().BeFalse();
        actualItem.Should().BeNull();
      }
    }

    [TestFixture]
    public class Keys : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsNoItems_ReturnsEmptyResult() {
        _sut = new OwinRequestScopeContextItems(StringComparer.InvariantCulture);
        _sut.Keys.Should().NotBeNull().And.BeEmpty();
      }

      [Test]
      public void WhenDictionaryContainsItems_ReturnsKeysFromThoseItems() {
        var expected = new[] {"A", "B"};
        _sut.Keys.Should().BeEquivalentTo(expected);
      }
    }

    [TestFixture]
    public class Values : OwinRequestScopeContextItemsFixture {
      [Test]
      public void WhenDictionaryContainsNoItems_ReturnsEmptyResult() {
        _sut = new OwinRequestScopeContextItems(StringComparer.InvariantCulture);
        _sut.Values.Should().NotBeNull().And.BeEmpty();
      }

      [Test]
      public void WhenDictionaryContainsItems_ReturnsValuesFromThoseItems() {
        var expected = new[] {1, 2};
        _sut.Values.Should().BeEquivalentTo(expected);
      }
    }

    [TestFixture]
    public class GetEnumeratorForUntypedIEnumerable : OwinRequestScopeContextItemsFixture {
      [Test]
      public void EnumeratesAllKeyValuePairs() {
        var actualItems = new List<object>();
        var enumerator = ((IEnumerable) _sut).GetEnumerator();
        while (enumerator.MoveNext()) {
          actualItems.Add(enumerator.Current);
        }

        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2)
        };
        actualItems.Should().BeEquivalentTo(expectedItems);
      }
    }

    [TestFixture]
    public class Indexer : OwinRequestScopeContextItemsFixture {
      [TestFixture]
      public class Getter : Indexer {
        [Test]
        public void WhenDictionaryContainsKey_ReturnsTheValue() {
          var itemToFind = _sut.InnerDictionary.ElementAt(0).Key;
          _sut[itemToFind].Should().Be(_sut.InnerDictionary.ElementAt(0).Value);
        }

        [Test]
        public void WhenDictionaryDoesNotContainKey_ThrowsKeyNotFoundException() {
          var itemToFind = "C";
          Action act = () => {
            var dummy = _sut[itemToFind];
          };
          act.Should().Throw<KeyNotFoundException>();
        }
      }

      [TestFixture]
      public class Setter : Indexer {
        [Test]
        public void WhenDictionaryContainsKey_OverwritesTheValue() {
          var theNewValue = "The new value";
          var key = _sut.InnerDictionary.ElementAt(0).Key;
          _sut[key] = theNewValue;
          _sut[key].Should().Be(theNewValue);
        }

        [Test]
        public void WhenDictionaryDoesNotContainKey_AddsTheValue() {
          var theNewValue = "The new value";
          var key = "The new key";
          _sut[key] = theNewValue;

          var expectedItems = new[] {
            new KeyValuePair<string, object>("A", 1),
            new KeyValuePair<string, object>("B", 2),
            new KeyValuePair<string, object>(key, theNewValue)
          };
          var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
          actualItems.Should().BeEquivalentTo(expectedItems);
        }
      }
    }

    [TestFixture]
    public class AddObject : OwinRequestScopeContextItemsFixture {
      [Test]
      public void GivenNewItem_AddsItemToDictionary() {
        var newKey = "newKey";
        var newValue = new object();

        _sut.Add(newKey, newValue);
        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2),
          new KeyValuePair<string, object>(newKey, newValue)
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        Action action = () => _sut.Add(key: _sut.Keys.ElementAt(0), value: _sut.Values.ElementAt(0));
        action.Should().Throw<ArgumentException>();
      }
    }

    [TestFixture]
    public class AddKeyValuePair : OwinRequestScopeContextItemsFixture {
      [Test]
      public void GivenNewItem_AddsItemToDictionary() {
        var newItem = new KeyValuePair<string, object>("newKey", value: new object());

        _sut.Add(newItem);

        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2),
          newItem
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        var existingElement = _sut.ElementAt(0);
        Action action = () => _sut.Add(existingElement);
        action.Should().Throw<ArgumentException>();
      }
    }

    [TestFixture]
    public class AddDisposable : OwinRequestScopeContextItemsFixture {
      [Test]
      public void GivenNewDisposableItem_AddsItemToDictionary() {
        var newKey = "newKey";
        var newValue = new MyDisposableObject();

        _sut.Add(newKey, newValue);
        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2),
          new KeyValuePair<string, object>(newKey, newValue)
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        var existingKey = _sut.Keys.ElementAt(0);
        var newValue = new MyDisposableObject();
        Action action = () => _sut.Add(existingKey, newValue);
        action.Should().Throw<ArgumentException>();
      }
    }

    [TestFixture]
    public class AddDisposableWithFlag : OwinRequestScopeContextItemsFixture {
      [Test]
      public void GivenNewDisposableItem_AddsItemToDictionary() {
        var newKey = "newKey";
        var newValue = new MyDisposableObject();

        _sut.Add(newKey, newValue, true);
        var expectedItems = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2),
          new KeyValuePair<string, object>(newKey, newValue)
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.Should().BeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        var existingKey = _sut.Keys.ElementAt(0);
        var newValue = new MyDisposableObject();
        Action action = () => _sut.Add(existingKey, newValue, true);
        action.Should().Throw<ArgumentException>();
      }
    }

    class MyDisposableObject : IDisposable {
      public void Dispose() { }
    }
  }
}