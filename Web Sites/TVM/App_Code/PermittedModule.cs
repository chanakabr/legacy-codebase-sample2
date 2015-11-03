using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using KLogMonitor;
using TVinciShared;

/// <summary>
/// Summary description for PermittedModule
/// </summary>
public class PermittedModule : IHttpModule
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    public PermittedModule()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public void Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(Context_BeginRequest);
        context.PreRequestHandlerExecute += new EventHandler(Application_AuthenticateRequest);
        context.AcquireRequestState += (new EventHandler(this.OnAcquireRequestState));
    }

    public void Dispose()
    {
    }

    private void OnAcquireRequestState(Object source, EventArgs ea)
    {
        HttpApplication app = (HttpApplication)source;
        if (app.Request.QueryString["platform"] != null)
            app.Session["platform"] = app.Request.QueryString["platform"].ToString();
    }

    private void Application_AuthenticateRequest(object sender, EventArgs e)
    {
        HttpContext context = ((HttpApplication)sender).Context;
        HttpApplication application = sender as HttpApplication;

        string sAppSateConfigValue = TVinciShared.WS_Utils.GetTcmConfigValue("APP_STATE");

        if (!string.IsNullOrEmpty(sAppSateConfigValue) && sAppSateConfigValue == "moved_to_ny")
        {
            if (application.Request.Url.ToString().ToLower().EndsWith("moved.html") == false)
                context.Server.Transfer("moved.html");
            context.Response.End();
            return;
        }

        string sIpAddress = context.Request.UserHostAddress;
        string sRefferer = "";
        if (context.Request.UrlReferrer != null)
            sRefferer = context.Request.UrlReferrer.Host;
        if (sRefferer.Trim().ToLower() == "localhost" || sRefferer.Trim().ToLower() == "admin.tvinci.com" || sRefferer.Trim().ToLower() == "tvm.tvinci.com")
            return;
        if (application.Request.Url.ToString().ToUpper().EndsWith("LOGIN.HTML") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("TOKEN.HTML") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("TOKEN_ENTER.HTML") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("XTI_LISTENER.ASPX") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("TOOLS.HTML") == false &&
            application.Request.Url.ToString().ToUpper().IndexOf(".SVC") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".JS") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".CSS") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".PNG") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".JPG") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".GIF") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".ICO") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".XAP") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".XML") == -1 &&
            application.Request.Url.PathAndQuery != "/")
        {
            if (LoginManager.CheckLogin() == false)
            {
                context.Server.Transfer("login.html");
                //context.Response.StatusCode = 403; // (Forbidden)
            }
        }
    }


    private void Context_BeginRequest(object sender, EventArgs e)
    {
        HttpContext context = ((HttpApplication)sender).Context;
        HttpApplication application = sender as HttpApplication;

        string sAppSateConfigValue = TVinciShared.WS_Utils.GetTcmConfigValue("APP_STATE");

        if (!string.IsNullOrEmpty(sAppSateConfigValue) && sAppSateConfigValue == "moved_to_ny")
        {
            if (application.Request.Url.ToString().ToLower().EndsWith("moved.html") == false)
                context.Server.Transfer("moved.html");
            context.Response.End();
            return;
        }
        string sRefferer = "";
        if (context.Request.UrlReferrer != null)
            sRefferer = context.Request.UrlReferrer.Host;

        bool bCont = true;
        if (application.Request.Url.ToString().ToUpper().IndexOf("LOGIN", 0) != -1 ||
            application.Request.Url.ToString().ToUpper().IndexOf("TOKEN", 0) != -1 ||
            application.Request.Url.ToString().ToUpper().IndexOf("TOOLS", 0) != -1 ||
            application.Request.Url.PathAndQuery == "/")
        {
            if (!application.Request.IsSecureConnection)
            {
                if (application.Request.Url.Host != "localhost" &&
                    application.Request.Url.Host != "127.0.0.1")
                {
                    application.Response.Redirect(application.Request.Url.ToString().Replace(application.Request.Url.Scheme, "https"));
                }
                bCont = false;
            }
        }
        else
        {
            if (application.Request.UrlReferrer == null)
            {
                //if (application.Request.Form.AllKeys.Length > 0 || application.Request.QueryString.AllKeys.Length > 0)
                //{
                //context.Server.Transfer("500.html");
                //bCont = false;
                //}
            }
            if (application.Request.UrlReferrer != null && application.Request.UrlReferrer.Host != application.Request.Url.Host)
            {
                if (application.Request.Url.ToString().ToUpper().IndexOf(".PNG") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".JPG") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".GIF") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".ICO") == -1)
                {
                    context.Server.Transfer("500.html");
                    bCont = false;
                }
            }
            if (application.Request.IsSecureConnection)
            {
                if (application.Request.Url.Host != "localhost" &&
                    application.Request.Url.Host != "127.0.0.1" &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".SVC") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".JS") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".CSS") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".PNG") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".JPG") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".GIF") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".ICO") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".XAP") == -1 &&
                    application.Request.Url.ToString().ToUpper().IndexOf(".XML") == -1)
                {
                    application.Response.Redirect(application.Request.Url.ToString().Replace(application.Request.Url.Scheme, "http"));
                }
                bCont = false;
            }
        }
        if (bCont == false)
            return;

        string sIpAddress = context.Request.UserHostAddress;

        if (sRefferer.Trim().ToLower() == "localhost" || sRefferer.Trim().ToLower() == "admin.tvinci.com" || sRefferer.Trim().ToLower() == "tvm.tvinci.com")
            return;
        bool bIpOK = IsIPPermitted(sIpAddress);
        if (application.Request.Url.ToString().ToUpper().EndsWith("LOGIN.HTML") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("TOKEN.HTML") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("TOKEN_ENTER.HTML") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("XTI_LISTENER.ASPX") == false &&
            application.Request.Url.ToString().ToUpper().EndsWith("TOOLS.HTML") == false &&
            application.Request.Url.ToString().ToUpper().IndexOf(".SVC") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".JS") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".CSS") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".PNG") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".JPG") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".GIF") == -1 &&
            application.Request.Url.ToString().ToUpper().IndexOf(".ICO") == -1 &&
            application.Request.Url.PathAndQuery != "/")

            if (bIpOK == false)
            {
                context.Response.StatusCode = 403; // (Forbidden)
            }

        if (application.Request.QueryString["RC"] != null ||
            application.Request.QueryString["M"] != null)
        {
            context.Response.StatusCode = 403;
        }
    }

    static protected bool IsIPPermitted(string sIP)
    {
        if (sIP == "127.0.0.1")
            return true;
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from groups_ips where ADMIN_OPEN=1 and is_active=1 and status=1 and (END_DATE is null OR END_DATE>getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ip", "=", sIP);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nID != 0)
            return true;

        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select id from xti where ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("caller_ip", "=", sIP);
        if (selectQuery1.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
            if (nCount > 0)
                nID = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery1.Finish();
        selectQuery1 = null;
        if (nID != 0)
            return true;
        return false;
    }
}
