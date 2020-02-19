using System.Collections.Generic;


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
    public interface IUtility
    {
        float Utility { get; }
        void CalculateUtility();
    }
    public interface IUtilizer
    {
        IUtility Select(IEnumerable<IUtility> utils);
    }
}
