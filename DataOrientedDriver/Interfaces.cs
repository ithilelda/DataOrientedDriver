
namespace DataOrientedDriver
{
    public interface ISchedulable
    {
        NodeStatus Status { get; }
        void Step(float dt); // called by the scheduler each frame.
    }
    public interface IScheduler
    {
        void PostSchedule(ISchedulable schedule);
    }
    public interface IUtilized
    {
        float Utility { get; }
        void CalculateUtility();
    }
}
