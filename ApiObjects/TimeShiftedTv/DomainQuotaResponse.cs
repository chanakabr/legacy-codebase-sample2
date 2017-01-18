using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class DomainQuotaResponse
    {
        public ApiObjects.Response.Status Status
        {
            get;
            set;
        }

        public int AvailableQuota
        {
            get;
            set;
        }


        public int TotalQuota
        {
            get;
            set;
        }
    }
}
