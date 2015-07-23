using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Billing
{ 
    public class PurchaseResponse
    {
        //Kaltura Payment Gateway response status code
        public ApiObjects.Response.Status Status { get; set; }

        //Kaltura unique ID representing the transaction
        public int TransactionID { get; set; }

        //Transaction reference ID that were returned from the payment gateway. Returned only if the payment gateway provides this information
        public string PGReferenceID { get; set; }

        //Original response ID that was provided from by the payment gateway. Returned only if the payment gateway provides this information.
        public string PGResponseID { get; set; }

        public PurchaseResponse()
        {
            Status = new ApiObjects.Response.Status();
        }

        public PurchaseResponse(PurchaseResponse pr)
        {
            this.TransactionID = pr.TransactionID;
            this.Status = pr.Status;
            this.PGResponseID = pr.PGResponseID;
            this.PGReferenceID = pr.PGReferenceID;
        }

        public PurchaseResponse(int statusCode, string statusMessage)
        {
            Status = new Response.Status(statusCode, statusMessage);
        }
    }
}
