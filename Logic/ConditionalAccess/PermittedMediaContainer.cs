using ApiObjects.Billing;
using Core.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    [Serializable]
    public class PermittedMediaContainer
    {
        public long PurchaseID { get; set; }
        public Int32 m_nMediaID;
        public Int32 m_nMediaFileID;
        public Int32 m_nMaxUses;
        public Int32 m_nCurrentUses;
        public DateTime m_dEndDate;
        public DateTime m_dCurrentDate;
        public DateTime m_dPurchaseDate;
        public DateTime m_dLastViewDate;
        public ePaymentMethod m_purchaseMethod;
        public string m_sDeviceUDID;
        public string m_sDeviceName;
        public bool m_bCancelWindow;
        public string PPVCode { get; set; }

        public PermittedMediaContainer()
        {
            m_nMediaFileID = 0;
            m_nMediaID = 0;
            m_nMaxUses = 0;
            m_nCurrentUses = 0;
            m_dEndDate = new DateTime(2099, 1, 1);
            m_dCurrentDate = DateTime.UtcNow;
            m_dPurchaseDate = DateTime.UtcNow;

            m_sDeviceUDID = string.Empty;
            m_sDeviceName = string.Empty;

            m_bCancelWindow = false;
        }

        public void Initialize(Int32 nMediaID, Int32 nMediaFileID, Int32 nMaxUses, Int32 nCurrentUses,
            DateTime dEndTime, DateTime dCurrentDate, DateTime dLastViewDate, DateTime dPurchaseDate, ePaymentMethod payMethod, string sDevicUDID, string ppvCode, long purchaseID, bool bCancelWindow = false)
        {
            m_nMediaID = nMediaID;
            m_nMediaFileID = nMediaFileID;
            m_nMaxUses = nMaxUses;
            m_nCurrentUses = nCurrentUses;
            m_dEndDate = dEndTime;
            m_dCurrentDate = dCurrentDate;
            m_dPurchaseDate = dPurchaseDate;
            m_purchaseMethod = payMethod;
            m_bCancelWindow = bCancelWindow;
            m_dLastViewDate = dLastViewDate;
            PPVCode = ppvCode;
            PurchaseID = purchaseID;

            if (!string.IsNullOrEmpty(sDevicUDID))
            {
                m_sDeviceUDID = sDevicUDID;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select name from devices where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sDevicUDID);
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

