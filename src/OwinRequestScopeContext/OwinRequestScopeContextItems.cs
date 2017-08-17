using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DavidLievrouw.OwinRequestScopeContext {
  public class OwinRequestScopeContextItems : IInternalOwinRequestScopeContextItems {
    public OwinRequestScopeContextItems(IEqualityComparer<string> keyComparer) {
      if (keyComparer == null) throw new ArgumentNullException(paramName: nameof(keyComparer));
      InnerDictionary = new ConcurrentDictionary<string, object>(keyComparer);
      KeyComparer = keyComparer;
      Disposables = new List<IDisposable>();
    }

    // For unit tests
    internal IDictionary<string, object> InnerDictionary { get; }
    internal IEqualityComparer<string> KeyComparer { get; }
    internal List<IDisposable> Disposables { get; }

    // For obsolete RegisterForDisposal method
    ICollection<IDisposable> IInternalOwinRequestScopeContextItems.Disposables => Disposables;

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
      return InnerDictionary.GetEnumerator();
    }

    public void Clear() {
      InnerDictionary.Clear();
      Disposables.Clear();
    }

    public bool Contains(KeyValuePair<string, object> item) {
      return InnerDictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
      InnerDictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, object> item) {
      var isRemoved = InnerDictionary.Remove(item);
      if (isRemoved && item.Value is IDisposable) Disposables.RemoveAll(_ => item.Value.Equals(_));
      return isRemoved;
    }

    public int Count => InnerDictionary.Count;

    public bool IsReadOnly => InnerDictionary.IsReadOnly;

    public bool ContainsKey(string key) {
      return InnerDictionary.ContainsKey(key);
    }

    public bool Remove(string key) {
      if (!TryGetValue(key, out object correspondingValue)) return false;

      var isRemoved = InnerDictionary.Remove(key);
      if (isRemoved && correspondingValue is IDisposable) Disposables.RemoveAll(_ => correspondingValue.Equals(_));

      return isRemoved;
    }

    public bool TryGetValue(string key, out object value) {
      return InnerDictionary.TryGetValue(key, out value);
    }

    public ICollection<string> Keys => InnerDictionary.Keys;

    public ICollection<object> Values => InnerDictionary.Values;

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public object this[string key] {
      get => InnerDictionary[key];
      set => InnerDictionary[key] = value;
    }

    public void Add(string key, object value) {
      InnerDictionary.Add(key, value);
    }

    public void Add(KeyValuePair<string, object> item) {
      InnerDictionary.Add(item);
    }

    public void Add(string key, IDisposable value) {
      Add(key, value, false);
    }

    public void Add(string key, IDisposable value, bool disposeWhenRequestIsCompleted) {
      InnerDictionary.Add(key, value);
      if (disposeWhenRequestIsCompleted) Disposables.Add(value);
    }

    public void Dispose() {
      var disposalExceptions = new List<Exception>();

      Disposables.ForEach(disposable => {
        try {
          disposable.Dispose();
        }
        catch (Exception ex) {
          disposalExceptions.Add(ex);
        }
      });

      if (disposalExceptions.Any())
        throw new AggregateException("One or more exception occurred while disposing items that were registered for disposal.", disposalExceptions);
    }
  }
}