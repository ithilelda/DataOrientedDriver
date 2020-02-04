using DataOrientedDriver;

namespace DODUnitTest
{
    public class MockBuilder : BehaviorTreeBuilder<MockBuilder>
    {
        protected override MockBuilder BuilderInstance => this;
        public MockBuilder(BehaviorSystem ss, BlackBoard bb) : base(ss, bb) {}

        public void NoCountAction()
        {
            var a = new MockCountAction(ss, bb, "noCount");
            Action(a);
        }
        public void CountAction()
        {
            var a = new MockCountAction(ss, bb, "count");
            Action(a);
        }
    }
}