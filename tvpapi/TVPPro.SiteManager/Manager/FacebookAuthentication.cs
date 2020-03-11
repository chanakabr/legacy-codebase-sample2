using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TVinciShared;

namespace TVPPro.SiteManager.Manager
{
    class FacebookAuthentication
    {
        public FacebookAuthentication()
        {

        }

        public static bool isConnected()
        {
            return (SessionKey != null && UserID != -1);
        }

        public static bool isSessionKeyChanged(string CurrentSessionKey)
        {
            bool ret = SessionKey != CurrentSessionKey;
            return ret;
        }

        public static string ApiKey
        {
            get
            {
                return TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FacebookConnect.API_Key; ; //Should be taken from configuration 
            }
        }

        public static string SecretKey
        {
            get
            {

                return TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FacebookConnect.Secret_Key;//Should be taken from configuration 
            }
        }

        public static string SessionKey
        {
            get
            {
                return GetFacebookCookie("session_key");
            }
        }

        public static int UserID
        {
            get
            {
                int userID = -1;
                int.TryParse(GetFacebookCookie("user"), out userID);
                return userID;
            }
        }

        private static string GetFacebookCookie(string cookieName)
        {
            string retString = null;
            string fullCookie = ApiKey + "_" + cookieName;

            if (HttpContext.Current.Request.Cookies[fullCookie] != null)
                retString = HttpContext.Current.Request.Cookies.GetValue(fullCookie);

            return retString;
        }
    }
}
