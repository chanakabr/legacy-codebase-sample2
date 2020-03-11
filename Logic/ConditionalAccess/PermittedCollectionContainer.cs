using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using Core.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class PermittedCollectionContainer
    {
        public string m_sCollectionCode;
        public DateTime m_dEndDate;
        public DateTime m_dCurrentDate;
        public DateTime m_dLastViewDate;
        public DateTime m_dPurchaseDate;
        public Int32 m_nCollectionPurchaseID;
        public ePaymentMethod m_paymentMethod;

        public string m_sDeviceUDID;
        public string m_sDeviceName;

        public bool m_bCancelWindow;


        public PermittedCollectionContainer()
        {
            m_sCollectionCode       = "";
            m_dEndDate              = new DateTime(2099 ,1 , 1);
            m_dCurrentDate          = DateTime.UtcNow;
            m_dPurchaseDate         = DateTime.UtcNow;
            m_nCollectionPurchaseID = 0;

            m_sDeviceUDID = string.Empty;
            m_sDeviceName = string.Empty;

            m_bCancelWindow = false;
        }

        public void Initialize(string sCollectionCode, DateTime dEndTime , DateTime dCurrentDate , DateTime dLastViewDate ,
            DateTime dPurchaseDate, Int32 nCollectionPurchaseID, ePaymentMethod payMethod, string sDeviceUDID, bool bCancelWindow = false)
        {
            m_nCollectionPurchaseID = nCollectionPurchaseID;
            m_sCollectionCode = sCollectionCode;
            m_dEndDate = dEndTime;
            m_dCurrentDate = dCurrentDate;
            if (dLastViewDate != new DateTime(2099 ,1 , 1))
                m_dLastViewDate = dLastViewDate;
            m_dPurchaseDate = dPurchaseDate;
            m_paymentMethod = payMethod;
            m_bCancelWindow = bCancelWindow;

            if (!string.IsNullOrEmpty(sDeviceUDID))
            {
                m_sDeviceUDID = sDeviceUDID;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select name from devices where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sDeviceUDID);
                selectQuery.SetConnectionKey("users_connection");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        m_sDeviceName = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                }
                selectQuery.Finish();
                selectQuery = null;
            }
        }
    }
}
