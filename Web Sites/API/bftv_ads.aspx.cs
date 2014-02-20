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

public partial class bftv_ads : System.Web.UI.Page
{
    protected string GetSafeQueryString(string sKey)
    {
        try
        {
            return Request.QueryString[sKey].ToString();
        }
        catch
        {
            return "0";
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "";
        string sChannelID = GetSafeQueryString("channel_id");
        string sMediaID = GetSafeQueryString("media_id");
        string sType = GetSafeQueryString("t");
        DateTime t1 = DateTime.Now;
        Logger.Logger.Log("message", "BFTV Ads Started:" + " Channel:" + sChannelID + " Media:" + sMediaID + " Ticks:" + DateTime.Now.Ticks.ToString(), "BFTV_FEEDER");
        if (CachingManager.CachingManager.Exist("bftv_ads_" + sChannelID + "_" + sMediaID + "_" + sType) == true)
            sRet = CachingManager.CachingManager.GetCachedData("bftv_ads_" + sChannelID + "_" + sMediaID + "_" + sType).ToString();
        else
        {
            sRet = BFTVFeeder.feeder.GetAdLink(sType, int.Parse(sMediaID), int.Parse(sChannelID), "", "");
            CachingManager.CachingManager.SetCachedData("bftv_ads_" + sChannelID + "_" + sMediaID + "_" + sType, sRet, 86400, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
        }

        DateTime t2 = DateTime.Now;
        Logger.Logger.Log("message", "BFTV Ads Finished:" + t2.Subtract(t1).Milliseconds.ToString(), "BFTV_FEEDER");
        Response.Clear();
        Response.Expires = -1;
        Response.ContentType = "text/xml";
        Response.Write(sRet.ToString());
    }
}
