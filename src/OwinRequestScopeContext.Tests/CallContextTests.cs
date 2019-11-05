using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DavidLievrouw.OwinRequestScopeContext {
    public class CallContextTests {
        [Fact(Skip="Not deterministic, run manually.")]
        public void WhenFlowingData_ThenCanUseContext() {
            var d1 = new object();
            var t1 = default(object);
            var t10 = default(object);
            var t11 = default(object);
            var t12 = default(object);
            var t13 = default(object);
            var d2 = new object();
            var t2 = default(object);
            var t20 = default(object);
            var t21 = default(object);
            var t22 = default(object);
            var t23 = default(object);

            Task.WaitAll(
                Task.Run(() => {
                    CallContext.SetData("d1", d1);
                    new Thread(() => t10 = CallContext.GetData("d1")).Start();
                    Task.WaitAll(
                        Task.Run(() => t1 = CallContext.GetData("d1"))
                            .ContinueWith(t => Task.Run(() => t11 = CallContext.GetData("d1"))),
                        Task.Run(() => t12 = CallContext.GetData("d1")),
                        Task.Run(() => t13 = CallContext.GetData("d1"))
                    );
                }),
                Task.Run(() => {
                    CallContext.SetData("d2", d2);
                    new Thread(() => t20 = CallContext.GetData("d2")).Start();
                    Task.WaitAll(
                        Task.Run(() => t2 = CallContext.GetData("d2"))
                            .ContinueWith(t => Task.Run(() => t21 = CallContext.GetData("d2"))),
                        Task.Run(() => t22 = CallContext.GetData("d2")),
                        Task.Run(() => t23 = CallContext.GetData("d2"))
                    );
                })
            );

            d1.Should().BeSameAs(t1);
            d1.Should().BeSameAs(t10);
            d1.Should().BeSameAs(t11);
            d1.Should().BeSameAs(t12);
            d1.Should().BeSameAs(t13);

            d2.Should().BeSameAs(t2);
            d2.Should().BeSameAs(t20);
            d2.Should().BeSameAs(t21);
            d2.Should().BeSameAs(t22);
            d2.Should().BeSameAs(t23);

            CallContext.GetData("d1").Should().BeNull();
            CallContext.GetData("d2").Should().BeNull();
        }
    }
}