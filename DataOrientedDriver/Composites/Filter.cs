namespace DataOrientedDriver
{
    public class Filter : Sequence
    {
        public Filter(IScheduler s) : base(s) { }

        public void AddCondition(Behavior condition) { Children.Insert(0, condition); condition.Parent = this; }
        public void AddAction(Behavior action) { Children.Add(action); action.Parent = this; }
    }
}
