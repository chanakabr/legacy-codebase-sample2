using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using DAL;
using KLogMonitor;
using M1BL;
using ApiObjects;
using Core.Users;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class M1CellularCreditCard : BaseCellularCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public M1CellularCreditCard(int groupID)
            : base(groupID)
        {
        }

        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters)
        {
            BillingResponse ret = new BillingResponse();

            try
            {
                log.Info("M1CellularCreditCard ChargeUser start at " + DateTime.Now.ToString());

                string sChargedMobileNumber = string.Empty;
                string sCustomerServiceID = string.Empty;

                DataTable dtUserLastBillingTransactions = ApiDAL.Get_LastBillingTransactionToUser(m_nGroupID, sSiteGUID, (int)eBillingProvider.M1);

                if (dtUserLastBillingTransactions != null && dtUserLastBillingTransactions.Rows.Count > 0)
                {
                    int nPaymentMethod = ODBCWrapper.Utils.GetIntSafeVal(dtUserLastBillingTransactions.Rows[0]["billing_method"]);
                    sChargedMobileNumber = ODBCWrapper.Utils.GetSafeStr(dtUserLastBillingTransactions.Rows[0]["cell_phone"]);
                    sCustomerServiceID = ODBCWrapper.Utils.GetSafeStr(dtUserLastBillingTransactions.Rows[0]["extra_params"]);
                    log.Info(string.Format("User billing details : GroupID={0}, SiteGuid={1}, ChargedMobileNumber{2}, CustomerServiceID{3}", m_nGroupID, sSiteGUID, sChargedMobileNumber, sCustomerServiceID));
                }
                else
                {
                    ret.m_oStatus = BillingResponseStatus.Fail;
                    ret.m_sStatusDescription = "No latest transaction found for M1 user:" + sSiteGUID;
                    return ret;
                }
                ret = ChargeM1(sSiteGUID, sChargedMobileNumber, sCustomerServiceID, dChargePrice, sCurrencyCode, sCustomData, nPaymentNumber, nNumberOfPayments);
            }
            catch (Exception ex)
            {
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = "Failed to charge M1 user:" + sSiteGUID;
                log.Error(string.Format("Exception on charge M1 credit card for user {0}, ex.Message {1}", sSiteGUID, ex.Message));
            }
            return ret;
        }

        private BillingResponse ChargeM1(string sSiteGUID, string sChargedMobileNumber, string sCustomerServiceID, double dChargePrice, string sCurrencyCode, string sCustomData, int nPaymentNumber, int nNumberOfPayments)
        {
            BillingResponse ret = new BillingResponse();
            ret.m_sExternalReceiptCode = "M1";

            string sFixedMailAddress = string.Empty;
            int nM1TransactionID = 1;
            int nBillingProvider = (int)eBillingProvider.M1;
            int nBillingMethod = 60;
            int nBillingProcessor = 30;
            int nBillingStatus = 0; //inserted new value to tvinci.dbo.billing_transactions should be 0   
            int nPurchaseType = 0;
            string sItemName = string.Empty;
            long lBillingTransactionID = 0;
            string sPaymentMethod = ePaymentMethod.M1.ToString().ToLower();

            if (!CheckUserData(sSiteGUID))
            {
                ret.m_oStatus = BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Unknown or active user";
                return ret;
            }

            int customDataID = Utils.AddCustomData(sCustomData);
            GetPurchaseTypeAndItemName(sCustomData, out nPurchaseType, out sItemName);

            M1Response m1Response = M1Logic.CheckSubsequencePurchasePermissions(m_nGroupID, sChargedMobileNumber, out sFixedMailAddress);


            if (!m1Response.is_succeeded)
            {
                nBillingStatus = 1;
                lBillingTransactionID = Core.Billing.Utils.InsertNewM1Transaction(
                    m_nGroupID, sSiteGUID, nPurchaseType, sChargedMobileNumber, sCustomerServiceID, dChargePrice, customDataID, (int)M1TransactionStatus.Fail, string.Empty,
                    sCurrencyCode, sCustomData, nBillingStatus, string.Empty, string.Empty, string.Empty, string.Empty, nPaymentNumber, nNumberOfPayments, nBillingProcessor,
                    nBillingMethod, nBillingProvider, ref nM1TransactionID);

                Core.Billing.Utils.SendMailToFixedAddress(sPaymentMethod, sItemName, sSiteGUID, lBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sChargedMobileNumber, m_nGroupID, sFixedMailAddress, eMailTemplateType.PaymentFail);

                ret.m_oStatus = BillingResponseStatus.CellularPermissionsError;
                ret.m_sRecieptCode = lBillingTransactionID.ToString();
                ret.m_sStatusDescription = m1Response.reason + "," + m1Response.description;
                return ret;
            }

            lBillingTransactionID = Core.Billing.Utils.InsertNewM1Transaction(m_nGroupID, sSiteGUID, nPurchaseType, sChargedMobileNumber, sCustomerServiceID, dChargePrice, customDataID, (int)M1TransactionStatus.Pending, string.Empty,
                                                                        sCurrencyCode, sCustomData, nBillingStatus, string.Empty, string.Empty, string.Empty, string.Empty, nPaymentNumber, nNumberOfPayments, nBillingProcessor,
                                                                        nBillingMethod, nBillingProvider, ref nM1TransactionID);

            ret.m_oStatus = BillingResponseStatus.Success;
            ret.m_sRecieptCode = lBillingTransactionID.ToString();

            Core.Billing.Utils.SendMailToFixedAddress(sPaymentMethod, sItemName, sSiteGUID, lBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sChargedMobileNumber, m_nGroupID, sFixedMailAddress, eMailTemplateType.Purchase);

            log.Info(string.Format("Charge M1 , for user {0} mobile number {1}", sSiteGUID, sChargedMobileNumber));
            return ret;
        }


        private bool CheckUserData(string sSiteGUID)
        {
            UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);

            if (uObj.m_RespStatus != ResponseStatus.OK)
            {
                return false;
            }
            return true;
        }

        private void GetPurchaseTypeAndItemName(string sCustomData, out int nPurchaseType, out string sItemName)
        {
            nPurchaseType = 1;
            sItemName = string.Empty;

            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(sCustomData);
                System.Xml.XmlNode theRequest = doc.FirstChild;


                string sType = GetCustomDataParamSafeValue(".", "type", theRequest);

                sItemName = string.Empty;

                if (sType.Equals("pp"))
                {
                    string sMediaID = TVinciShared.XmlUtils.GetSafeValue("m", ref theRequest);
                    if (!string.IsNullOrEmpty(sMediaID))
                    {
                        int nMediaID = int.Parse(sMediaID);
                        sItemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "MAIN_CONNECTION_STRING").ToString();
                    }
                }
                else if (sType == "sp")
                {
                    nPurchaseType = 2;
                    string sSubscriptionID = TVinciShared.XmlUtils.GetSafeValue("s", ref theRequest);
                    if (!string.IsNullOrEmpty(sSubscriptionID))
                    {
                        int nSubID = int.Parse(sSubscriptionID);
                        sItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nSubID, "pricing_connection").ToString();
                    }
                }
                else if (sType.Equals("prepaid"))
                {
                    nPurchaseType = 3;
                    string sPrePaidCode = TVinciShared.XmlUtils.GetSafeValue("pp", ref theRequest);
                    if (!string.IsNullOrEmpty(sPrePaidCode))
                    {
                        int nPrePaidCode = int.Parse(sPrePaidCode);
                        sItemName = ODBCWrapper.Utils.GetTableSingleVal("pre_paid_modules", "name", nPrePaidCode, "pricing_connection").ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error (GetPurchaseTypeAndItemName)" + ex.Message, ex);
            }
        }

        protected string GetCustomDataParamSafeValue(string sQueryKey, string sParName, System.Xml.XmlNode theRoot)
        {
            try
            {
                if (theRoot.SelectSingleNode(sQueryKey) != null && theRoot.SelectSingleNode(sQueryKey).Attributes[sParName] != null)
                {
                    return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }




        public override bool UpdatePurchaseIDInBillingTable(long purchaseID, long billingRefTransactionID)
        {
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("m1_transactions");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", purchaseID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", billingRefTransactionID);
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }

            return true;
        }
    }
}
