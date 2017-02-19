using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class Compensation
    {
        public CompensationType CompensationType { get; set; }
       
        public int Amount { get; set; }

        public int TotalRenewals { get; set; }

        public int Renewals { get; set; }
    }
}
