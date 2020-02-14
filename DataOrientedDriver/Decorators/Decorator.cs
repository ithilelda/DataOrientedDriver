namespace DataOrientedDriver
{
    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;

        protected Decorator(IScheduler s) : base(s) { }

        public void SetChild(Behavior child) { Child = child; Child.Parent = this; }
        public Behavior GetChild() => Child;
        public override void Abort() { base.Abort(); Child.Abort(); }
        public override void Step(float dt) { } // because most decorators are not posted, this will never be called.
        public override void Enter()
        {
            // Most decorators save time by not posting itself.
            Child.Enter(); //Enters our child the same frame we enter.
        }
        public override void Exit(NodeStatus status) {}
    }
}
