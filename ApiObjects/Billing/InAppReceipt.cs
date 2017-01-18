using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects.Billing
{
    [Serializable]
    public class InAppReceipt
    {
        #region Member

        
        string m_status;

        iTunesReceipt m_receipt;
        iTunesReceipt m_latest_expired_receipt_info;
        List<iTunesReceipt> m_latest_receipt_info;
        List<iTunesReceipt> m_in_app;
        string m_latest_receipt;

        #endregion


        #region Methods
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
        public List<iTunesReceipt> latest_receipt_info
        {
            get { return m_latest_receipt_info; }
            set { m_latest_receipt_info = value; }
        }

        [DataMember]
        public List<iTunesReceipt> in_app
        {
            get
            {
                return m_in_app;
            }
            set
            {
                m_in_app = value;
            }
        }

        /// <summary>
        /// Which iOs Version does this InApp Receipt corresponds to. It is important because the structure of the receipt is completely different
        /// </summary>
        [DataMember]
        public string iOSVersion;

        #endregion
    }
}
