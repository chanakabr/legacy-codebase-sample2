using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Web;

namespace TVPPro.SiteManager.Helper
{
    public class CookiesHelper
    {
        #region Members
        private string httpCookie;
        private string name;
        private DateTime expires;
        private bool httpOnly;

        #endregion Members

        #region Properties
        public DateTime Expires
        {
            get
            {
                if (Enabled())
                {
                    return expires;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            set
            {
                if (Enabled())
                {
                    expires = value;
                }
            }
        }

        public bool HttpOnly
        {
            get
            {
                if (Enabled())
                {
                    return httpOnly;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (Enabled())
                {
                    httpOnly = value;
                }
            }
        }

        #endregion Properties

        #region Constractor
        public CookiesHelper(string Name)
        {
            name = Name;
            var context = System.Web.HttpContext.Current;

            if (context != null && context.Request.Cookies != null)
            {
                //get cookie from request, if available
                httpCookie = context.Request.Cookies.GetValue(Name);

                ////if cookie wasn't available, create a new one.
                //if (string.IsNullOrEmpty(httpCookie))
                //{
                //    httpCookie = ;
                //}


                // The following cookie setting ensures that cookies are not made available within client-side
                // script code thus mitigating the risk of unsafe access through attacks such as XSS.
                // This feature is generally available in versions of Internet Explorer 6 and above
                //httpCookie.HttpOnly = true;
            }
        }
        #endregion Constractor

        /// <summary>
        /// Checks whether there's an HTTP context and if the browser supports cookies.
        /// </summary>
        /// <param name="Name"></param>
        public static bool Enabled()
        {
            var context = System.Web.HttpContext.Current;
            if (context != null && context.Request.Cookies != null)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public string GetValue(string key)
        {
            if (Enabled())
            {
                var context = System.Web.HttpContext.Current;
                return context.Request.Cookies.GetValue(key);
            }
            else
            {
                return String.Empty;
            }
        }

        public string GetValue()
        {
            if (Enabled())
            {
                return httpCookie;
            }
            else
            {
                return String.Empty;
            }
        }

        public void SetValue(string key, string value)
        {
            if (Enabled())
            {
                var context = System.Web.HttpContext.Current;
                context.Request.Cookies.SetValue(key, value);
                Write();
            }
        }


        private void Write()
        {
            if (Enabled())
            {
                var context = System.Web.HttpContext.Current;
                context.Response.Cookies.SetValue(name, httpCookie);
                //context.Response.Cookies.Add(httpCookie);
            }
        }

        /// <summary>
        /// Attemps to remove the cookie from the client's browser by setting its expriation date to -30 years.
        /// </summary>
        public void Remove()
        {
            if (Enabled())
            {
                var context = System.Web.HttpContext.Current;
                context.Response.Cookies.Remove(name);
            }
        }

        public static Dictionary<string, string> GetRememberMeCookieData()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            
            dic.Add("UserId", TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user.m_sSiteGUID);
            dic.Add("UserName", TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user.m_oBasicData.m_sUserName);
            dic.Add("NickName", TVPPro.SiteManager.Services.UsersService.Instance.GetUserNickName());
            dic.Add("FirstName", TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user.m_oBasicData.m_sFirstName);

            return dic;
        }
    }
}
