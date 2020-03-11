using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class admin_player_template : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else
        {
            if (Request.Form["autoplay"] != null)
                Session["autoplay"] = Request.Form["autoplay"];
            else
                Session["autoplay"] = null;

            if (Request.Form["fv"] != null)
                Session["fv"] = Request.Form["fv"];
            else
                Session["fv"] = null;

            if (Request.Form["ft"] != null)
                Session["ft"] = Request.Form["ft"];
            else
                Session["ft"] = null;
        }

        string sRet = "";
        //string sRet = "<div id=\"WMPDiv\" style=\"position:absolute;z-index:0;top:0;left:0\">";
        //sRet += "<object id=\"WMPObj\" style=\"DISPLAY: none\" height=\"0\" width=\"0\" classid=\"CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6\">";
        //sRet += "<PARAM value=\"application/x-mplayer2\" name=\"TYPE\" />";
        //sRet += "<PARAM value=\"http://www.microsoft.com/Windows/MediaPlayer/download/default.asp\" name=\"PLUGINSPACE\" />";
        //sRet += "<PARAM value=\"false\" name=\"Autostart\" />";
        //sRet += "<PARAM value=\"none\" name=\"uiMode\" />";
        //sRet += "<PARAM value=\"flase\" name=\"windowlessVideo\" />";
        //sRet += "<PARAM value=\"true\" name=\"stretchToFit\" />";
        //sRet += "<PARAM value=\"0\" name=\"ShowControls\" />";
        //sRet += "<PARAM value=\"\" name=\"src\" />";
        //sRet += "</object>";
        //sRet += "</div>";
        //sRet = "";
        //sRet += "<div id=\"flashDiv\" style=\"position:absolute;z-index:1;top:0;left:0;width:100%;height:100%;\"></div>";
        sRet = GetNDSIframe();

        Response.ClearHeaders();
        Response.Clear();
        Response.Write(sRet + "~~|~~" + Session["fv"].ToString());
    }

    protected string GetNDSIframe()
    {
        string sRet = "";
        if (Session["ft"] != null && Session["ft"].ToString() == "gib")
            sRet = "<iframe id=\"NDSFrame\" width=\"0px;\" height=\"0px;\" src=\"NDSPlayer.htm\"></iframe>";
        return sRet;
    }


}
