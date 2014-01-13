using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class AdyenBillingDetail
    {
        public BillingInfo billingInfo { get; set; }
    }

    public class BillingInfo
    {
        public string expiryMonth { get; set; }
           
        public string expiryYear { get; set; }
            
        public string lastFourDigits { get; set; }
           
        public string holderName { get; set; }
            
        public string cvc { get; set; }
          
        public string variant { get; set; }
    }
}
