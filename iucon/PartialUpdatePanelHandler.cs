// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using KLogMonitor;

namespace iucon.web.Controls
{
    public class PartialUpdatePanelHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constants
        private const string ISAJAX_REQUEST_ITEM = "IsAjaxRequestItem";
        #endregion

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["__USERCONTROLPATH"] != null)
            {
                try
                {
                    context.Items.Add(ISAJAX_REQUEST_ITEM, true);

                    // TODO Change to a normal file exists check

                    if (!string.IsNullOrEmpty(PartialUpdatePanelSingleton.Instance.BaseUrl))
                    {
                        // check if url contains querystring
                        if (PartialUpdatePanelSingleton.Instance.BaseUrl.Contains('?'))
                        {
                            context.RewritePath(PartialUpdatePanelSingleton.Instance.BaseUrl.Substring(0, PartialUpdatePanelSingleton.Instance.BaseUrl.IndexOf('?')), string.Empty,
                            PartialUpdatePanelSingleton.Instance.BaseUrl.Substring(PartialUpdatePanelSingleton.Instance.BaseUrl.IndexOf('?') + 1), true);
                        }
                        else
                        {
                            context.RewritePath(PartialUpdatePanelSingleton.Instance.BaseUrl, string.Empty, string.Empty, true);
                        }
                    }
                    else if (context.Request.UrlReferrer != null)
                    {
                        context.RewritePath(context.Request.UrlReferrer.LocalPath, "",
                            context.Request.UrlReferrer.Query.StartsWith("?") ? context.Request.UrlReferrer.Query.Substring(1) : context.Request.UrlReferrer.Query, true);
                    }

                    PanelHostPage page = new PanelHostPage(context.Request.Form["__USERCONTROLPATH"], context.Request.Form["__CONTROLCLIENTID"]);

                    ((IHttpHandler)page).ProcessRequest(context);

                    context.Response.Clear();
                    context.Response.Write(page.GetHtmlContent());
                }
                catch (Exception ex)
                {
                    // Prevent ScriptModule from reformatting the exception
                    if (HttpContext.Current.Items["System.Web.UI.PageRequestManager:AsyncPostBackError"] != null)
                        HttpContext.Current.Items["System.Web.UI.PageRequestManager:AsyncPostBackError"] = false;

                    logger.Error("Error occurred while performing Ajax request", ex);
                    context.Response.Write("Error occured while performing ajax request");

                    //if (ex.InnerException != null)
                    //{
                    //    context.Response.Write(ex.InnerException.Message.Replace("\n","<br />"));
                    //    context.Response.Write("<hr />");
                    //    context.Response.Write(ex.InnerException.StackTrace.Replace("\n", "<br />"));
                    //}
                    //else
                    //{
                    //    context.Response.Write(ex.Message.Replace("\n", "<br />"));
                    //    context.Response.Write("<hr />");
                    //    context.Response.Write(ex.StackTrace.Replace("\n", "<br />"));
                    //}                    
                }
            }
        }

        #endregion

        #region Static Methods
        public static bool IsRequestAjax()
        {
            if (HttpContext.Current.Items.Contains(ISAJAX_REQUEST_ITEM) &&
                HttpContext.Current.Items[ISAJAX_REQUEST_ITEM] is bool)
            {
                return (bool)HttpContext.Current.Items[ISAJAX_REQUEST_ITEM];
            }

            return false;
        }
        #endregion
    }
}
