using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class AjaxManipGift : System.Web.UI.Page
{
    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    protected void AddUserLog(string sSiteGUID, string sRemarks)
    {
        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        users_ws.UsersService p = new users_ws.UsersService();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "AddToLog", "users", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("users_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        /*
        bool bOK = p.CC_DummyChargeUserForSubscription(sWSUserName, sWSPass, sSiteGUID, 0, "USD", sSubscriptionCode, "", "1.1.1.1", "", "", "", "");
        return bOK;
        */
    }

    protected ca_ws.BillingResponseStatus GiveFreeSubOld(string sSiteGUID, string sGiftCode, string sRemarks, ref string sError)
    {
        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CC_DummyChargeUserForSubscription", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        ca_ws.BillingResponse ret = p.CC_DummyChargeUserForSubscription(sWSUserName, sWSPass, sSiteGUID, 0, "USD", sGiftCode, "", "1.1.1.1", "", "", "", "");
        AddUserLog(sSiteGUID, sRemarks);
        sError = ret.m_sStatusDescription;
        return ret.m_oStatus;
    }

    protected ca_ws.BillingResponseStatus GiveFreeSub(string sSiteGUID, string sGiftCode, string sRemarks, ref string sError)
    {
        ca_ws.BillingResponseStatus ret = ca_ws.BillingResponseStatus.Fail;

        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CC_DummyChargeUserForSubscription", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;

        var res = p.GrantEntitlements(sWSUserName, sWSPass, sSiteGUID, 0, 0, int.Parse(sGiftCode), ca_ws.eTransactionType.Subscription, "1.1.1.1", string.Empty, true);
        sError = res.Message;
        ret = res.Code == 0 ? ca_ws.BillingResponseStatus.Success : ca_ws.BillingResponseStatus.Fail;

        return ret;
    }

    protected ca_ws.BillingResponseStatus GiveFreePPVOld(string sSiteGUID, string sGiftCode, string sRemarks, ref string sError)
    {
        ca_ws.BillingResponseStatus ret = ca_ws.BillingResponseStatus.Fail;

        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CC_DummyChargeUserForSubscription", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        int mediaFileId = 0;
        if (int.TryParse(sGiftCode, out mediaFileId))
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection_string");
            selectQuery += " select ppv_module_id from ppv_modules_media_files where is_active = 1 and status = 1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", mediaFileId);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    string ppvModuleID = selectQuery.Table("query").DefaultView[0].Row["ppv_module_id"].ToString();
                    ca_ws.BillingResponse response = p.CC_DummyChargeUserForMediaFile(sWSUserName, sWSPass, sSiteGUID, 0, "USD", int.Parse(sGiftCode), ppvModuleID, "", "1.1.1.1", "", "", "", "");
                    sError = response.m_sStatusDescription;
                    ret = response.m_oStatus;
                    AddUserLog(sSiteGUID, sRemarks);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }
        return ret;
    }

    protected ca_ws.BillingResponseStatus GiveFreePPV(string sSiteGUID, string sGiftCode, string sRemarks, ref string sError)
    {
        ca_ws.BillingResponseStatus ret = ca_ws.BillingResponseStatus.Fail;

        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CC_DummyChargeUserForSubscription", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        int mediaFileId = 0;
        if (int.TryParse(sGiftCode, out mediaFileId))
        {
            int productId = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection_string");
            selectQuery += " select ppv_module_id from ppv_modules_media_files where is_active = 1 and status = 1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", mediaFileId);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    productId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ppv_module_id", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (productId > 0)
            {
                var res = p.GrantEntitlements(sWSUserName, sWSPass, sSiteGUID, 0, mediaFileId, productId, ca_ws.eTransactionType.PPV, "1.1.1.1", string.Empty, true);
                sError = res.Message;
                ret = res.Code == 0 ? ca_ws.BillingResponseStatus.Success : ca_ws.BillingResponseStatus.Fail;
            }

        }
        return ret;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";

        string sSiteGUID = "";
        string sType = "";
        string sGiftCode = "";
        string sRemarks = "";
        if (Request.Form["user_id"] != null)
            sSiteGUID = Request.Form["user_id"].ToString();
        if (Request.Form["type"] != null)
            sType = Request.Form["type"].ToString();
        if (Request.Form["gift_code"] != null)
            sGiftCode = Request.Form["gift_code"].ToString();
        if (Request.Form["remarks"] != null)
            sRemarks = Request.Form["remarks"].ToString();
        bool bOK = false;
        string sError = "";
        if (sType == "sub")
        {
            ca_ws.BillingResponseStatus ret = GiveFreeSub(sSiteGUID, sGiftCode, sRemarks, ref sError);
            if (ret == ca_ws.BillingResponseStatus.Success)
                bOK = true;
        }
        if (sType == "ppv")
        {
            ca_ws.BillingResponseStatus ret =  GiveFreePPV(sSiteGUID, sGiftCode, sRemarks, ref sError);
            if (ret == ca_ws.BillingResponseStatus.Success)
                bOK = true;
        }
        if (bOK == true)
            sRet = "OK";
        else
            sRet = "FAIL";
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~" + sError);
    }
}
