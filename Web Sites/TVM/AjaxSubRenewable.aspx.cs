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

public partial class AjaxSubRenewable : System.Web.UI.Page
{
    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    protected bool RenewCancledSubscription(string sSiteGUID, string sSubscriptionCode, Int32 nPurchaseID)
    {
        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetUserBillingHistory", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        return p.RenewCancledSubscription(sWSUserName, sWSPass, sSiteGUID, sSubscriptionCode, nPurchaseID);
    }

    protected bool CancelSubscription(string sSiteGUID, string sSubscriptionCode, Int32 nPurchaseID)
    {
        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetUserBillingHistory", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        return p.CancelSubscription(sWSUserName, sWSPass, sSiteGUID, sSubscriptionCode, nPurchaseID);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";

        string sSiteGUID = "";
        string sSubscriptionCode = "";
        Int32 nPurchaseID = 0;
        Int32 nStatus = -1;
        if (Request.Form["user_id"] != null)
            sSiteGUID = Request.Form["user_id"].ToString();
        if (Request.Form["sub_code"] != null)
            sSubscriptionCode = Request.Form["sub_code"].ToString();
        if (Request.Form["purchase_id"] != null)
            nPurchaseID = int.Parse(Request.Form["purchase_id"].ToString());
        if (Request.Form["status"] != null)
            nStatus = int.Parse(Request.Form["status"].ToString());
        bool bOK = false;
        if (nStatus == 0)
            bOK = CancelSubscription(sSiteGUID, sSubscriptionCode, nPurchaseID);
        if (nStatus == 1)
            bOK = RenewCancledSubscription(sSiteGUID, sSubscriptionCode, nPurchaseID);
        if (bOK == true)
            sRet = "OK";
        else
            sRet = "FAIL";
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~");
    }
}
