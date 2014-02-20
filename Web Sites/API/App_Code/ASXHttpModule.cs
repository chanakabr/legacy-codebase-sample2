using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ASXHttpModule
/// </summary>
public class ASXHttpModule : IHttpModule
{
    public ASXHttpModule()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public void Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(Context_BeginRequest);
        context.PreRequestHandlerExecute += new EventHandler (Application_AuthenticateRequest);
    }

    public void Dispose()
    {
    }

    private void Context_BeginRequest(object sender, EventArgs e)
    {
        HttpContext context = ((HttpApplication)sender).Context;
        HttpApplication application = sender as HttpApplication;
        if (application.Request.Url.ToString().ToUpper().EndsWith(".ASX") == true)
        {
            Logger.Logger.Log("rrr", "inside", "ttt");
            string sQuery = application.Request.Url.Query;
            context.Server.Transfer("asx_handler.aspx?" + sQuery);
        }
    }

    private void Application_AuthenticateRequest(object sender, EventArgs e)
    {
        /*
        HttpContext context = ((HttpApplication)sender).Context;
        HttpApplication application = sender as HttpApplication;
        if (application.Request.Url.ToString().ToUpper().EndsWith(".ASX") == true)
        {
            string sQuery = application.Request.Url.Query;
            context.Server.Transfer("asx_handler.aspx?" + sQuery);
        }
        */
    }
}
