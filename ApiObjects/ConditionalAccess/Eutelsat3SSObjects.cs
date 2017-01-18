using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.ConditionalAccess
{

    public class EutelsatCheckTvodRequest
    {
        [JsonProperty("tvod")]
        public EutelsatCheckTvod CheckTvod;
    }

    [JsonObject(Title = "check_tvod")]
    public class EutelsatCheckTvod
    {
        [JsonProperty("user_id")]
        public string UserID                { get; set; }

        [JsonProperty("price")]
        public double Price                 { get; set; }

        [JsonProperty("currency")]
        public string Currency              { get; set; }

        [JsonProperty("asset_id")]
        public string AssetID               { get; set; }

        [JsonProperty("IPNO_Id")]
        public string IPNO_ID               { get; set; }

        [JsonProperty("contract_id")]
        public string ContractID            { get; set; }

        [JsonProperty("tmp_site_guid_eutelsat")]
        public string SiteGUID              { get; set; }

    }

    public class EutelsatTransactionRequest
    {
        [JsonProperty("transaction")]
        public EutelsatTransaction Transaction;
    }

    [JsonObject(Title = "transaction")]
    public class EutelsatTransaction
    {
        [JsonProperty("user_id")]
        public string UserID                { get; set; }

        [JsonProperty("price")]
        public double Price                 { get; set; }

        [JsonProperty("currency")]
        public string Currency              { get; set; }

        [JsonProperty("asset_id")]
        public int AssetID                  { get; set; }

        [JsonProperty("ppv_module_code")]
        public string PPVModuleCode         { get; set; }

        [JsonProperty("coupon_code")]
        public string CouponCode            { get; set; }

        [JsonProperty("rovi_id")]
        public string RoviID                { get; set; }
            
        [JsonProperty("device_brand")]
        public int DeviceBrandID            { get; set; }

        [JsonProperty("device_id")]
        public string DeviceUDID            { get; set; }

        [JsonProperty("transaction_id")]
        public int TransactionID            { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType       { get; set; }

    }

    public class EutelsatSubRequest
    {
        [JsonProperty("subscription")]
        public EutelsatSubscription Subscription;
    }

    [JsonObject(Title = "subscription")]
    public class EutelsatSubscription
    {
        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("product_id")]
        public string ProductID { get; set; }

        [JsonProperty("device_id")]
        public string DeviceUDID { get; set; }
    }


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
