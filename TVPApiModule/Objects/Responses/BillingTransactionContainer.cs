using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class BillingTransactionContainer
    {
        public string recieptCode { get; set; }

        public string purchasedItemName { get; set; }
        
        public string purchasedItemCode { get; set; }
        
        public BillingItemsType itemType { get; set; }
        
        public BillingAction billingAction { get; set; } 
        
        public Price price { get; set; }
        
        public System.DateTime actionDate { get; set; }
       
        public System.DateTime startDate { get; set; }
        
        public System.DateTime endDate { get; set; }
        
        public PaymentMethod paymentMethod { get; set; }

        public string paymentMethodExtraDetails { get; set; }
       
        public bool isRecurring { get; set; }
       
        public int billingProviderRef { get; set; }
       
        public int purchaseID  { get; set; }
       
        public string remarks { get; set; } 
    }

}
