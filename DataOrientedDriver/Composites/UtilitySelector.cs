using System;
using System.Collections.Generic;
using System.Text;

namespace DataOrientedDriver
{
    public class UtilitySelector : Selector
    {
        public UtilitySelector(IScheduler s) : base(s) { }

        public override void Enter()
        {
            Clear();
            // TODO: need implementation.
        }
    }
}
