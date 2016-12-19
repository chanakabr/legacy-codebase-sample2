using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class PermittedSubscriptionContainer
    {
        public string m_sSubscriptionCode;
        public Int32 m_nMaxUses;
        public Int32 m_nCurrentUses;
        public DateTime m_dEndDate;
        public DateTime m_dCurrentDate;
        public DateTime m_dLastViewDate;
        public DateTime m_dPurchaseDate;
        public DateTime m_dNextRenewalDate;
        public bool m_bRecurringStatus;
        public bool m_bIsSubRenewable;
        public Int32 m_nSubscriptionPurchaseID;
        public PaymentMethod m_paymentMethod;

        public string m_sDeviceUDID;
        public string m_sDeviceName;
        public bool m_bCancelWindow;

        public bool m_bIsInGracePeriod;

        public int paymentGatewayId;
        public int paymentMethodId;


        public PermittedSubscriptionContainer()
        {
            m_sSubscriptionCode = "";
            m_nMaxUses = 0;
            m_nCurrentUses = 0;
            m_dEndDate = new DateTime(2099, 1, 1);
            m_dCurrentDate = DateTime.UtcNow;
            m_dPurchaseDate = DateTime.UtcNow;
            m_bRecurringStatus = false;
            m_nSubscriptionPurchaseID = 0;

            m_sDeviceUDID = string.Empty;
            m_sDeviceName = string.Empty;

            m_bCancelWindow = false;
            m_bIsInGracePeriod = false;

            paymentGatewayId = 0;
            paymentMethodId = 0;
        }

        public void Initialize(string sSubscriptionCode, Int32 nMaxUses, Int32 nCurrentUses,
            DateTime dEndTime, DateTime dCurrentDate, DateTime dLastViewDate, DateTime dPurchaseDate, DateTime dNextRenewalDate,
            bool bRecurringStatus, bool bIsSubRenewable, Int32 nSubscriptionPurchaseID, PaymentMethod payMethod, string sDeviceUDID, bool bCancelWindow = false, bool isInGracePeriod = false,
            int paymentGatewayId = 0, int paymentMethodId = 0)
        {
            m_nSubscriptionPurchaseID = nSubscriptionPurchaseID;
            m_sSubscriptionCode = sSubscriptionCode;
            m_nMaxUses = nMaxUses;
            m_nCurrentUses = nCurrentUses;
            m_dEndDate = dEndTime;
            m_dCurrentDate = dCurrentDate;
            if (dLastViewDate != new DateTime(2099, 1, 1))
                m_dLastViewDate = dLastViewDate;
            m_dPurchaseDate = dPurchaseDate;
            m_dNextRenewalDate = dNextRenewalDate;
            m_bRecurringStatus = bRecurringStatus;
            m_bIsSubRenewable = bIsSubRenewable;
            m_paymentMethod = payMethod;
            m_bCancelWindow = bCancelWindow;
            this.m_bIsInGracePeriod = isInGracePeriod;

            this.paymentGatewayId = paymentGatewayId;
            this.paymentMethodId = paymentMethodId;

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
