using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextItemsTests {
    OwinRequestScopeContextItems _sut;

    [SetUp]
    public void SetUp() {
      _sut = new OwinRequestScopeContextItems(OwinRequestScopeContextOptions.Default) {{"A", 1}, {"B", 2}};
    }

    [TestFixture]
    public class Construction : OwinRequestScopeContextItemsTests {
      [Test]
      public void GivenNullOptions_Throws() {
        Action act = () => new OwinRequestScopeContextItems(null);
        act.ShouldThrow<ArgumentNullException>();
      }

      [Test]
      public void UsesSpecifiedComparerFromOptions() {
        var options1 = new OwinRequestScopeContextOptions {
          ItemKeyEqualityComparer = StringComparer.Ordinal
        };
        var sut1 = new OwinRequestScopeContextItems(options1) {{"abc", 1}};
        sut1.ContainsKey("ABC").Should().BeFalse();

        var options2 = new OwinRequestScopeContextOptions {
          ItemKeyEqualityComparer = StringComparer.OrdinalIgnoreCase
        };
        var sut2 = new OwinRequestScopeContextItems(options2) {{"abc", 1}};
        sut2.ContainsKey("ABC").Should().BeTrue();
      }
    }

    [TestFixture]
    public class GetEnumerator : OwinRequestScopeContextItemsTests {
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }
    }

    [TestFixture]
    public class Clear : OwinRequestScopeContextItemsTests {
      [Test]
      public void ClearsAllItemsFromDictionary() {
        _sut.Count.Should().BeGreaterThan(0);
        _sut.Clear();
        _sut.Count.Should().Be(0);
      }
    }

    [TestFixture]
    public class Contains : OwinRequestScopeContextItemsTests {
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
    public class CopyTo : OwinRequestScopeContextItemsTests {
      [Test]
      public void GivenNullArrayToCopyTo_ThrowsArgumentNullException() {
        Action act = () => _sut.CopyTo(null, 0);
        act.ShouldThrow<ArgumentNullException>();
      }

      [Test]
      public void GivenIndexOutOfRange_ThrowsArgumentException() {
        var inputArray = new KeyValuePair<string, object>[2];
        Action act = () => _sut.CopyTo(inputArray, inputArray.Length);
        act.ShouldThrow<ArgumentException>();
      }

      [Test]
      public void CopiesToSpecifiedArray() {
        var inputArray = new KeyValuePair<string, object>[2];
        _sut.CopyTo(inputArray, 0);

        var expectedArray = new[] {
          new KeyValuePair<string, object>("A", 1),
          new KeyValuePair<string, object>("B", 2)
        };
        inputArray.ShouldBeEquivalentTo(expectedArray);
      }
    }

    [TestFixture]
    public class Remove : OwinRequestScopeContextItemsTests {
      [Test]
      public void WhenDictionaryContainsItem_RemovesItemAndReturnsTrue() {
        var itemToRemove = _sut.InnerDictionary.ElementAt(0);
        var itemThatIsStillThere = _sut.InnerDictionary.ElementAt(1);
        _sut.Remove(itemToRemove).Should().BeTrue();

        var expectedItems = new[] {
          itemThatIsStillThere
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.ShouldBeEquivalentTo(expectedItems);
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }
    }

    [TestFixture]
    public class Count : OwinRequestScopeContextItemsTests {
      [Test]
      public void ReturnsNumberOfItemsInDictionary() {
        _sut.Count.Should().Be(2);
      }
    }

    [TestFixture]
    public class IsReadOnly : OwinRequestScopeContextItemsTests {
      [Test]
      public void ReturnsFalse() {
        _sut.IsReadOnly.Should().BeFalse();
      }
    }

    [TestFixture]
    public class ContainsKey : OwinRequestScopeContextItemsTests {
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
    public class RemoveKey : OwinRequestScopeContextItemsTests {
      [Test]
      public void WhenDictionaryContainsKey_RemovesItemAndReturnsTrue() {
        var itemToRemove = _sut.InnerDictionary.ElementAt(0).Key;
        var itemThatIsStillThere = _sut.InnerDictionary.ElementAt(1);
        _sut.Remove(itemToRemove).Should().BeTrue();

        var expectedItems = new[] {
          itemThatIsStillThere
        };
        var actualItems = (ICollection<KeyValuePair<string, object>>) _sut;
        actualItems.ShouldBeEquivalentTo(expectedItems);
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }
    }

    [TestFixture]
    public class TryGetValue : OwinRequestScopeContextItemsTests {
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
    public class Keys : OwinRequestScopeContextItemsTests {
      [Test]
      public void WhenDictionaryContainsNoItems_ReturnsEmptyResult() {
        _sut = new OwinRequestScopeContextItems(OwinRequestScopeContextOptions.Default);
        _sut.Keys.Should().NotBeNull().And.BeEmpty();
      }

      [Test]
      public void WhenDictionaryContainsItems_ReturnsKeysFromThoseItems() {
        var expected = new[] {"A", "B"};
        _sut.Keys.ShouldBeEquivalentTo(expected);
      }
    }

    [TestFixture]
    public class Values : OwinRequestScopeContextItemsTests {
      [Test]
      public void WhenDictionaryContainsNoItems_ReturnsEmptyResult() {
        _sut = new OwinRequestScopeContextItems(OwinRequestScopeContextOptions.Default);
        _sut.Values.Should().NotBeNull().And.BeEmpty();
      }

      [Test]
      public void WhenDictionaryContainsItems_ReturnsValuesFromThoseItems() {
        var expected = new[] {1, 2};
        _sut.Values.ShouldBeEquivalentTo(expected);
      }
    }

    [TestFixture]
    public class GetEnumeratorForUntypedIEnumerable : OwinRequestScopeContextItemsTests {
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }
    }

    [TestFixture]
    public class Indexer : OwinRequestScopeContextItemsTests {
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
          act.ShouldThrow<KeyNotFoundException>();
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
          actualItems.ShouldBeEquivalentTo(expectedItems);
        }
      }
    }

    [TestFixture]
    public class AddObject : OwinRequestScopeContextItemsTests {
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        Action action = () => _sut.Add(key: _sut.Keys.ElementAt(0), value: _sut.Values.ElementAt(0));
        action.ShouldThrow<ArgumentException>();
      }
    }

    [TestFixture]
    public class AddKeyValuePair : OwinRequestScopeContextItemsTests {
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        var existingElement = _sut.ElementAt(0);
        Action action = () => _sut.Add(existingElement);
        action.ShouldThrow<ArgumentException>();
      }
    }

    [TestFixture]
    public class AddDisposable : OwinRequestScopeContextItemsTests {
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        var existingKey = _sut.Keys.ElementAt(0);
        var newValue = new MyDisposableObject();
        Action action = () => _sut.Add(existingKey, newValue);
        action.ShouldThrow<ArgumentException>();
      }
    }

    [TestFixture]
    public class AddDisposableWithFlag : OwinRequestScopeContextItemsTests {
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
        actualItems.ShouldBeEquivalentTo(expectedItems);
      }

      [Test]
      public void GivenExistingKey_ThrowsArgumentException() {
        var existingKey = _sut.Keys.ElementAt(0);
        var newValue = new MyDisposableObject();
        Action action = () => _sut.Add(existingKey, newValue, true);
        action.ShouldThrow<ArgumentException>();
      }
    }

    class MyDisposableObject : IDisposable {
      public void Dispose() { }
    }
  }
}