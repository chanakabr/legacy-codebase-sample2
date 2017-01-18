using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;

namespace Core.Billing
{
    public class TvinciPopup: BasePopup
    {
        public TvinciPopup(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        protected void GetSCParameters(ref string sMerchantID, ref string sMerchantSiteID, ref string sPopupSecret)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from sc_group_parameters where is_active=1 and status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMerchantID = selectQuery.Table("query").DefaultView[0].Row["MERCHANT_ID"].ToString();
                    sMerchantSiteID = selectQuery.Table("query").DefaultView[0].Row["MERCHANT_SITE_ID"].ToString();
                    sPopupSecret = selectQuery.Table("query").DefaultView[0].Row["POPUP_SECRET"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        public override string GetPopupMethodURL(double dChargePrice, string sCurrencyCode, string sItemName,
            string sCustomData , string sPaymentMethod ,string sExtranParameters)
        {
            string sMerchantID = "";
            string sMerchantSiteID = "";
            string sPopupSecret = "";
            GetSCParameters(ref sMerchantID, ref sMerchantSiteID, ref sPopupSecret);
            string sURL = "https://secure.gate2shop.com/ppp/purchase.do?merchant_id=" + sMerchantID;
            sURL += "&merchant_site_id=" + sMerchantSiteID;
            sURL += "&currency=" + sCurrencyCode;
            sURL += "&total_amount=" + dChargePrice.ToString();
            sURL += "&item_name_1=" + sItemName;
            sURL += "&item_amount_1=" + dChargePrice.ToString();
            sURL += "&item_quantity_1=1";
            sURL += "&checksum=";

            string sTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd.HH:mm:ss");

            string sToMD5 = sPopupSecret + sMerchantID + sCurrencyCode + dChargePrice.ToString() + sItemName + dChargePrice.ToString() + "1" + sTimeStamp;
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            md5Provider = new MD5CryptoServiceProvider();
            byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToMD5);
            byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
            sURL += BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();

            sURL += "&time_stamp=" + sTimeStamp;
            sURL += "&version=3.0.0&skip_billing_tab=true&payment_method=" + sPaymentMethod;
            sURL += "&customData=";
            Int32 nCustomDataID = Utils.AddCustomData(sCustomData);
            string sCustomDataToSend = nCustomDataID.ToString();
            sURL += HttpUtility.UrlEncode(sCustomDataToSend);
            sURL += "&customField2=" + sPaymentMethod;
            sURL += "&customField1=";
            originalBytes = UTF8Encoding.Default.GetBytes(sCustomDataToSend);
            encodedBytes = md5Provider.ComputeHash(originalBytes);
            sURL += BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
            if (sExtranParameters != "")
            {
                if (sExtranParameters.StartsWith("&") == true)
                    sURL += sExtranParameters;
                else
                    sURL += "&" + sExtranParameters;
            }
            return sURL;
        }
    }
}
