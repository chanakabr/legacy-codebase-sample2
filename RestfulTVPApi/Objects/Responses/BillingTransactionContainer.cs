using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class BillingTransactionContainer
    {
        public string reciept_code { get; set; }

        public string purchased_item_name { get; set; }
        
        public string purchased_item_code { get; set; }
        
        public BillingItemsType item_type { get; set; }
        
        public BillingAction billing_action { get; set; } 
        
        public Price price { get; set; }
        
        public System.DateTime action_date { get; set; }
       
        public System.DateTime start_date { get; set; }
        
        public System.DateTime end_date { get; set; }
        
        public PaymentMethod payment_method { get; set; }

        public string payment_method_extra_details { get; set; }
       
        public bool is_recurring { get; set; }
       
        public int billing_provider_ref { get; set; }
       
        public int purchase_id  { get; set; }
       
        public string remarks { get; set; } 
    }

}
