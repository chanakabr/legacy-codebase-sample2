using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ExternalEntitlement
    {
        public long PurchaseId { get; set; }

        public long ProductId { get; set; }

        public string ProductCode { get; set; }

        public eTransactionType EntitlementType { get; set; }

        public string ContentId { get; set; }

        public long StartDateSeconds { get; set; }

        public long EndDateSeconds { get; set; }
    }

    public class OSSAdapterEntitlementsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<ExternalEntitlement> Entitlements { get; set; }

        public OSSAdapterEntitlementsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());            
        }
    }
}
