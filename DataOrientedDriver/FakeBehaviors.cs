
namespace DataOrientedDriver
{
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public FakeSuccessAction(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.SUCCESS; Exit(Status); }
    }
    public class FakeFailureAction : Action
    {
        public FakeFailureAction(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.FAILURE; Exit(Status); }
    }
    public class AlwaysTrueCondition : Condition
    {
        public AlwaysTrueCondition(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.SUCCESS; Exit(Status); }
    }
    public class AlwaysFalseCondition : Condition
    {
        public AlwaysFalseCondition(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.FAILURE; Exit(Status); }
    }
}
