using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    [Serializable]
    public class UserPrePaidObject
    {
        public double m_nTotalAmount;
        public double m_nAmountUsed;

        public Int32 m_nPPPurchaseID;
        public Int32 m_nPPModuleID;

        public DateTime m_dEndDate;
        public DateTime m_dStartDate;

        public UserPrePaidObject()
        {
            m_nTotalAmount = 0;
            m_nAmountUsed = 0;

            m_nPPPurchaseID = 0;

            m_nPPModuleID = 0;

            m_dEndDate = new DateTime();
            m_dStartDate = new DateTime();
        }
    }

    [Serializable]
    public class UserPrePaidContainer
    {
        public double m_nTotalAmount;
        public double m_nAmountUsed;
        public string m_sUserSiteGuid;
        public string m_sCurrencyCode;

        public UserPrePaidObject[] m_oUserPPs;

        public UserPrePaidContainer()
        {
            m_nTotalAmount = 0.0;
            m_nAmountUsed = 0.0;

            m_sUserSiteGuid = string.Empty;
            m_sCurrencyCode = string.Empty;

            m_oUserPPs = new UserPrePaidObject[0];
        }

        public void Initialize(string sUserSiteGuid, string sCurrencyCode)
        {

            m_sUserSiteGuid = sUserSiteGuid;
            m_sCurrencyCode = sCurrencyCode;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from pre_paid_purchases where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", sUserSiteGuid);
            if (!string.IsNullOrEmpty(sCurrencyCode))
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("currency_code", "=", sCurrencyCode);
            }
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", ">", DateTime.Now);
            selectQuery += " and total_amount>amount_used order by end_date";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    m_oUserPPs = new UserPrePaidObject[nCount];
                }
                for (int i = 0; i < nCount; i++)
                {
                    UserPrePaidObject uppo = new UserPrePaidObject();

                    uppo.m_nPPPurchaseID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);

                    uppo.m_nPPModuleID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "pre_paid_module_id", i);

                    uppo.m_nTotalAmount = ODBCWrapper.Utils.GetDoubleSafeVal(selectQuery, "total_amount", i);
                    uppo.m_nAmountUsed = ODBCWrapper.Utils.GetDoubleSafeVal(selectQuery, "amount_used", i);

                    uppo.m_dEndDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "end_date", i);
                    uppo.m_dStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "start_date", i);

                    m_nTotalAmount += uppo.m_nTotalAmount;
                    m_nAmountUsed += uppo.m_nAmountUsed;

                    m_oUserPPs[i] = uppo;
                }
            }

            selectQuery.Finish();
            selectQuery = null;
        }

    }
}
