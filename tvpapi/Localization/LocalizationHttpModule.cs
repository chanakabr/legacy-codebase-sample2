using System;
using System.Web;
using Tvinci.Helpers;
using Tvinci.Localization;
using Phx.Lib.Log;
using System.Reflection;

namespace Tvinci.Projects.Orange.TVS.HttpModule
{
    //public sealed class LocalizationHttpModule : IHttpModule
    //{
    //    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    //    public void Init(HttpApplication application)
    //    {
    //        application.PostAcquireRequestState += new EventHandler(application_PostAcquireRequestState);
    //    }

    //    void application_PostAcquireRequestState(object sender, EventArgs e)
    //    {
    //        HttpRequest request = ((HttpApplication)sender).Request;
    //        HttpContext context = ((HttpApplication)sender).Context;

    //        string filePath = context.Request.FilePath;
    //        string fileExtension = VirtualPathUtility.GetExtension(filePath);

    //        if (filePath.Contains("404"))
    //        {
    //            return;
    //        }

    //        if (fileExtension.Equals(".ashx"))
    //        {
    //            string ajaxRequestLangID = string.Empty;

    //            if (HttpContext.Current != null && HttpContext.Current.Request != null)
    //            {
    //                ajaxRequestLangID = HttpContext.Current.Request.Form["__SiteLanguageKey"];
    //            }

    //            if (!string.IsNullOrEmpty(ajaxRequestLangID))
    //            {
    //                LanguageManager.Instance.SetActiveLanguageByCulture(ajaxRequestLangID, true);
    //            }
    //            else
    //            {
    //                logger.Error(string.Format("Failed to assign AJAX request language of url '{0}'", context.Request.Url.OriginalString));
    //                throw new Exception("Failed to handle ajax request language");
    //            }
    //        }
    //        else if (fileExtension.Equals(".aspx"))
    //        {
    //            // try extract language from querystring
    //            string requestedLanguage = QueryStringHelper.GetString("Language", string.Empty).Trim();
    //            if (string.IsNullOrEmpty(requestedLanguage))
    //            {
    //                if (LanguageManager.Instance.LanguageScope == eLanguageScope.Request)
    //                {
    //                    LanguageManager.Instance.SetActiveLanguageToDefault();
    //                }
    //            }
    //            else
    //            {
    //                try
    //                {
    //                    LanguageManager.Instance.SetActiveLanguageByCulture(requestedLanguage, false);
    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.Error(string.Format("Failed to assign request language of url '{0}'", context.Request.Url.OriginalString), ex);
    //                    throw;
    //                }
    //            }
    //        }
    //    }

    //    public void Dispose() { }

    //}
}
