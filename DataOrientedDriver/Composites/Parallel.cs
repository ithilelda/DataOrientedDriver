using System;
using System.Collections.Generic;
using System.Linq;

namespace DataOrientedDriver
{
    public class Parallel : Composite
    {
        public enum Policy
        {
            Ignore,
            One,
            All
        }
        protected Policy successPolicy, failurePolicy;
        public HashSet<ISchedulable> succeeded = new HashSet<ISchedulable>();
        public HashSet<ISchedulable> failed = new HashSet<ISchedulable>();
        public Parallel(IScheduler s, Policy sucPolicy = Policy.Ignore, Policy failPolicy = Policy.One) : base(s)
        {
            successPolicy = sucPolicy;
            failurePolicy = failPolicy;
        }

        public override void Clear()
        {
            succeeded.Clear();
            failed.Clear();
        }
        public override void Enter()
        {
            // the parallel node itself is not posted either, so we only enter all of our children.
            foreach (var child in Children)
            {
                child.Enter();
            }
        }
        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // we value success first.
            if (status == NodeStatus.SUCCESS)
            {
                // if our success policy is one, then any successful child will make us propagate the success code and abort everyone else.
                if (successPolicy == Policy.One)
                {
                    AbortChildren();
                    Parent.OnChildComplete(this, status);
                }
                // if we need everyone to succeed, then we have to go on processing.
                else if (successPolicy == Policy.All)
                {
                    // this child may fail previously, but as long as we don't exit, we should remove it from the failed set and add it to the success set.
                    failed.Remove(sender);
                    succeeded.Add(sender);
                    // if every child succeeded, we abort and exit with success.
                    if (succeeded.Count == Children.Count)
                    {
                        AbortChildren();
                        Parent.OnChildComplete(this, status);
                    }
                    // if not, we ask it to keep running again.
                    else sender.Enter();
                }
                
            }
            // the failure part is essentially the same.
            else if (status == NodeStatus.FAILURE)
            {
                if (failurePolicy == Policy.One)
                {
                    AbortChildren();
                    Parent.OnChildComplete(this, status);
                }
                else if (failurePolicy == Policy.All)
                {
                    succeeded.Remove(sender);
                    failed.Add(sender);
                    if (failed.Count == Children.Count)
                    {
                        AbortChildren();
                        Parent.OnChildComplete(this, status);
                    }
                    else sender.Enter();
                }
            }
        }

        protected void AbortChildren()
        {
            foreach (var child in Children)
            {
                if (child.Status == NodeStatus.RUNNING || child.Status == NodeStatus.SUSPENDED) child.Abort();
            }
        }
    }
}
