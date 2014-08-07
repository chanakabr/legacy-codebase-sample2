using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class Waiver
    {   
        public int nWaiver { get; set; }
        public DateTime dWaiverDate { get; set; }
        public DateTime dCancellationDate { get; set; }

        public Waiver()
        {
            nWaiver = 0;
            dWaiverDate = new DateTime(2000, 1, 1);
            dCancellationDate = new DateTime(2000, 1, 1);

        }
    }
}
