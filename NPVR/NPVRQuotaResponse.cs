using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRQuotaResponse
    {
        public string entityID;
        public bool isOK;
        public long totalQuota;
        public long usedQuota;
        public string msg;
    }
}
