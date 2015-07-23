using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PurchaseResponse
    {
         //Kaltura Payment Gateway response status code     //public int ResponseID {get; set;}
        public ApiObjects.Response.Status Status { get; set; }

        //Kaltura unique ID representing the transaction
        public int TransactionID { get; set; }
        
        //Transaction reference ID that were returned from the payment gateway. Returned only if the payment gateway provides this information
        public string PGReferenceID {get; set;}

        //Original response ID that was provided from by the payment gateway. Returned only if the payment gateway provides this information.
        public string PGResponseID { get; set; }

        public PurchaseResponse()
        {
            Status = new ApiObjects.Response.Status();            
        }
    }
}
