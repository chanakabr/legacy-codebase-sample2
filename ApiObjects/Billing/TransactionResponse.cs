using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Billing
{
    public class TransactionResponse : CoreObject
    {
        //Kaltura purchase response status
        public ApiObjects.Response.Status Status { get; set; }

        //Kaltura unique ID representing the transaction
        public string TransactionID { get; set; }

        //Transaction reference ID that were returned from the payment gateway. Returned only if the payment gateway provides this information
        public string PGReferenceID { get; set; }

        //Original response ID that was provided from by the payment gateway. Returned only if the payment gateway provides this information.
        public string PGResponseCode { get; set; }

        public string State { get; set; }

        // Adapter failure reason code 
        public int FailReasonCode { get; set; }

        public long CreatedAt { get; set; }

        public long StartDateSeconds { get; set; }

        public long EndDateSeconds { get; set; }

        public bool AutoRenewing { get; set; }

        public TransactionResponse()
        {
            Status = new ApiObjects.Response.Status();
        }

        public TransactionResponse(TransactionResponse transactionResponse)
        {
            this.TransactionID = transactionResponse.TransactionID;
            this.Status = transactionResponse.Status;
            this.PGResponseCode = transactionResponse.PGResponseCode;
            this.PGReferenceID = transactionResponse.PGReferenceID;
        }

        public TransactionResponse(int statusCode, string statusMessage)
        {
            Status = new Response.Status(statusCode, statusMessage);
        }

        public override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        public override bool DoUpdate()
        {
            throw new NotImplementedException();
        }

        public override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }
    }
}
