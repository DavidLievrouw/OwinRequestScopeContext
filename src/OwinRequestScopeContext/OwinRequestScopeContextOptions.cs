using System.Collections.Generic;

namespace DavidLievrouw.OwinRequestScopeContext {
    public class OwinRequestScopeContextOptions {
        public OwinRequestScopeContextOptions() {
            ItemKeyEqualityComparer = EqualityComparer<string>.Default;
        }

        public IEqualityComparer<string> ItemKeyEqualityComparer { get; set; }

        public static OwinRequestScopeContextOptions Default { get; } = new OwinRequestScopeContextOptions();
    }
}