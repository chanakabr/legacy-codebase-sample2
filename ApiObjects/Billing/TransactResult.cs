using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class TransactResult
    {
        public ApiObjects.Response.Status Status { get; set; }

        /// <summary>
        /// Kaltura unique ID representing the transaction
        /// </summary>
        public long TransactionID { get; set; }

        /// <summary>
        /// Transaction reference ID that were returned from the payment gateway. Returned only if the payment gateway provides this information
        /// </summary>
        public string PGReferenceID { get; set; }

        /// <summary>
        /// Original response ID that was provided from by the payment gateway. Returned only if the payment gateway provides this information.
        /// </summary>
        public string PGResponseID { get; set; }

        /// <summary>
        /// OK, Pending, Failed...
        /// </summary>
        public eTransactionState State { get; set; }

        /// <summary>
        /// Adapter failure reason code 
        /// </summary>
        public int FailReasonCode { get; set; }

        public string PaymentDetails
        {
            get;
            set;
        }

        /// <summary>
        /// Credit card, cellular etc. (free text)
        /// </summary>
        public string PaymentMethod
        {
            get;
            set;
        }

        /// <summary>
        /// epoch - transaction date 
        /// </summary>
        public long StartDateSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// epoch - purchase is no longer valid
        /// </summary>
        public long EndDateSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// Does the subscription automatically renew or not
        /// </summary>
        public bool AutoRenewing
        {
            get;
            set;
        }

        public TransactResult()
        {
            Status = new Response.Status();
        }
    }
}