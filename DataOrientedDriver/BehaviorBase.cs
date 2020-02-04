using System;

namespace DataOrientedDriver
{
    //base class.
    public abstract class Behavior : ISchedulable, IUtilized
    {
        public NodeStatus Status { get; protected set; }
        public float Utility { get; protected set; }

        public Behavior Parent;
        protected IScheduler scheduler;

        public Behavior(IScheduler s) => scheduler = s ?? throw new ArgumentNullException("scheduler");

        public virtual void Clear() { Status = NodeStatus.SUSPENDED; }
        public virtual void Abort() { Status = NodeStatus.ABORTED; }
        public virtual void CalculateUtility() { }

        public abstract void Enter();
        public abstract void Step(float dt);
        public abstract void Exit(NodeStatus status);
        public abstract void OnChildComplete(Behavior sender, NodeStatus childStatus);
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        public Action(IScheduler s) : base(s) { }
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }
        public override void Exit(NodeStatus status) { }
        public override void OnChildComplete(Behavior sender, NodeStatus childStatus) {}
    }
    public abstract class Condition : Behavior
    {
        public Condition(IScheduler s) : base(s) { }
        public override void Enter() { Clear(); scheduler.PostSchedule(this);  }
        public override void Exit(NodeStatus status) { }
        public override void OnChildComplete(Behavior sender, NodeStatus childStatus) {}
    }
}
