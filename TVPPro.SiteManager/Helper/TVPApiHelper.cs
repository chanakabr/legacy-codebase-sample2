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
        static bool useTVPAPI = true;
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
            GetMediaLicenseLinkWithIP
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

        public static object GetInitObj()
        {
            return new
            {
                Locale = new
                {
                    LocaleLanguage = "",
                    LocaleCountry = "",
                    LocaleDevice = "",
                    LocaleUserState = "Unknown"
                },
                Platform = "iPad",
                SiteGuid =Services.UsersService.Instance.GetUserID(),
                DomainID =Services.UsersService.Instance.GetDomainID() ,
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

        public static string RecordAll(string channelCode, string recordDate, string recordTime, string versionId)
        {
            var response = "";
            string userID = Services.UsersService.Instance.GetUserID();

            UserResponseObject userData = Services.UsersService.Instance.GetUserData(userID);
            if (userData != null)
            {
                UserDynamicDataContainer accNumDynamicData = userData.m_user.m_oDynamicData.m_sUserData.Where(item => item.m_sDataType.Equals("accNum")).FirstOrDefault();
                string accountNumber = (accNumDynamicData != null) ? accNumDynamicData.m_sValue : null;

                var postData = new
                {
                    initObj = GetInitObj(),
                    accountNumber = accountNumber,
                    channelCode = channelCode,
                    recordDate = recordDate,
                    recordTime = recordTime,
                    versionId = versionId
                };
                if (useTVPAPI)
                {
                    response = MakeRequest(TVPApiHelper.TVPAPI_METHODS.RecordAll, new JavaScriptSerializer().Serialize(postData));
                }
                return response;
            }
            else
            {
                throw new Exception("UserData is null, probably logged out");
            }
        }
    }
}
