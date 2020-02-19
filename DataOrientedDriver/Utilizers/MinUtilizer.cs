using System.Linq;
using System.Collections.Generic;


namespace DataOrientedDriver
{
    public class MinUtilizer : IUtilizer
    {
        public IUtility Select(IEnumerable<IUtility> utils)
        {
            return utils.OrderBy(u => u.Utility).FirstOrDefault();
        }
    }
}