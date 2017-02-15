using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace WebAPI
{
    public class ExtWsPagesHendler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.ApplicationPath.ToLower().Contains("bill"))
            {
                switch (context.Request.AppRelativeCurrentExecutionFilePath.ToLower())
                {
                    case "~/adyen_api.aspx":
                        Page adyen_api_page = new WS_Billing.adyen_api();
                        adyen_api_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        adyen_api_page.ProcessRequest(context);
                        break;

                    case "~/googleinapp.aspx":
                        Page GoogleInApp_page = new WS_Billing.GoogleInApp();
                        GoogleInApp_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        GoogleInApp_page.ProcessRequest(context);
                        break;

                    case "~/m1_callback.aspx":
                        Page M1_Callback_page = new WS_Billing.M1_Callback();
                        M1_Callback_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        M1_Callback_page.ProcessRequest(context);
                        break;

                    case "~/plimus_api.aspx":
                        Page plimus_api_page = new WS_Billing.plimus_api();
                        plimus_api_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        plimus_api_page.ProcessRequest(context);
                        break;

                    case "~/plimus_callback.aspx":
                        Page Plimus_Callback_page = new WS_Billing.Plimus_Callback();
                        Plimus_Callback_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        Plimus_Callback_page.ProcessRequest(context);
                        break;

                    case "~/sc_api.aspx":
                        Page sc_api_page = new WS_Billing.sc_api();
                        sc_api_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        sc_api_page.ProcessRequest(context);
                        break;

                    case "~/tp_api.aspx":
                        Page tp_api_page = new WS_Billing.tp_api();
                        tp_api_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        tp_api_page.ProcessRequest(context);
                        break;

                    case "~/winpl_api.aspx":
                        Page winpl_api_page = new WS_Billing.winpl_api();
                        winpl_api_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        winpl_api_page.ProcessRequest(context);
                        break;

                    default:
                        break;
                }
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("social"))
            {
                switch (context.Request.AppRelativeCurrentExecutionFilePath.ToLower())
                {
                    case "~/facebook_api.aspx":
                        Page facebook_api_page = new WS_Social.facebook_api();
                        facebook_api_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        facebook_api_page.ProcessRequest(context);
                        break;

                    case "~/socialfeed.aspx":
                        Page SocialFeed_page = new WS_Social.SocialFeed.SocialFeed();
                        SocialFeed_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        SocialFeed_page.ProcessRequest(context);
                        break;

                    case "~/socialfeedtags.aspx":
                        Page SocialFeedTags_page = new WS_Social.SocialFeedTags();
                        SocialFeedTags_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        SocialFeedTags_page.ProcessRequest(context);
                        break;

                    default:
                        break;
                }
            }
            else if (context.Request.ApplicationPath.ToLower().Contains("users"))
            {
                switch (context.Request.AppRelativeCurrentExecutionFilePath.ToLower())
                {
                    case "~/oauth.aspx":
                        Page OAuth_page = new OAuth();
                        OAuth_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        OAuth_page.ProcessRequest(context);
                        break;

                    case "~/osaml.aspx":
                        Page OSaml_page = new WS_Users.OSaml();
                        OSaml_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        OSaml_page.ProcessRequest(context);
                        break;

                    case "~/sso.aspx":
                        Page SSO_page = new WS_Users.SSO();
                        SSO_page.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                        SSO_page.ProcessRequest(context);
                        break;

                    default:
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(context.Request.ApplicationPath) && context.Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains("clear_cache"))
            {
                Page clearCache = new clear_cache();
                clearCache.AppRelativeVirtualPath = context.Request.AppRelativeCurrentExecutionFilePath;
                clearCache.ProcessRequest(context);
            }
        }
    }
}
