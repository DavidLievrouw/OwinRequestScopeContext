using System;
using System.Diagnostics;

namespace Sample {
    public class MyDisposableObject : IDisposable {
        public string Value { get; set; }

        public void Dispose() {
            Debug.WriteLine($"{GetType().Name} with value '{Value ?? "[NULL]"}' was disposed.");
        }
    }
}