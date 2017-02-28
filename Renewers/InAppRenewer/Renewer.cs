using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Core.ConditionalAccess;
using ApiObjects.Billing;

namespace InAppRenewer
{
    public class Renewer : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static protected object o = new object();
        protected Int32 m_nGroupID;
        protected int m_nBillingProvider = 200;
        protected int m_nMinutes = -120;

        public Renewer(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            if (!string.IsNullOrEmpty(sParameters))
            {
                string[] seperator = { "||" };
                string[] splited = sParameters.Split(seperator, StringSplitOptions.None);

                if (splited.Length > 0)
                {
                    m_nGroupID = int.Parse(splited[0]);

                    if (splited.Length > 1)
                    {
                        m_nMinutes = int.Parse(splited[1]);
                    }
                }
                else
                {
                    m_nGroupID = int.Parse(sParameters);
                }
            }
            else
            {
                m_nGroupID = 0;
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new Renewer(nTaskID, nIntervalInSec, sParameters);
        }

        protected override bool DoTheTaskInner()
        {
            return DoTheJob();
        }

        protected string GetListTemp()
        {
            string sList = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += "select top 10 * from subscriptions_purchases where IS_ACTIVE=1 and STATUS=1 and IS_RECURRING_STATUS=1 and RECURRING_RUNTIME_STATUS=0 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", 128953);
            if (m_nGroupID != 0)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            }
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FAIL_COUNT", "<", 10);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    if (sList != "")
                        sList += ",";
                    sList += selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sList != "")
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RECURRING_RUNTIME_STATUS", "=", 1);
                updateQuery += " where id in (" + sList + ")";
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            return sList;
        }

        protected string GetList()
        {
            string sList = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += "select top 10 * from subscriptions_purchases where IS_ACTIVE=1 and STATUS=1 and IS_RECURRING_STATUS=1 and RECURRING_RUNTIME_STATUS=0 and ";
            selectQuery += string.Format("DATEADD(minute,{0},END_DATE) < GETDATE()", m_nMinutes);
            if (m_nGroupID != 0)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            }
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FAIL_COUNT", "<", 10);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    if (sList != "")
                        sList += ",";
                    sList += selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (sList != "")
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RECURRING_RUNTIME_STATUS", "=", 1);
                updateQuery += " where id in (" + sList + ")";
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            return sList;
        }

        public bool DoTheJob()
        {
            string sList = " ";
            log.DebugFormat("Start - Job start: group: {0} billing provider: {1}", m_nGroupID.ToString(), m_nBillingProvider.ToString());

            while (sList != "")
            {
                sList = GetList();
                if (sList != "")
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select q.* from (select * from billing_transactions where purchase_id in (" + sList + ") and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", m_nBillingProvider);
                    //selectQuery += " and (payment_number < number_of_payments or number_of_payments = 0)";
                    selectQuery += ")q,(select max(id) as id,purchase_id from billing_transactions where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", m_nBillingProvider);
                    // selectQuery += " and (payment_number < number_of_payments or number_of_payments = 0)";
                    selectQuery += " and purchase_id in (" + sList + ") group by purchase_id)q1 where q.id=q1.id";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            BaseConditionalAccess t = null;
                            Int32 nGroupID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["group_id"].ToString());
                            int nBillingMethod = 0;
                            object oBillingMethod = selectQuery.Table("query").DefaultView[i].Row["BILLING_METHOD"];
                            if (oBillingMethod != null && oBillingMethod != System.DBNull.Value)
                            {
                                nBillingMethod = int.Parse(oBillingMethod.ToString());
                            }
                            string sSiteGUID = selectQuery.Table("query").DefaultView[i].Row["SITE_GUID"].ToString();
                            double dPrice = double.Parse(selectQuery.Table("query").DefaultView[i].Row["PRICE"].ToString());
                            string sCurrency = selectQuery.Table("query").DefaultView[i].Row["CURRENCY_CODE"].ToString();
                            string sSubscriptionCode = selectQuery.Table("query").DefaultView[i].Row["SUBSCRIPTION_CODE"].ToString();
                            if (!string.IsNullOrEmpty(sSubscriptionCode))
                            {
                                string sExtraParams = selectQuery.Table("query").DefaultView[i].Row["EXTRA_PARAMS"].ToString();
                                Int32 nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["purchase_id"].ToString());

                                int nInAppTransactionID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "BILLING_PROVIDER_REFFERENCE", i);

                                Int32 nPaymentNumber = int.Parse(selectQuery.Table("query").DefaultView[i].Row["PAYMENT_NUMBER"].ToString());
                                Int32 nNumOfPayments = int.Parse(selectQuery.Table("query").DefaultView[i].Row["number_of_payments"].ToString());
                                if (nInAppTransactionID > 0 && (nNumOfPayments == 0 || nPaymentNumber <= nNumOfPayments))
                                {
                                    nPaymentNumber++;
                                    string sLogStr = "SiteGUID: " + sSiteGUID + " | price: " + dPrice.ToString() + sCurrency + " | Subscription Code: " + sSubscriptionCode
                                        + " | Extra params: " + sExtraParams + " | Prchase ID: " + nPurchaseID.ToString() + " | Payment number : " + nPaymentNumber.ToString() + " | ";

                                    object oCountryCd = selectQuery.Table("query").DefaultView[i].Row["COUNTRY_CODE"];
                                    string sCountryCd = String.Empty;
                                    if (oCountryCd != null && oCountryCd != DBNull.Value)
                                        sCountryCd = oCountryCd.ToString();

                                    object oDeviceName = selectQuery.Table("query").DefaultView[i].Row["DEVICE_NAME"];
                                    string sDeviceName = String.Empty;
                                    if (oDeviceName != null && oDeviceName != DBNull.Value)
                                        sDeviceName = oDeviceName.ToString();

                                    object oLanguageCode = selectQuery.Table("query").DefaultView[i].Row["LANGUAGE_CODE"];
                                    string sLanguageCode = String.Empty;
                                    if (oLanguageCode != null && oLanguageCode != DBNull.Value)
                                        sLanguageCode = oLanguageCode.ToString();

                                    Utils.GetBaseConditionalAccessImpl(ref t, nGroupID, "CA_CONNECTION_STRING");

                                    InAppBillingResponse resp = t.InApp_RenewSubscription(sSiteGUID, dPrice, sCurrency, sSubscriptionCode, nPurchaseID, 200,
                                        nPaymentNumber, sCountryCd, sLanguageCode, sDeviceName, nInAppTransactionID);

                                    System.Threading.Thread.Sleep(10);
                                }
                            }
                        }
                    }

                    selectQuery.Finish();
                    selectQuery = null;
                }
            }
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
            updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RECURRING_RUNTIME_STATUS", "=", 0);
            updateQuery += " where RECURRING_RUNTIME_STATUS=1 and IS_ACTIVE=1 and STATUS=1 and IS_RECURRING_STATUS=1 and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }
    }
}

