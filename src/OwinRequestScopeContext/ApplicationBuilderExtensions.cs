#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MidFunc = System.Func<
    System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>, 
    System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

namespace DavidLievrouw.OwinRequestScopeContext {
    public static class ApplicationBuilderExtensions {
        public static Action<MidFunc> UseRequestScopeContext(this Action<MidFunc> builder, OwinRequestScopeContextOptions options = null) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder(UseRequestScopeContext(options).Invoke);
            return builder;
        }

        public static Action<MidFunc> Use(this Action<MidFunc> builder, Func<IDictionary<string, object>, Task> middleware) {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));

            builder(
                next =>
                    async environment => {
                        await middleware.Invoke(environment);
                        if (next != null) await next.Invoke(environment);
                    });
            return builder;
        }

        private static MidFunc UseRequestScopeContext(OwinRequestScopeContextOptions options = null) {
            options = options ?? OwinRequestScopeContextOptions.Default;
            return
                next =>
                    async environment => {
                        var middleware = new OwinRequestScopeContextMiddleware(next, options);
                        await middleware.Invoke(environment);
                    };
        }
    }
}
#endif