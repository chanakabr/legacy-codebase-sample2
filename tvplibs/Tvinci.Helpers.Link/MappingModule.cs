using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Tvinci.Helpers;
using Tvinci.Web.HttpModules.Configuration;
using System.Text.RegularExpressions;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Helpers.Link
{
    public class MappingModule : IHttpModule
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public delegate bool HandleMappingDelegate(string token);

        public static HandleMappingDelegate HandleMappingMethod { get; set; }

        private bool shouldHandleMappingRequest(HttpContext context)
        {
            string mappingPath = QueryConfigManager.Instance.Data.General.FriendlyMapping.MappingURL;
            if (string.IsNullOrEmpty(mappingPath))
            {
                return false;
            }

            mappingPath = LinkHelper.ParseURL(mappingPath);

            Match urlToken = Regex.Match(context.Request.Url.ToString(), "^(.*?)[?]404;(.*)$");

            //if (!urlToken.Success || urlToken.Groups.Count != 2)
            //{
            //    return false;
            //}

            //if (!urlToken.Groups[2].Value.Contains(".aspx"))
            //{
            //    return false;
            //}

            string url = urlToken.Groups[1].Value;

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            return (mappingPath.ToLower() == url.ToLower());
        }

        private void handleMappingRequest(HttpContext context)
        {

            //logger.DebugFormat("Entered with query '{0}'", context.Request.QueryString);

            if (context.Request.QueryString.Count == 1)
            {
                string mappedFrom = context.Request.QueryString[0];

                if (!string.IsNullOrEmpty(mappedFrom))
                {
                    string[] token = mappedFrom.Split(';');

                    if (token.Length == 2)
                    {
                        if (token[0] == "404")
                        {
                            string value = LinkHelper.StripURL(token[1]);

                            if (HandleMappingMethod != null)
                            {
                                if (HandleMappingMethod(value))
                                {
                                    return;
                                }
                            }

                            context.Response.Clear();
                            context.Response.StatusCode = 404;
                            context.Response.End();
                            return;
                        }
                    }
                }
            }

            context.Response.Redirect(LinkHelper.ParseURL(QueryConfigManager.Instance.Data.General.FriendlyMapping.ErrorURL), true);
            context.Response.End();

            return;
        }

        #region IHttpModule Members

        public void Dispose()
        {
        }



        public void Init(HttpApplication context)
        {
            context.AcquireRequestState += new EventHandler(context_AcquireRequestState);
        }

        bool m_ignoreRequests = false;

        void context_AcquireRequestState(object sender, EventArgs e)
        {
            if (m_ignoreRequests)
            {
                return;
            }

            if (QueryConfigManager.Instance.SyncMode == Tvinci.Configuration.eMode.NotSynced)
            {
                logger.ErrorFormat("This module requires 'QueryConfigManager' instance to be synced. Ignoring requests");
                m_ignoreRequests = true;
                return;
            }

            HttpRequest request = ((HttpApplication)sender).Request;
            HttpContext context = ((HttpApplication)sender).Context;
            string filePath = LinkHelper.GetPageVirtualPath();

            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (fileExtension.Equals(".aspx"))
            {
                if (shouldHandleMappingRequest(context))
                {
                    handleMappingRequest(context);
                }
            }
        }

        #endregion
    }
}
