using System.Linq;


namespace DataOrientedDriver
{
    public class UtilitySelector : Composite
    {
        protected IUtilizer Utilizer;
        public UtilitySelector(IScheduler s, IUtilizer u) : base(s) { Utilizer = u; }

        public override void Enter()
        {
            var selected = Utilizer.Select(Children);
            if(selected != null) ((Behavior)selected).Enter();
        }
        public override void OnChildComplete(Behavior sender, NodeStatus status)
        {
            // we exit as soon as our child returns successfully.
            if (status == NodeStatus.SUCCESS)
            {
                Exit(status); // same as decorators. Because the selector is not posted, we have to exit ourselves.
            }
            // we move to the next child if the previous one failed or is aborted.
            else
            {
                var selected = Utilizer.Select(Children.Where(b => b != sender));
                // if the utilizer returned a valid selection, we continue.
                if(selected != null)
                {
                    ((Behavior)selected).Enter();
                }
                // otherwise, we exit with failure.
                else Exit(status);
            }
        }
    }
}
