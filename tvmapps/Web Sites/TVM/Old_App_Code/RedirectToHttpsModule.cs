using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Redirects a request to the HTTPS site. 
/// </summary>
public class RedirectToHttpsModule : IHttpModule
{
    #region Constants

    private const string HTTPS = "https";
    private const string HTTP = "http";

    #endregion

    #region IHttpModule Members

    public void Dispose()
    {
        // Nothing to dispose. 
    }

    public void Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(context_BeginRequest);
    }

    void context_BeginRequest(object sender, EventArgs e)
    {
        
    }

    #endregion
}
