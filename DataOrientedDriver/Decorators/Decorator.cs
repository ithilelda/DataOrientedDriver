

namespace DataOrientedDriver
{
    //branch nodes.
    public abstract class Decorator : Behavior
    {
        public Behavior Child
        {
            get => _child;
            set
            {
                _child = value; // add the new.
                _child.Parent = this; // set the new child's parent to ourself.
            }
        }

        private Behavior _child;

        protected Decorator(IScheduler s) : base(s) { }
        public override void Abort() { base.Abort(); Child.Abort(); }
        public override void Step(float dt) { } // because most decorators are not posted, this will never be called.
        public override void Enter() { Child.Enter(); }
    }
}
