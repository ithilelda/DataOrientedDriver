using System.Collections.Generic;


namespace DataOrientedDriver
{
    public abstract class Composite : Behavior
    {
        protected IEnumerator<Behavior> ChildIterator;
        protected List<Behavior> Children = new List<Behavior>();

        public Composite(IScheduler s) : base(s) { }

        public void AddChild(Behavior child) { Children.Add(child); child.Parent = this; }
        public void RemoveChild(Behavior child) { Children.Remove(child); child.Parent = null; }
        public List<Behavior> GetChildren() => Children;
        public void ClearChildren()
        {
            foreach (var child in Children)
            {
                child.Parent = null;
            }
            Children.Clear();
        }
        public override void Abort()
        {
            base.Abort();
            foreach (var child in Children)
            {
                child.Abort();
            }
        }
        public override void Step(float dt) { }
        public override void Enter()
        {
            ChildIterator = Children.GetEnumerator();
            // most composites don't need to be posted either, so we only proceed to ask our child to enter.
            // if there is no child, we just silently ignore the execution and proceed. This way, our code doesn't have to deal with malformed trees.
            if (ChildIterator.MoveNext()) ChildIterator.Current.Enter();
        }
    }
}
