

namespace DataOrientedDriver
{
    public class InfiniteDecorator : Decorator
    {
        public InfiniteDecorator(IScheduler s) : base(s) { }

        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // The infiniteDecorator is specially used as the root, so it is different than anything else, where itself is never exited, but will enter its child again on completion.
            // The child is entered in the same frame, so it will be stepped the very next frame.
            Child.Enter();
        }
    }
    public class IgnoreFailureDecorator : Decorator
    {
        public IgnoreFailureDecorator(IScheduler s) : base(s) { }

        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // Because it is not posted, so we have to acknowledge our parent (if any) about our completion, so instead of calling exit from the scheduler, we have to exit here.
            // We don't have to do anything to our own status because it doesn't matter. We just propagate success up the tree regardless of our child's.
            Exit(NodeStatus.SUCCESS);
        }
    }
    public class InvertDecorator : Decorator
    {
        public InvertDecorator(IScheduler s) : base(s) { }

        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // similar to above, we just make our status the opposite, and exit here.
            // since success = 1 (b01), failure = 2 (b10), we can simple XOR them with the number 3 (b11) to get their opposite.
            Exit(status ^ NodeStatus.RUNNING);
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected int times;
        protected int counter;
        public RepeatDecorator(IScheduler s, int t) : base(s) => times = t;
        public override void Enter()
        {
            // we need to reset the counter besides doing decorator enter stuff.
            counter = 0;
            base.Enter();
        }
        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // We only repeat until the set count if our child completed successfully. 
            if (status == NodeStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    Child.Enter();
                }
                else Exit(NodeStatus.SUCCESS); // if we reached our limit and all operations were successful, we exit with success. 
            }
            else Exit(NodeStatus.FAILURE); // if any of our child operation failed, we exit with failure and does not continue.
        }
    }
    public class TimedPrematureSuccessDecorator : Decorator
    {
        protected float duration;
        protected float counter;
        public TimedPrematureSuccessDecorator(IScheduler s, float t) : base(s) => duration = t;
        public override void Enter()
        {
            counter = 0f;
            // because we need to actively keep time, we need to post ourselves onto the scheduler to have us run every frame.
            // and since we are running, our status matters, and we have to clear it when entering.
            Clear();
            scheduler.PostSchedule(this);
            Child.Enter();
        }
        public override void Step(float dt)
        {
            counter += dt;
            if (counter >= duration)
            {
                // since we are a posted decorator, we don't need to exit ourselves, the scheduler will do that for us.
                // The important thing is to mark our status accordingly. This timed decorator will count a premature abortion as a successful run.
                Status = NodeStatus.SUCCESS;
                Child.Abort();
                Parent.OnChildComplete(this, Status);
            }
            else Status = NodeStatus.RUNNING;
        }
        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            Status = status;
            Exit(Status);
        }
    }

    public class TimedPrematureFailureDecorator : TimedPrematureSuccessDecorator
    {
        public TimedPrematureFailureDecorator(IScheduler s, float t) : base(s, t) { }
        public override void Step(float dt)
        {
            counter += dt;
            if (counter >= duration)
            {
                // there is only one difference, a premature abortion is considered as a failure.
                Status = NodeStatus.FAILURE;
                Child.Abort();
                Parent?.OnChildComplete(this, Status);
            }
            else Status = NodeStatus.RUNNING;
        }
    }
}
