using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.ConditionalAccess
{
    public class TransactionResult
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionID { get; set; }

        [JsonProperty(PropertyName = "pg_reference_id")]
        public string PGReferenceID { get; set; }

        [JsonProperty(PropertyName = "pg_response_code")]
        public string PGResponseCode { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "fail_reason_code")]
        public int FailReasonCode { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public long CreatedAt { get; set; }

        public TransactionResult(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.TransactionResponse transactionResponse)
        {
            if (transactionResponse != null)
            {
                this.FailReasonCode = transactionResponse.FailReasonCode;
                this.PGReferenceID = transactionResponse.PGReferenceID;
                this.PGResponseCode = transactionResponse.PGResponseCode;
                this.State = transactionResponse.State;
                this.TransactionID = transactionResponse.TransactionID;
                this.CreatedAt = transactionResponse.CreatedAt;
            }
        }

        public TransactionResult()
        {

        }
    }
}
