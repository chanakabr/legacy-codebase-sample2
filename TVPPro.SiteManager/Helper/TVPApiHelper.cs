using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;
using System.Web.Script.Serialization;

namespace TVPPro.SiteManager.Helper
{
    public static class TVPApiHelper
    {
        //static bool useTVPAPI = true;
        static string baseTVPapiURL = System.Configuration.ConfigurationManager.AppSettings["BASE_TVPAPI_URL"];
        static JavaScriptSerializer json = new JavaScriptSerializer();

        public enum TVPAPI_METHODS
        {
            FBUserMerge,
            FBConfig,
            GetFBUserData,
            DoUserAction,
            SetUserSocialPrivacy,
            GetUserActions,
            GetUsersData,
            SetUserExternalActionShare,
            GetUserExternalActionShare,
            GetItemFromList,
            IsItemExistsInList,
            RemoveItemFromList,
            AddItemToList,
            UpdateItemInList,
            RecordAll,
            GetMediaLicenseLink,
            GetMediaLicenseLinkWithIP,
            MediaHit,
            SetUserDynamicDataEx,
            FBTokenValidation,
            GetAutoCompleteSearchList,
            GetUserActivityFeed,
            GetCrowdsourceFeed,
            GetAccountSTBs,
            MediaMark
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string MakeRequest(TVPAPI_METHODS method, string postData)
        {
            string url = getRequestURL(method);

            return SendRequest<string>(url, postData);
        }

        static private T SendRequest<T>(string url, string postData)
        {
            Stream dataStream = null;
            StreamReader reader = null;
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentLength = byteArray.Length;

                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                response = request.GetResponse();
                dataStream = response.GetResponseStream();

                reader = new StreamReader(dataStream);
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                if (typeof(T) == typeof(String))
                {
                    string resposne = reader.ReadToEnd().Trim(new char[] { '\"' });
                    return (T)Convert.ChangeType(resposne, typeof(T));
                }

                return serializer.Deserialize<T>(reader.ReadToEnd());

            }
            finally
            {
                // Clean up the streams.
                if (reader != null) reader.Close();
                if (dataStream != null) dataStream.Close();
                if (response != null) response.Close();
            }

        }

        private static string getRequestURL(TVPAPI_METHODS method)
        {
            return baseTVPapiURL + method;
        }

        public static object GetInitObj(string language = "")
        {
            return GetInitObj(Services.UsersService.Instance.GetDomainID(), Services.UsersService.Instance.GetUserID(), language);
        }

        public static object GetInitObj(int domainID, string siteGuid, string language = "")
        {
            return new
            {
                Locale = new
                {
                    LocaleLanguage = language,
                    LocaleCountry = "",
                    LocaleDevice = "",
                    LocaleUserState = "Unknown"
                },
                Platform = "iPad",
                SiteGuid = siteGuid,
                DomainID = domainID,
                UDID = DeviceDNAHelper.GetDeviceDNA(),
                ApiUser = "tvpapi_153",
                ApiPass = "11111"
            };
        }

        public static void CastResponse<T>(string response, out T responseObj)
        {
            responseObj = default(T);
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    responseObj = (T)json.DeserializeObject(response);
                }
                catch { }
            }
        }
    }
}
