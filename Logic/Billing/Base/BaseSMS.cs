using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public abstract class BaseSMS
    {
        protected BaseSMS() { }

        protected BaseSMS(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract BillingResponse SendCode(string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters);
        public abstract BillingResponse CheckCode(string sSiteGUID, string sCellPhone, string sCode, string sReferenceCode);

        protected bool UpdateSMSCodeStatus(Int32 nID, Int32 nStatus)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("sms_codes");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nStatus);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
            return true;
        }

        protected Int32 CheckSMSCode(ref string sSubCode, ref Int32 nMediaFileID, string sSiteGUID,
            string sToken, string sCellPhone, ref string sPPVModuleCode, string sPriceCode, double dPrice,
            string sCurrencyCd , bool bUpdateStatus , ref Int32 nActivationState , ref string sCustomData)
        {
            Int32 nID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from sms_codes where is_active in (0,2) and status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (sSubCode != "")
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCode);
            }
            if (nMediaFileID != 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            }
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SMS_CODE", "=", sToken);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCellPhone);
            if (sPPVModuleCode != "")
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVModuleCode);
            }
            if (sPriceCode != "")
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE_CODE", "=", sPriceCode);
            }
            if (dPrice != 0.0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            }
            if (sCurrencyCd != "")
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrencyCd);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nActivationState = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    sCustomData = selectQuery.Table("query").DefaultView[0].Row["CUSTOM_DATA"].ToString();        
                    if (selectQuery.Table("query").DefaultView[0].Row["SUBSCRIPTION_CODE"] != null &&
                        selectQuery.Table("query").DefaultView[0].Row["SUBSCRIPTION_CODE"] != DBNull.Value)
                        sSubCode = selectQuery.Table("query").DefaultView[0].Row["SUBSCRIPTION_CODE"].ToString();
                    if (selectQuery.Table("query").DefaultView[0].Row["PPVMODULE_CODE"] != null &&
                        selectQuery.Table("query").DefaultView[0].Row["PPVMODULE_CODE"] != DBNull.Value)
                        sPPVModuleCode = selectQuery.Table("query").DefaultView[0].Row["PPVMODULE_CODE"].ToString();
                    nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MEDIA_FILE_ID"].ToString());
                    if (bUpdateStatus == true && nActivationState == 2)
                        UpdateSMSCodeStatus(nID, 1);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        protected bool InsertSMSCode(string sSubCode , Int32 nMediaFileID , string sSiteGUID , 
            string sToken , string sCellPhone , string sPPVModuleCode , string sPriceCode , double dPrice , 
            string sCurrencyCd , string sCustomData , Int32 nProvider)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("sms_codes");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            if (sSubCode != "")
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SMS_CODE", "=", sToken);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCellPhone);
            if (sPPVModuleCode != "")
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVModuleCode);


            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE_CODE", "=", sPriceCode);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrencyCd);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOM_DATA", "=", sCustomData);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SMS_BILLING_PROVIDER", "=", nProvider);

            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            return true;
        }

        protected bool IsPendingSMSPurchase(Int32 nMediaFileID, string sSiteGUID, string sCellNum, double dPrice, string sCurrencyCd, string sSubscriptionCode, string sPPVMODULE_CODE, ref string sToken , ref Int32 nActivationState , ref Int32 nID)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from sms_codes where is_active in (0,2) and status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (nMediaFileID != 0)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
            }
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCellNum);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrencyCd);
            if (sSubscriptionCode != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
            }
            if (sPPVMODULE_CODE != "")
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVMODULE_CODE);
            }

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sToken = selectQuery.Table("query").DefaultView[0].Row["SMS_CODE"].ToString();
                    nActivationState = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
                    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    bRet = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }

        protected Int32 m_nGroupID;
    }
}
