using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Helper
{
    public class CookiesHelper
    {
        #region Members
        private System.Web.HttpCookie httpCookie;
        #endregion Members

        #region Properties
        public DateTime Expires
        {
            get
            {
                if (Enabled())
                {
                    return httpCookie.Expires;
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
                    httpCookie.Expires = value;
                }
            }
        }

        public bool HttpOnly
        {
            get
            {
                if (Enabled())
                {
                    return httpCookie.HttpOnly;
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
                    httpCookie.HttpOnly = value;
                }
            }
        }

        #endregion Properties

        #region Constractor
        public CookiesHelper(string Name)
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;

            if (context != null && context.Request.Browser.Cookies)
            {
                //get cookie from request, if available
                httpCookie = context.Request.Cookies[Name];
                
                //if cookie wasn't available, create a necontextw one.
                if (httpCookie == null)
                {
                    httpCookie = new System.Web.HttpCookie(Name);
                }


                // The following cookie setting ensures that cookies are not made available within client-side
                // script code thus mitigating the risk of unsafe access through attacks such as XSS.
                // This feature is generally available in versions of Internet Explorer 6 and above
                httpCookie.HttpOnly = true;
            }
        }
        #endregion Constractor

        /// <summary>
        /// Checks whether there's an HTTP context and if the browser supports cookies.
        /// </summary>
        /// <param name="Name"></param>
        public static bool Enabled()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            if (context != null && context.Request.Browser.Cookies)
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
                return httpCookie.Values[key];
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
                return httpCookie.Value;
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
                httpCookie.Values[key] = value;
                Write();
            }
        }


        private void Write()
        {
            if (Enabled())
            {
                System.Web.HttpContext context = System.Web.HttpContext.Current;
                context.Response.Cookies.Add(httpCookie);
            }
        }

        /// <summary>
        /// Attemps to remove the cookie from the client's browser by setting its expriation date to -30 years.
        /// </summary>
        public void Remove()
        {
            if (Enabled())
            {
                System.Web.HttpContext context = System.Web.HttpContext.Current;
                context.Response.Cookies[httpCookie.Name].Expires = DateTime.Now.AddYears(-30);
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
