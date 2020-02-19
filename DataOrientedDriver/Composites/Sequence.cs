

namespace DataOrientedDriver
{
    public class Sequence : Composite
    {
        public Sequence(IScheduler s) : base(s) { }

        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // if the child return with success, we proceed to the next.
            if (status == NodeStatus.SUCCESS)
            {
                // if we have more, we move on.
                if (ChildIterator.MoveNext())
                {
                    ChildIterator.Current.Enter();
                }
                // otherwise, we say that we have successfully completed all sequence, and exit with success.
                else
                {
                    Exit(status);
                }
            }
            // if it failed, or is aborted, we exit and propagate the code.
            else
            {
                Exit(status);
            }
        }
    }
}
