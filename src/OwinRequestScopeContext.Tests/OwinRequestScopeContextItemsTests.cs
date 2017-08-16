using System;
using FluentAssertions;
using NUnit.Framework;

namespace DavidLievrouw.OwinRequestScopeContext {
  [TestFixture]
  public class OwinRequestScopeContextItemsTests {
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
  }
}