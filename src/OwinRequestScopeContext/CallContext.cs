using System.Collections.Concurrent;
using System.Threading;

namespace DavidLievrouw.OwinRequestScopeContext {
    internal static class CallContext {
        static readonly ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();

        public static void SetData(string name, object data) {
            state.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;
        }

        public static object GetData(string name) {
            return state.TryGetValue(name, out var data) ? data.Value : null;
        }

        public static bool FreeNamedDataSlot(string name) {
            return state.TryRemove(name, out var data) ? true : false;
        }
    }
}