﻿using System;
using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
  internal static class EnumerableExtensions {
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
      if (source == null) return;
      if (action == null) return;
      foreach (var element in source) action(element);
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action) {
      if (source == null) return;
      if (action == null) return;
      var index = 0;
      foreach (var element in source) action(element, index++);
    }
  }
}