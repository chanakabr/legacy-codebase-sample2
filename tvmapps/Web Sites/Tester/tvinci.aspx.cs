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

public partial class tvinci : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ca.module caModule = new ca.module();

        //tik_bill.Service s = new tik_bill.Service();
        //tik_bill.Response resp = s.NotifyCustomer("282");
        //s.NotifyProduct
        //string s = DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss");
        //Response.Write(s);
        //ca.module m = new ca.module();
        //string[] subs = {"1"};
        //ca.SubscriptionsPricesContainer[] cont = m.GetSubscriptionsPrices("conditionalaccess", "11111", subs, "30");
        CCRenewer.Renewer r = new CCRenewer.Renewer(0, 0, "93||1");
        r.DoTheJob();
        /*
        tv_api.API t = new tv_api.API();
        t.Url = "http://localhost:1120/api.asmx";
        string sC = "";
        string sL = "";
        string sD = "";
        tv_api.UserStatus us = new tv_api.UserStatus();
        t.GetAdminTokenValues("api_96", "11111", "80.179.194.132", "28a00047-f41a-4ab9-9958-cf6d81c0f167", ref sC, ref sL, ref sD, ref us);
        */
    }
}
