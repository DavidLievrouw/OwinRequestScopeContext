using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace DavidLievrouw.OwinRequestScopeContext {
    public class ExtensionsFixture {
        public class ForEach : ExtensionsFixture {
            public ForEach() {
                SourceItem.ResetPerformedActionCount();
            }

            [Fact]
            public void GivenActionIsNull_DoesNotThrow_DoesNotPerformAction() {
                var source = new[] {new SourceItem(), new SourceItem()};
                Action act = () => source.ForEach((Action<SourceItem>) null);
                act.Should().NotThrow();
                SourceItem.PerformedActionCount.Should().Be(0);
                foreach (var sourceItem in source) {
                    sourceItem.ActionPerformed.Should().BeFalse();
                }
            }

            [Fact]
            public void GivenSourceIsEmpty_DoesNotThrow_DoesNotPerformAction() {
                var source = Enumerable.Empty<SourceItem>();
                Action act = () => source.ForEach(item => item.DoSomeAction());
                act.Should().NotThrow();
                SourceItem.PerformedActionCount.Should().Be(0);
            }

            [Fact]
            public void GivenSourceIsNull_DoesNotThrow_DoesNotPerformAction() {
                IEnumerable<SourceItem> source = null;
                Action act = () => source.ForEach(item => item.DoSomeAction());
                act.Should().NotThrow();
                SourceItem.PerformedActionCount.Should().Be(0);
            }

            [Fact]
            public void GivenSourceWithElements_PerformsActionForAllElements() {
                var source = new[] {new SourceItem(), new SourceItem()};
                Action act = () => source.ForEach(item => item.DoSomeAction());
                act.Should().NotThrow();
                SourceItem.PerformedActionCount.Should().Be(source.Length);
                foreach (var sourceItem in source) {
                    sourceItem.ActionPerformed.Should().BeTrue();
                }
            }

            class SourceItem {
                public static int PerformedActionCount { get; private set; }

                public bool ActionPerformed { get; private set; }

                public static void ResetPerformedActionCount() {
                    PerformedActionCount = 0;
                }

                public void DoSomeAction() {
                    ActionPerformed = true;
                    PerformedActionCount++;
                }
            }
        }

        public class ForEachWithIndex : ExtensionsFixture {
            public ForEachWithIndex() {
                SourceItem.ResetPerformedActions();
            }

            [Fact]
            public void GivenActionIsNull_DoesNotThrow_DoesNotPerformAction() {
                var source = new[] {new SourceItem(), new SourceItem()};
                Action act = () => source.ForEach((Action<SourceItem, int>) null);
                act.Should().NotThrow();
                SourceItem.PerformedActions.Should().BeEmpty();
                foreach (var sourceItem in source) {
                    sourceItem.ActionIndex.Should().Be(-1);
                }
            }

            [Fact]
            public void GivenSourceIsEmpty_DoesNotThrow_DoesNotPerformAction() {
                var source = Enumerable.Empty<SourceItem>();
                Action act = () => source.ForEach((item, idx) => item.DoSomeAction(idx));
                act.Should().NotThrow();
                SourceItem.PerformedActions.Should().BeEmpty();
            }

            [Fact]
            public void GivenSourceIsNull_DoesNotThrow_DoesNotPerformAction() {
                IEnumerable<SourceItem> source = null;
                Action act = () => source.ForEach((item, idx) => item.DoSomeAction(idx));
                act.Should().NotThrow();
                SourceItem.PerformedActions.Should().BeEmpty();
            }

            [Fact]
            public void GivenSourceWithElements_PerformsActionForAllElements_WithCorrectIndex() {
                var source = new[] {new SourceItem(), new SourceItem()};
                Action act = () => source.ForEach((item, idx) => item.DoSomeAction(idx));
                act.Should().NotThrow();
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

                public int ActionIndex { get; private set; }

                public static void ResetPerformedActions() {
                    PerformedActions = new Dictionary<SourceItem, int>();
                }

                public void DoSomeAction(int index) {
                    ActionIndex = index;
                    PerformedActions[this] = index;
                }
            }
        }
    }
}