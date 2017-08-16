using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class ExtensionsTests {
    [TestFixture]
    public class ForEach : ExtensionsTests {
      [SetUp]
      public void SetUp() {
        SourceItem.ResetPerformedActionCount();
      }

      [Test]
      public void GivenSourceIsNull_DoesNotThrow_DoesNotPerformAction() {
        IEnumerable<SourceItem> source = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        Assert.DoesNotThrow(() => source.ForEach(item => item.DoSomeAction()));
        SourceItem.PerformedActionCount.Should().Be(0);
      }

      [Test]
      public void GivenActionIsNull_DoesNotThrow_DoesNotPerformAction() {
        var source = new[] { new SourceItem(), new SourceItem() };
        Assert.DoesNotThrow(() => source.ForEach((Action<SourceItem>)null));
        SourceItem.PerformedActionCount.Should().Be(0);
        foreach (var sourceItem in source) {
          Assert.That(sourceItem.ActionPerformed, Is.False);
        }
      }

      [Test]
      public void GivenSourceIsEmpty_DoesNotThrow_DoesNotPerformAction() {
        var source = Enumerable.Empty<SourceItem>();
        Assert.DoesNotThrow(() => source.ForEach(item => item.DoSomeAction()));
        SourceItem.PerformedActionCount.Should().Be(0);
      }

      [Test]
      public void GivenSourceWithElements_PerformsActionForAllElements() {
        var source = new[] { new SourceItem(), new SourceItem() };
        Assert.DoesNotThrow(() => source.ForEach(item => item.DoSomeAction()));
        SourceItem.PerformedActionCount.Should().Be(source.Length);
        foreach (var sourceItem in source) {
          sourceItem.ActionPerformed.Should().BeTrue();
        }
      }

      class SourceItem {
        public static int PerformedActionCount { get; private set; }

        public static void ResetPerformedActionCount() {
          PerformedActionCount = 0;
        }

        public bool ActionPerformed { get; private set; }

        public void DoSomeAction() {
          ActionPerformed = true;
          PerformedActionCount++;
        }
      }
    }

    [TestFixture]
    public class ForEachWithIndex : ExtensionsTests {
      [SetUp]
      public void SetUp() {
        SourceItem.ResetPerformedActions();
      }

      [Test]
      public void GivenSourceIsNull_DoesNotThrow_DoesNotPerformAction() {
        IEnumerable<SourceItem> source = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        Assert.DoesNotThrow(() => source.ForEach((item,idx) => item.DoSomeAction(idx)));
        Assert.That(SourceItem.PerformedActions, Is.Empty);
      }

      [Test]
      public void GivenActionIsNull_DoesNotThrow_DoesNotPerformAction() {
        var source = new[] { new SourceItem(), new SourceItem() };
        Assert.DoesNotThrow(() => source.ForEach((Action<SourceItem, int>)null));
        Assert.That(SourceItem.PerformedActions, Is.Empty);
        foreach (var sourceItem in source) {
          sourceItem.ActionIndex.Should().Be(-1);
        }
      }

      [Test]
      public void GivenSourceIsEmpty_DoesNotThrow_DoesNotPerformAction() {
        var source = Enumerable.Empty<SourceItem>();
        Assert.DoesNotThrow(() => source.ForEach((item, idx) => item.DoSomeAction(idx)));
        Assert.That(SourceItem.PerformedActions, Is.Empty);
      }

      [Test]
      public void GivenSourceWithElements_PerformsActionForAllElements_WithCorrectIndex() {
        var source = new[] { new SourceItem(), new SourceItem() };
        Assert.DoesNotThrow(() => source.ForEach((item, idx) => item.DoSomeAction(idx)));
        SourceItem.PerformedActions.Count.Should().Be(source.Length);
        foreach (var sourceItem in source) {
          sourceItem.ActionIndex.Should().Be(Array.IndexOf(source, sourceItem));
        }
      }

      class SourceItem {
        public static Dictionary<SourceItem, int> PerformedActions;

        public SourceItem() {
          ActionIndex = -1;
          PerformedActions = new Dictionary<SourceItem, int>();
        }

        public static void ResetPerformedActions() {
          PerformedActions = new Dictionary<SourceItem, int>();
        }

        public int ActionIndex { get; private set; }

        public void DoSomeAction(int index) {
          ActionIndex = index;
          PerformedActions[this] = index;
        }
      }
    }
  }
}