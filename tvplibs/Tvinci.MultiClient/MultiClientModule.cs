using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Tvinci.Helpers;
using System.Web.SessionState;
using Tvinci.MultiClient.Configuration;
using System.Text.RegularExpressions;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.MultiClient
{
    public class MultiClientModule : IHttpModule, IRequiresSessionState
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region IHttpModule Members

        public void Dispose()
        {
            // do nothing
        }

        public void Init(HttpApplication context)
        {
            context.AcquireRequestState += new EventHandler(context_AcquireRequestState);

            try
            {
                Tvinci.MultiClient.MultiClientHelper.Instance.Sync();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to sync clients list", ex);
            }

        }

        void context_AcquireRequestState(object sender, EventArgs e)
        {
            HttpRequest request = ((HttpApplication)sender).Request;
            HttpContext context = ((HttpApplication)sender).Context;

            if (context.Session == null)
            {
                return;
            }

            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (!fileExtension.Equals(".aspx"))
            {
                return;
            }

            if (string.IsNullOrEmpty(MultiClientHelper.Instance.ActiveUserClient.ClientIdentifier))
            {
                string valueFromCookie = MultiClientHelper.Instance.GetClientIDFromCookie();

                if (MultiClientHelper.Instance.TryLoginClient(valueFromCookie))
                {
                    // no implementation needed here
                }
            }

            if (!MultiClientHelper.Instance.ValidateConfigurationRestriction())
            {
                string redirectTo = LinkHelper.ParseURL(MultiClientHelper.Instance.Data.Pages.NoClientURL);
                redirectTo = QueryStringHelper.CreateQueryString(redirectTo, new QueryStringPair("CallerURL", HttpContext.Current.Request.Url.AbsoluteUri));
                if (shouldRedirect(context, redirectTo))
                {
                    context.Response.Redirect(redirectTo);
                    context.Response.End();

                }
            }
        }

        private bool shouldRedirect(HttpContext context, string redirectTo)
        {
            if (string.IsNullOrEmpty(redirectTo))
            {
                return false;
            }

            redirectTo = LinkHelper.GetLinkWithoutQuerystring(redirectTo).ToLower();
            string currentPageURL = context.Request.Url.GetLeftPart(UriPartial.Path).ToLower();

            if (currentPageURL == redirectTo)
            {
                return false;
            }

            foreach (string url in MultiClientHelper.Instance.Data.Pages.IgnoredPages)
            {
                if (currentPageURL == LinkHelper.ParseURL(url).ToLower())
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
