using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WS_Billing
{
    /// <summary>
    /// Constants that passes to the query string key "desc"
    /// of the redirected page after adyen purchased
    /// </summary>
    public static class AdyenRedirectMessages
    {
        public const string OK= "OK";        
        public const string USER_CANCELLED = "User cancelled";           
        public const string DOUBLE_CC = "DoubleCC";        
        public const string ITEM_PURCHASED_ERROR= "Item purchase error";        
        public const string ITEM_ALREADY_PURCHASED = "Item already purchased";        
        public const string COUPON_ALREADY_USED = "Item purchase error,Coupon alreday used";        
        public const string INVALID_ERROR = "Invalid error";
        public const string INVALID_MERCHANT = "Invalid merchant";
        public const string REFUSED_TRANSACTION = "Refused transaction";
        public const string INVALID_ACCOUNT = "Invalid account";
    }
}