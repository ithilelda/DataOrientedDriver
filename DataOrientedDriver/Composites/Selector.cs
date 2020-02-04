namespace DataOrientedDriver
{
    public class Selector : Composite
    {
        public Selector(IScheduler s) : base(s) { }

        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // we exit as soon as our child returns successfully.
            if (status == NodeStatus.SUCCESS)
            {
                Parent.OnChildComplete(this, status); // same as decorators. Because the selector is not posted, we have to exit ourselves.
            }
            // we move to the next child if the previous one failed or is aborted.
            else
            {
                // if we have more, we enters our next child.
                if (ChildIterator.MoveNext())
                {
                    ChildIterator.Current.Enter();
                }
                // if we don't have any left, we exit with failure.
                else
                {
                    Parent.OnChildComplete(this, status);
                }
            }
        }
    }
}
