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

public partial class AjaxSubDateChange : System.Web.UI.Page
{
    static public string GetWSURL(string sKey)
    {
        if (ConfigurationManager.AppSettings[sKey] != null &&
            ConfigurationManager.AppSettings[sKey].ToString() != "")
            return ConfigurationManager.AppSettings[sKey].ToString();
        return "";
    }

    protected bool ChangeSubDates(string sSiteGUID, string sSubscriptionCode, Int32 nPurchaseID , 
        Int32 nDurationIndays , bool bNewRenewable , Int32 nCurrentRenewable)
    {
        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "ChangeSubscriptionDates", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;
        return p.ChangeSubscriptionDates(sWSUserName, sWSPass, sSiteGUID, sSubscriptionCode, nPurchaseID, nDurationIndays, bNewRenewable);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";

        string sSiteGUID = "";
        string sSubscriptionCode = "";
        Int32 nPurchaseID = 0;
        Int32 nChangeType = 0;
        Int32 nsub_renewable = 0;
        if (Request.Form["user_id"] != null)
            sSiteGUID = Request.Form["user_id"].ToString();
        if (Request.Form["sub_code"] != null)
            sSubscriptionCode = Request.Form["sub_code"].ToString();
        if (Request.Form["purchase_id"] != null)
            nPurchaseID = int.Parse(Request.Form["purchase_id"].ToString());
        if (Request.Form["change_type"] != null)
            nChangeType = int.Parse(Request.Form["change_type"].ToString());
        if (Request.Form["sub_renewable"] != null)
            nsub_renewable = int.Parse(Request.Form["sub_renewable"].ToString());
        bool bOK = false;
        Int32 t = -111111;
        bool bNewRenewable = false;
        //Final cancel
        if (nChangeType == 2)
        {
            if (nsub_renewable == 1)
                bNewRenewable = true;
            else
                bNewRenewable = false;
            t = 7;
        }
        if (nChangeType == 3)
        {
            if (nsub_renewable == 1)
                bNewRenewable = true;
            else
                bNewRenewable = false;
            t = 30;
        }
        bOK = ChangeSubDates(sSiteGUID, sSubscriptionCode, nPurchaseID, t, bNewRenewable, nsub_renewable);
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
