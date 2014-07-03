using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Notifiers;

namespace Notifiers
{

    public class EutelsatProductNotification
    {
        [JsonProperty("product")]
        public EutelsatProduct Product;
    }

    [JsonObject(Title = "product")]
    public class EutelsatProduct
    {
        [JsonProperty("external_product_id")]
        public string ExternalProductID { get; set; }

        [JsonProperty("internal_product_id")]
        public int InternalProductID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("package_Ids")]
        public List<ObjPackageID> PackageIDs { get; set; }

        [JsonProperty("price_b2c")]
        public double PriceB2C { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("IPNO_Ids")]
        public List<ObjIPNO_ID> IPNO_IDs { get; set; }

    }

    public class EutelsatPackageNotification
    {
        [JsonProperty("product")]
        public EutelsatPackage Product;
    }

    [JsonObject(Title = "product")]
    public class EutelsatPackage
    {
        [JsonProperty("external_product_id")]
        public string ExternalProductID { get; set; }

        [JsonProperty("internal_product_id")]
        public int InternalProductID { get; set; }

        [JsonProperty("IPNO_id")]
        public string IPNO_ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("logo")]
        public string LogoUrl { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }
    }

    public class ObjPackageID
    {
        [JsonProperty("package_Id")]
        public int PackageID { get; set; }
    }

    public class ObjIPNO_ID
    {
        [JsonProperty("IPNO_Id")]
        public string IPNO_ID { get; set; }
    }


    //public class EutelsatTransactionResponse
    //{

    //    //"http://82.79.128.235:8080/TvinciService.svc/transaction/create.json?user_id={userId}&price={price}&currency={currency}&asset_id={assetId}&ppv_module_code={ppvModuleCode}&coupon_code={couponCode}&rovi_id={roviId}&device_brand={deviceBrand}&transaction_id={transactionId}"


    //    [JsonProperty("success")]
    //    public bool Success { get; set; }

    //    //[JsonProperty("error_code")]
    //    //public int ErrorCode { get; set; }

    //    [JsonProperty("message")]
    //    public string Message { get; set; }

    //    //[JsonProperty("transaction_id")]
    //    //public string TransactionId { get; set; }
    //}

    public class ProductNotificationResponse
    {
        [JsonProperty("success")]
        public bool success { get; set; }

        [JsonProperty("errors")]
        public EutelsatError[] errors { get; set; }
    }

    public class PackageNotificationResponse
    {
        [JsonProperty("success")]
        public bool success { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        //[JsonProperty("errors")]
        //public EutelsatError[] errors { get; set; }
    }

    public class EutelsatError
    {
        [JsonProperty("error_message")]
        public string error_message { get; set; }

        [JsonProperty("error_type")]
        public string error_type { get; set; }
    }

}
