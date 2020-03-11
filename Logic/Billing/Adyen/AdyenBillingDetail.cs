using APILogic.AdyenRecAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public class AdyenBillingDetail
    {
       
        public BillingInfo billingInfo;
        public AdyenBillingDetail()
        {
          
            billingInfo = null;
        }
        public void Initialize(RecurringDetail detail)
        {
            if (detail != null && detail.card != null)
            {
                billingInfo = new BillingInfo(detail.card.expiryMonth, detail.card.expiryYear, detail.card.number, detail.card.holderName, detail.card.cvc, detail.variant);
            }
           
        }


    }
     public class BillingInfo
    {
        public string expiryMonth;
        public string expiryYear;
        public string lastFourDigits;
        public string holderName;
        public string cvc;
        public string variant;
        public BillingInfo()
        {
            expiryMonth = "";
            expiryYear = "";
            lastFourDigits = "";
            holderName = "";
            cvc = "";
            variant = "";

        }
        public  BillingInfo(string sexpiryMonth, string sexpiryYear, string slastFourDigits, string sholderName, string scvc, string svariant)
        {
            expiryMonth = sexpiryMonth;
            expiryYear = sexpiryYear;
            lastFourDigits = slastFourDigits;
            holderName = sholderName;
            cvc = scvc;
            variant = svariant;

        }
        
      

       

  
       
    }
}
