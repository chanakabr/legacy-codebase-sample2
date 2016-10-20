using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace CCRenewer
{
    public class Renewer : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static protected object o = new object();
        protected Int32 m_nGroupID;
        protected Int32 m_nBillingProvider;

        public Renewer(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            string[] seperator = { "||" };
            string[] splited = sParameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 2)
            {
                m_nGroupID = int.Parse(splited[0].ToString());
                m_nBillingProvider = int.Parse(splited[1].ToString());
            }
            else
            {
                m_nGroupID = 0;
                m_nBillingProvider = 0;
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
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "<", DateTime.UtcNow.AddHours(24));
            if (m_nGroupID != 0)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            }
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FAIL_COUNT", "<", GetGroupFAILCOUNT());
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
            log.Debug("Start - Job start: group: " + m_nGroupID.ToString() + " | billing provider: " + m_nBillingProvider.ToString());
            while (sList != "")
            {
                sList = GetList();
                if (sList != "")
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select q.* from (select * from billing_transactions where purchase_id in (" + sList + ") and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", m_nBillingProvider);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING", "=", 1);
                    //selectQuery += " and (payment_number < number_of_payments or number_of_payments = 0)";
                    selectQuery += ")q,(select max(id) as id,purchase_id from billing_transactions where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", m_nBillingProvider);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING", "=", 1);
                    // selectQuery += " and (payment_number < number_of_payments or number_of_payments = 0)";
                    selectQuery += " and purchase_id in (" + sList + ") group by purchase_id)q1 where q.id=q1.id";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            ConditionalAccess.BaseConditionalAccess t = null;
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
                                Int32 nPaymentNumber = int.Parse(selectQuery.Table("query").DefaultView[i].Row["PAYMENT_NUMBER"].ToString());
                                Int32 nNumOfPayments = int.Parse(selectQuery.Table("query").DefaultView[i].Row["number_of_payments"].ToString());
                                if (nNumOfPayments == 0 || nPaymentNumber <= nNumOfPayments)
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

                                    ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, nGroupID, "CA_CONNECTION_STRING");
                                    if (m_nBillingProvider == 10)
                                    {
                                        var resp = t.DD_BaseRenewSubscription(sSiteGUID, dPrice, sCurrency, sSubscriptionCode, "1.1.1.1", sExtraParams,
                                       nPurchaseID, nBillingMethod, nPaymentNumber, sCountryCd, sLanguageCode, sDeviceName);
                                        log.Debug("renew - " + sLogStr + "status code: " + resp.m_oStatus.ToString() + " | status desc: " + resp.m_sStatusDescription + " | reciept: " + resp.m_sRecieptCode);
                                    }
                                    else
                                    {
                                        var  resp = t.CC_BaseRenewSubscription(sSiteGUID, dPrice, sCurrency, sSubscriptionCode, "1.1.1.1", sExtraParams,
                                            nPurchaseID, nPaymentNumber, sCountryCd, sLanguageCode, sDeviceName);
                                        log.Debug("renew - " + sLogStr + "status code: " + resp.m_oStatus.ToString() + " | status desc: " + resp.m_sStatusDescription + " | reciept: " + resp.m_sRecieptCode);
                                    }
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
            updateQuery += " where RECURRING_RUNTIME_STATUS=1 and IS_ACTIVE=1 and STATUS=1 and IS_RECURRING_STATUS=1";
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        private int GetGroupFAILCOUNT()
        {
            int res = 10;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery += "select FAIL_COUNT from groups_parameters where IS_ACTIVE =1 and STATUS=1 ";
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                res = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "FAIL_COUNT", 0);
                if (res == 0)
                {
                    //default value; 
                    res = 10;
                }
            }
            return res;
        }
    }
}
