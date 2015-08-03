using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class TransactResult
    {
        public ApiObjects.Response.Status Status { get; set; }

        //Kaltura unique ID representing the transaction
        public long TransactionID { get; set; }

        //Transaction reference ID that were returned from the payment gateway. Returned only if the payment gateway provides this information
        public string PGReferenceID { get; set; }

        //Original response ID that was provided from by the payment gateway. Returned only if the payment gateway provides this information.
        public string PGResponseID { get; set; }

        public eTransactionState State { get; set; }

        public TransactResult()
        {
        }


    }
}
