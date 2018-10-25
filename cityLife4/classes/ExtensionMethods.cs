using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public static class ExtensionMethods
    {
        public static bool IsWeekend(this DateTime aDate)
        {
            return (aDate.DayOfWeek.Equals(DayOfWeek.Saturday) || aDate.DayOfWeek.Equals(DayOfWeek.Sunday));
            
        }
        
    }
}