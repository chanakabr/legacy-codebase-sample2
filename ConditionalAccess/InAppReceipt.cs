using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ConditionalAccess
{
    [Serializable]
    public class InAppReceipt
    {
        #region Member

        string m_status;

        iTunesReceipt m_receipt;
        iTunesReceipt m_latest_expired_receipt_info;
        iTunesReceipt m_latest_receipt_info;
        string m_latest_receipt;

        #endregion

        #region Properties
        /// <summary>
        /// Get or set latest receipt info Based 64 bit.
        /// </summary>
        [DataMember]
        public string latest_receipt
        {
            get { return m_latest_receipt; }
            set { m_latest_receipt = value; }
        }
        /// <summary>
        /// Get or set valid receipt status, status key is 0 this is valid receipt. 
        /// if the value is anuthing other than 0, this reeipt is invalid.
        /// </summary>
        [DataMember]
        public string Status
        {
            get { return m_status; }
            set { m_status = value; }
        }
        [DataMember]
        public iTunesReceipt receipt
        {
            get { return m_receipt; }
            set { m_receipt = value; }
        }
        [DataMember]
        public iTunesReceipt latest_expired_receipt_info
        {
            get { return m_latest_expired_receipt_info; }
            set { m_latest_expired_receipt_info = value; }
        }
        [DataMember]
        public iTunesReceipt latest_receipt_info
        {
            get { return m_latest_receipt_info; }
            set { m_latest_receipt_info = value; }
        }


        #endregion
    }
}
