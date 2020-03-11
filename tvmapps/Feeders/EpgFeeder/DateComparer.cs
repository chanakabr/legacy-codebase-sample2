using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpgFeeder
{
    public class DateComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime dt1, DateTime dt2)
        {
            return DateTime.Equals(dt1, dt2);
        }
        public int GetHashCode(DateTime dt)
        {
            return dt.GetHashCode();
        }
    }
}
