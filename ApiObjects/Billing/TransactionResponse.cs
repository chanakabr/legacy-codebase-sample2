using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Billing
{ 
    public class TransactionResponse
    {
        //Kaltura Payment Gateway response status code
        public ApiObjects.Response.Status Status { get; set; }

        //Kaltura unique ID representing the transaction
        public int TransactionID { get; set; }

        //Transaction reference ID that were returned from the payment gateway. Returned only if the payment gateway provides this information
        public string PGReferenceID { get; set; }

        //Original response ID that was provided from by the payment gateway. Returned only if the payment gateway provides this information.
        public string PGResponseID { get; set; }

        public string State { get; set; }

        public TransactionResponse()
        {
            Status = new ApiObjects.Response.Status();
        }

        public TransactionResponse(TransactionResponse transactionResponse)
        {
            this.TransactionID = transactionResponse.TransactionID;
            this.Status = transactionResponse.Status;
            this.PGResponseID = transactionResponse.PGResponseID;
            this.PGReferenceID = transactionResponse.PGReferenceID;
        }

        public TransactionResponse(int statusCode, string statusMessage)
        {
            Status = new Response.Status(statusCode, statusMessage);
        }
    }
}
