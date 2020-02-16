using DataOrientedDriver;

namespace DODUnitTest
{
    public class MockBuilder : BehaviorTreeBuilder<MockBuilder>
    {
        protected override MockBuilder BuilderInstance => this;
        public MockBuilder(BehaviorSystem ss, BlackBoard bb) : base(ss, bb) {}

        public MockBuilder MockCountAction(string key)
        {
            var a = new MockCountAction(ss, bb, key);
            Action(a);
            return this;
        }
    }
}