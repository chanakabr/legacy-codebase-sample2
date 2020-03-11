using ApiObjects.Response;
using Core.Catalog.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class SubscriptionSetsResponse
    {

        public Status Status;

        public List<SubscriptionSet> SubscriptionSets { get; set; }                

        public SubscriptionSetsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            SubscriptionSets = new List<SubscriptionSet>();            
        }

    }
}
