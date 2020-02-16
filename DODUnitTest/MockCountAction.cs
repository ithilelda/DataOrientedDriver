using DataOrientedDriver;
using System;

namespace DODUnitTest
{
    public class MockCountAction : DataOrientedDriver.Action
    {
        private string Key;
        private BlackBoard bb;
        private int count;
        public MockCountAction (IScheduler ss, BlackBoard bb, string key) : base(ss)
        {
            this.bb = bb;
            Key = key ?? throw new ArgumentNullException("key");
            count = 0;
        }

        public override void Step(float dt)
        {
            bb.PostInt(Key, ++count);
            Status = NodeStatus.SUCCESS;
            Exit(Status);
        }
    }
}