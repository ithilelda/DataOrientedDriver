using DataOrientedDriver;

namespace DODUnitTest
{
    public class MockCountAction : Action
    {
        private string Key;
        private BlackBoard bb;
        private int count;
        public MockCountAction (IScheduler ss, BlackBoard bb, string key) : base(ss)
        {
            this.bb = bb;
            Key = key;
            count = 0;
        }

        public override void Step(float dt)
        {
            bb.Post(Key, ++count);
            Status = NodeStatus.SUCCESS;
            Parent.OnChildComplete(this, Status);
        }
    }
}