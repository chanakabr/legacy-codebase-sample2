using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects.Billing
{
    public class iTunesReceipt
    {
        #region Member

        string m_original_purchase_date_pst;
        string m_original_transaction_id;
        string m_expires_date;
        string m_expires_date_ms;
        string m_teansaction_id;
        string m_quantity;
        string m_product_id;
        string m_bvrs;
        string m_bid;
        string m_web_order_line_item_id;
        string m_item_id;
        string m_expires_date_formatted;
        string m_purchase_date;
        string m_purchase_date_ms;
        string m_expires_date_formatted_pst;
        string m_purchase_date_pst;
        string m_original_purchase_date;
        string m_original_purchase_date_ms;
        string m_version_external_identifier;



        #endregion

        #region Methods
        #endregion

        #region Properties
        /// <summary>
        /// Get or set the original purchase date PST
        /// </summary>
        [DataMember]
        public string original_purchase_date_pst
        {
            get { return m_original_purchase_date_pst; }
            set { m_original_purchase_date_pst = value; }
        }
        /// <summary>
        /// Get or set for a transatction that restores a previous transaction, this holds the original transaction identifier.
        /// </summary>
        [DataMember]
        public string Original_transaction_id
        {
            get { return m_original_transaction_id; }
            set { m_original_transaction_id = value; }
        }
        /// <summary>
        /// Get or set the date expires of the item that was purchased.
        /// </summary>
        [DataMember]
        public string expires_date
        {
            get { return m_expires_date; }
            set { m_expires_date = value; }

        }
        /// <summary>
        /// Get or set the transaction identifier of the item that was purchased.
        /// </summary>
        [DataMember]
        public string transaction_id
        {
            get { return m_teansaction_id; }
            set { m_teansaction_id = value; }

        }
        /// <summary>
        /// Get or set the number of items purchased.
        /// </summary>
        [DataMember]
        public string quantity
        {
            get { return m_quantity; }
            set { m_quantity = value; }
        }
        /// <summary>
        /// Get or set the product identifier of the item that was purchased.
        /// </summary>
        [DataMember]
        public string product_id
        {
            get { return m_product_id; }
            set { m_product_id = value; }
        }
        /// <summary>
        /// Get or set a version number for the application.
        /// </summary>
        [DataMember]
        public string bvrs
        {
            get { return m_bvrs; }
            set { m_bvrs = value; }
        }
        /// <summary>
        /// Get or set the bundle identifier for the application.
        /// </summary>
        [DataMember]
        public string bid
        {
            get { return m_bid; }
            set { m_bid = value; }
        }
        /// <summary>
        /// Get or set the web order line item id.
        /// </summary>
        [DataMember]
        public string web_order_line_item_id
        {
            get { return m_web_order_line_item_id; }
            set { m_web_order_line_item_id = value; }
        }
        /// <summary>
        /// Get or set the uniquely identify the application that created the payment transaction.
        /// </summary>
        [DataMember]
        public string item_id
        {
            get { return m_item_id; }
            set { m_item_id = value; }
        }
        /// <summary>
        /// Get or set the date expires formatted.
        /// </summary>
        [DataMember]
        public string expires_date_formatted
        {
            get { return m_expires_date_formatted; }
            set { m_expires_date_formatted = value; }
        }
        /// <summary>
        /// Get or set the date and time this transation eccurred.
        /// </summary>
        [DataMember]
        public string purchase_date
        {
            get { return m_purchase_date; }
            set { m_purchase_date = value; }
        }
        /// <summary>
        /// Get or set the date and time this transation eccurred in millisecond.
        /// </summary>
        [DataMember]
        public string purchase_date_ms
        {
            get { return m_purchase_date_ms; }
            set { m_purchase_date_ms = value; }
        }
        /// <summary>
        /// Get or set the date expires formatted PST
        /// </summary>
        [DataMember]
        public string expires_date_formatted_pst
        {
            get { return m_expires_date_formatted_pst; }
            set { m_expires_date_formatted_pst = value; }
        }
        /// <summary>
        /// Get or set the date and time this transation eccurred PST.
        /// </summary>
        [DataMember]
        public string purchase_date_pst
        {
            get { return m_purchase_date_pst; }
            set { m_purchase_date_pst = value; }
        }
        /// <summary>
        /// Get or set the date and time of original transation.
        /// </summary>
        [DataMember]
        public string original_purchase_date
        {
            get { return m_original_purchase_date; }
            set { m_original_purchase_date = value; }
        }
        /// <summary>
        /// Get or set the date and time of original transation in millisecond.
        /// </summary>
        [DataMember]
        public string original_purchase_date_ms
        {
            get { return m_original_purchase_date_ms; }
            set { m_original_purchase_date_ms = value; }
        }
        /// <summary>
        /// Get or set an arbitrary number that uniquely identifies a version of your application.
        /// </summary>
        [DataMember]
        public string version_external_identifier
        {
            get { return m_version_external_identifier; }
            set { m_version_external_identifier = value; }
        }

        /// <summary>
        /// Get or set the expires date ms
        /// </summary>
        [DataMember]
        public string expires_date_ms
        {
            get
            {
                return m_expires_date_ms;
            }
            set
            {
                m_expires_date_ms = value;
            }
        }


        #endregion

    }
}
