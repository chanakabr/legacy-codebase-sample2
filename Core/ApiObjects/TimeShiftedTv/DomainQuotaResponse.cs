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

        private int _used;
        public int Used
        {
            get
            {
                return _used;
            }
            set
            {
                _used = _used == default ? (int)Math.Ceiling((double)(this.TotalQuota - this.AvailableQuota)) : value;
            }
        }
    }
}
