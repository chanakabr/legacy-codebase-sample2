using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiModule.Objects.Responses.ConditionalAccess
{
    public class DomainsBillingTransactionsResponse
    {
        [JsonProperty(PropertyName = "status")]       
        public Status status;

        [JsonProperty(PropertyName = "billing_transactions")]
        public DomainBillingTransactionsResponse[] billingTransactions;

        /// <summary>
        /// Create an instance of thie response type based on the WS_CAS response
        /// </summary>
        /// <param name="source"></param>
        public DomainsBillingTransactionsResponse(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainsBillingTransactionsResponse source)
        {
            if (source != null)
            {
                if (source.status != null)
                {
                    this.status = new Status(source.status.Code, source.status.Message);
                }

                if (source.billingTransactions != null)
                {
                    this.billingTransactions = source.billingTransactions.ToArray();
                }
            }
        }
        
    }
}
