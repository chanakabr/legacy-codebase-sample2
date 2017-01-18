using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using System.Data;
using ApiObjects.Billing;

namespace Core.Billing
{
    public abstract class BaseCreditCard
    {
        #region Data Members

        protected Int32 m_nGroupID;
        protected bool m_isMultiUserCC;
        protected string m_sEncryptedCVV;
        protected string m_sPaymentMethodID;

        #endregion

        #region Ctors

        protected BaseCreditCard()
        {
        }

        protected BaseCreditCard(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
            m_isMultiUserCC = IsUserMultiCC();
            m_sPaymentMethodID = string.Empty;
            m_sEncryptedCVV = string.Empty;
        }

        protected BaseCreditCard(int nGroupID, string sPaymentMethodID, string sEncryptedCVV)
        {
            m_nGroupID = nGroupID;
            m_isMultiUserCC = IsUserMultiCC();
            m_sPaymentMethodID = sPaymentMethodID;
            m_sEncryptedCVV = sEncryptedCVV;
        }

        #endregion

        #region Abstract Methods

        public abstract BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters);
        public abstract string GetClientCheckSum(string sUserIP, string sRandom);
        public abstract string GetClientMerchantSig(string sParams);
        public abstract bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID);

        #endregion

        #region Virtual Methods

        public virtual void DeleteUserCreditCardDigits(string sSiteGUID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_tokens");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("cc_saved", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            updateQuery += " and group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        public virtual string GetUserCreditCardDigits(string sSiteGUID)
        {
            bool bSaved = false;

            string sRet = GetUserDigits(sSiteGUID, ref bSaved);
            if (bSaved == true)
                return sRet;
            return "";
        }

        #endregion

        #region Private and protected methods

        protected string GetUserToken(string sSiteGUID, ref Int32 nExpM, ref Int32 nExpY,
            ref string sStartM, ref string sStartY, ref string sIssueNum, ref string sIssuerBank, ref int transactionID)
        {
            string sToken = "";
            DataTable dt = BillingDAL.Get_UserToken(m_nGroupID, sSiteGUID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    sToken = Core.Billing.Utils.GetStrSafeVal(dt.Rows[0], "token");
                    nExpM = Core.Billing.Utils.GetIntSafeVal(dt.Rows[0], "EXP_MONTH");//-1 if can't do int.Parse
                    nExpY = Core.Billing.Utils.GetIntSafeVal(dt.Rows[0], "EXP_YEAR");//-1 if can't do int.Parse
                    sIssueNum = Core.Billing.Utils.GetStrSafeVal(dt.Rows[0], "DC_ISSUE");
                    sStartM = Core.Billing.Utils.GetStrSafeVal(dt.Rows[0], "DC_START_MONTH");
                    sStartY = Core.Billing.Utils.GetStrSafeVal(dt.Rows[0], "DC_START_YEAR");
                    sIssuerBank = Core.Billing.Utils.GetStrSafeVal(dt.Rows[0], "DC_ISSUING_BANK");
                    transactionID = Core.Billing.Utils.GetIntSafeVal(dt.Rows[0], "TRANSACTION_ID"); //-1 if can't do int.Parse
                }
            }
            return sToken;
        }

        private bool IsUserMultiCC()
        {
            bool retVal = false;
            DataTable dt = BillingDAL.IsUserMultiCC(m_nGroupID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    string multiStr = Utils.GetStrSafeVal(dt.Rows[0], "MULTI_USER_CC");
                    if (multiStr.Equals("1"))
                    {
                        retVal = true;
                    }
                }
            }
            return retVal;
        }

        protected string GetUserDigits(string sSiteGUID, ref bool bSaved)
        {
            string sRet = "";
            bSaved = false;


            DataTable dt = BillingDAL.Get_UserDigits(m_nGroupID, sSiteGUID);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                sRet = Utils.GetStrSafeVal(dt.Rows[0], "LAST_FOUR_DIGITS");
                Int32 nIsActive = Utils.GetIntSafeVal(dt.Rows[0], "cc_saved");
                if (nIsActive == 1)
                    bSaved = true;
            }
            return sRet;
        } 

        #endregion
    }
}

