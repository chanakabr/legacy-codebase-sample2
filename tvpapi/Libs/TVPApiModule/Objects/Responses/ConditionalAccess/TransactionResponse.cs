using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.ConditionalAccess
{
    public class TransactionResponse
    {
        [JsonProperty(PropertyName = "result")]
        public TransactionResult Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public TransactionResponse(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.TransactionResponse transaction)
        {
            if (transaction != null)
            {
                this.Status = new Responses.Status(transaction.Status.Code, transaction.Status.Message);
                this.Result = new TransactionResult(transaction);
            }
        }

        public TransactionResponse()
        {
        }
    }
}
