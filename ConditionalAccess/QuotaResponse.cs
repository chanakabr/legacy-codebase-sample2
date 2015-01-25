using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class QuotaResponse : NPVRResponse
    {
        public long totalQuota;
        public long occupiedQuota;
    }
}
