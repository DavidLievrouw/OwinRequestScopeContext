using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  public class OwinRequestScopeContextItems : IOwinRequestScopeContextItems {
    public OwinRequestScopeContextItems(OwinRequestScopeContextOptions options) {
      if (options == null) throw new ArgumentNullException(paramName: nameof(options));
      InnerDictionary = new ConcurrentDictionary<string, object>(options.ItemKeyEqualityComparer);
    }

    internal IDictionary<string, object> InnerDictionary { get; }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
      return InnerDictionary.GetEnumerator();
    }

    public void Clear() {
      InnerDictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, object> item) {
      return InnerDictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
      InnerDictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, object> item) {
      return InnerDictionary.Remove(item);
    }

    public int Count => InnerDictionary.Count;

    public bool IsReadOnly => InnerDictionary.IsReadOnly;

    public bool ContainsKey(string key) {
      return InnerDictionary.ContainsKey(key);
    }

    public bool Remove(string key) {
      return InnerDictionary.Remove(key);
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
    }
  }
}