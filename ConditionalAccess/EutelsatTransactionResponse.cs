using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ConditionalAccess
{
    public class EutelsatTransactionResponse
    {
        
        //"http://82.79.128.235:8080/TvinciService.svc/transaction/create.json?user_id={userId}&price={price}&currency={currency}&asset_id={assetId}&ppv_module_code={ppvModuleCode}&coupon_code={couponCode}&rovi_id={roviId}&device_brand={deviceBrand}&transaction_id={transactionId}"


        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
    }
}
