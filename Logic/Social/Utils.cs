using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Data;
using Core.Users;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using Core.Billing;
using ApiObjects.Statistics;
using Newtonsoft.Json.Linq;
using ApiObjects.Billing;

namespace Core.Social
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public string GetValFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static UserResponseObject GetUserDataByID(string sUserGuid, Int32 nGroupID)
        {
            UserResponseObject response = null;

            try
            {
                response = Core.Users.Module.GetUserData(nGroupID, sUserGuid, string.Empty);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (GetUserDataByID). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return response;
        }

        static public UserResponseObject GetUserDataByFacebookID(string sUserFacebookID, Int32 nGroupID)
        {
            UserResponseObject response = null;
            try
            {
                response = Core.Users.Module.GetUserByFacebookID(nGroupID, sUserFacebookID);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (GetUserDataByFacebookID). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return response;
        }

        static public UserResponseObject GetUserByUsername(string sUsername, Int32 nGroupID)
        {
            UserResponseObject response = null;
            try
            {
                response = Core.Users.Module.GetUserByUsername(nGroupID, sUsername);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (GetUserByUsername). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return response;
        }

        static public UserResponseObject AddNewUser(Int32 nGroupID, UserBasicData ubd, UserDynamicData udd, string sPass, string sAffiliateCode)
        {
            UserResponseObject response = null;
            try
            {
                response = Core.Users.Module.AddNewUser(nGroupID, ubd, udd, sPass, sAffiliateCode);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (AddNewUser). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return response;

        }

        static public UserResponseObject SetUserData(Int32 nGroupID, string sSiteGuid, UserBasicData ubd, UserDynamicData udd)
        {
            UserResponseObject response = null;
            try
            {
                response = Core.Users.Module.SetUserData(nGroupID, sSiteGuid, ubd, udd);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (SetUserData). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }
            return response;
        }

        private static void GetCacheCredentials(Int32 nGroupID, out string sWSUserName, out string sWSPass, eWSModules toWSModules)
        {
            sWSUserName = string.Empty;
            sWSPass = string.Empty;
            Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(eWSModules.SOCIAL, nGroupID, toWSModules);
            if (oCredentials != null)
            {
                sWSUserName = oCredentials.m_sUsername;
                sWSPass = oCredentials.m_sPassword;
            }
        }

        static public UserResponseObject CheckUserPassword(Int32 nGroupID, string sUserName, string sPassword, bool bPreventDoubleLogins)
        {
            UserResponseObject response = null;
            try
            {
                response = Core.Users.Module.CheckUserPassword(nGroupID, sUserName, sPassword, bPreventDoubleLogins);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (CheckUserPassword). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return response;
        }

        static public DomainResponseObject AddNewDomain(Int32 nGroupID, User user)
        {
            DomainResponseObject dObj = new DomainResponseObject();
            dObj.m_oDomainResponseStatus = DomainResponseStatus.UnKnown;
            try
            {
                var res = Core.Domains.Module.AddDomain(nGroupID, user.m_oBasicData.m_sFirstName + "'s Domain", string.Empty, int.Parse(user.m_sSiteGUID));
                if (res != null)
                {
                    dObj = res.DomainResponse;
                }
            }
            catch (Exception ex)
            {
                dObj.m_oDomainResponseStatus = DomainResponseStatus.Error;
                string msg = string.Format("GroupID:{0}, SITE_GUID:{1}; msg:{3}", nGroupID, user.m_sSiteGUID, ex.Message);
                log.Error("AddNewDomain - ERROR - " + msg, ex);
            }

            return dObj;
        }

        static public BillingResponse DummyChargeUserForSubscription(int nGroupID, string sSiteGUID, string sSubscriptionCode, string sCouponCode, string sUserIP)
        {
            BillingResponse bObj = new BillingResponse();
            bObj.m_oStatus = BillingResponseStatus.UnKnown;
            try
            {
                bObj = Core.ConditionalAccess.Module.CC_DummyChargeUserForSubscription(nGroupID, sSiteGUID, 0, "EUR", sSubscriptionCode, sCouponCode, sUserIP, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                bObj.m_oStatus = BillingResponseStatus.Fail;
                string msg = string.Format("GroupID:{0}, SITE_GUID:{1}, SubID:{2} ; msg:{3}", nGroupID, sSiteGUID, sSubscriptionCode, ex.Message);
                log.Error("DummyChargeUserForSubscription - ERROR - " + msg, ex);
            }

            return bObj;
        }

        public static string Encrypt(string toEncrypt, string key)
        {
            MD5CryptoServiceProvider hashmd5 = null;
            TripleDESCryptoServiceProvider tdes = null;
            ICryptoTransform cTransform = null;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
                hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));


                tdes = new TripleDESCryptoServiceProvider();
                //set the secret key for the tripleDES algorithm
                tdes.Key = keyArray;
                //mode of operation. there are other 4 modes.
                //We choose ECB(Electronic code Book)
                tdes.Mode = CipherMode.ECB;
                //padding mode(if any extra byte added)

                tdes.Padding = PaddingMode.PKCS7;

                cTransform = tdes.CreateEncryptor();
                //transform the specified region of bytes array to resultArray
                byte[] resultArray =
                  cTransform.TransformFinalBlock(toEncryptArray, 0,
                  toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at Encrypt. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                log.Error("SocialErrors - " + sb.ToString(), ex);
                #endregion
            }
            finally
            {
                #region Disposing
                if (hashmd5 != null)
                {
                    hashmd5.Clear();
                }
                if (tdes != null)
                {
                    tdes.Clear();
                }
                if (cTransform != null)
                {
                    cTransform.Dispose();
                }
                #endregion
            }

            return string.Empty;
        }

        public static string Decrypt(string cipherString, string key)
        {
            MD5CryptoServiceProvider hashmd5 = null;
            TripleDESCryptoServiceProvider tdes = null;
            ICryptoTransform cTransform = null;
            string res = string.Empty;
            try
            {
                byte[] keyArray;
                //get the byte code of the string

                byte[] toEncryptArray = Convert.FromBase64String(cipherString);

                //if hashing was used get the hash code with regards to your key
                hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));


                tdes = new TripleDESCryptoServiceProvider();
                //set the secret key for the tripleDES algorithm
                tdes.Key = keyArray;
                //mode of operation. there are other 4 modes. 
                //We choose ECB(Electronic code Book)

                tdes.Mode = CipherMode.ECB;
                //padding mode(if any extra byte added)
                tdes.Padding = PaddingMode.PKCS7;

                cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(
                                     toEncryptArray, 0, toEncryptArray.Length);

                //return the Clear decrypted TEXT
                res = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Could not decrypt cipher text. Cipher={0};ex={1};stack={2}", cipherString, ex.Message, ex.StackTrace), ex); ;
            }
            finally
            {
                #region Disposing
                if (hashmd5 != null)
                {
                    hashmd5.Clear();
                }
                if (tdes != null)
                {
                    tdes.Clear();
                }
                if (cTransform != null)
                {
                    cTransform.Dispose();
                }
                #endregion
            }
            return res;
        }

        static public string SendGetHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword)
        {
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            HttpWebResponse oWebResponse = null;
            Stream receiveStream = null;
            Int32 nStatusCode = -1;
            Encoding enc = new UTF8Encoding(false);
            try
            {
                oWebRequest.Credentials = new NetworkCredential(sUserName, sPassword);
                oWebRequest.Timeout = 1000000;
                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream, enc);
                string resultString = sr.ReadToEnd();

                sr.Close();

                oWebResponse.Close();
                oWebRequest = null;
                oWebResponse = null;
                nStatus = nStatusCode;
                return resultString;
            }
            catch (Exception ex)
            {
                log.Error("Notifier - SendGetHttpReq exception:" + ex.Message + " to: " + sUrl, ex);
                if (oWebResponse != null)
                    oWebResponse.Close();
                if (receiveStream != null)
                    receiveStream.Close();
                nStatus = 404;
                return ex.Message;
            }
        }

        static public string SendDeleteHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword, string sParams)
        {
            Int32 nStatusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "DELETE";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParams);
            webRequest.ContentLength = bytes.Length;
            System.IO.Stream os = webRequest.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();

            string res = string.Empty;
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                HttpStatusCode sCode = webResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(webResponse.GetResponseStream());
                    res = sr.ReadToEnd();
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }

            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    res = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }
            }

            nStatus = nStatusCode;
            return res;
        }

        static public string SendPostHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword, string sParams)
        {
            Int32 nStatusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParams);
            webRequest.ContentLength = bytes.Length;
            System.IO.Stream os = webRequest.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();

            string res = string.Empty;
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                HttpStatusCode sCode = webResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(webResponse.GetResponseStream());
                    res = sr.ReadToEnd();
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }

            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    res = errorStream.ReadToEnd();
                    log.Error("Error - SendPostHttpReq exception:" + ex.Message + "; error=" + res + " to: " + sUrl, ex);
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }
            }

            nStatus = nStatusCode;
            return res;
        }

        static public string SendPostHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword, string sParams, Dictionary<string, string> headers = null)
        {
            Int32 nStatusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            if (headers != null)
            {
                SetRequestHeaders(webRequest, headers);
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParams);
            webRequest.ContentLength = bytes.Length;
            System.IO.Stream os = webRequest.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();

            string res = string.Empty;
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                HttpStatusCode sCode = webResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(webResponse.GetResponseStream());
                    res = sr.ReadToEnd();
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }

            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    res = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }
            }

            nStatus = nStatusCode;
            return res;
        }


        public static List<T> GetTopRecords<T>(int nTotalItems, int nStartIndex, List<T> list)
        {
            List<T> lResult = new List<T>();
            if (list == null || list.Count < nStartIndex)
                return lResult;

            lResult = list;

            if (nStartIndex > 0)
                lResult = list.Skip(nStartIndex).ToList();

            if (nTotalItems > 0 && nTotalItems < lResult.Count)
                lResult = lResult.Take(nTotalItems).ToList();

            return lResult;

        }

        static protected Int32 GetResponseCode(HttpStatusCode theCode)
        {
            if (theCode == HttpStatusCode.OK)
                return (int)HttpStatusCode.OK;
            if (theCode == HttpStatusCode.NotFound)
                return (int)HttpStatusCode.NotFound;
            return (int)HttpStatusCode.InternalServerError;

        }

        public static T Deserialize<T>(string sObject)
        {
            T response = default(T);
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                response = serializer.Deserialize<T>(sObject);
            }
            catch
            {
            }
            return response;
        }

        static public string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader, ref Int32 nStatus)
        {
            try
            {
                nStatus = 500;

                //Create the HTTP POST request and the authentication headers
                HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
                oWebRequest.Method = "post";
                oWebRequest.ContentType = "text/xml; charset=utf-8";
                //oWebRequest.Headers["SOAPAction"] = sSoapHeader;

                byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
                //oWebRequest.ContentLength = encodedBytes.Length;
                //oWebRequest.AllowWriteStreamBuffering = true;

                //Send the request.
                Stream requestStream = oWebRequest.GetRequestStream();
                requestStream.Write(encodedBytes, 0, encodedBytes.Length);
                requestStream.Close();

                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream);
                string resultString = sr.ReadToEnd();

                sr.Close();
                oWebRequest = null;
                oWebResponse = null;

                nStatus = GetResponseCode(sCode);

                return resultString;
            }
            catch (Exception ex)
            {
                log.Error("SendXMLHttpReq error - " + ex.Message, ex);
                return ex.Message;
            }
        }

        static public byte[] encryptStringToBytes_AES(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the stream used to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt = null;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {

                    // Create the streams used for encryption.
                    using (msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {

                                //Write all data to the stream.
                                swEncrypt.Write(plainText);
                            }
                        }

                        // Return the encrypted bytes from the memory stream.
                        return msEncrypt.ToArray();
                    }
                }

            }
            finally
            {

                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

        }

        static public string GetEncryptPass(string sSiteGuid)
        {
            string sPass = string.Empty;

            int nSiteGuid = 0;

            if (!Int32.TryParse(sSiteGuid, out nSiteGuid))
            {
                return sPass;
            }

            Object oPass = ODBCWrapper.Utils.GetTableSingleVal("users", "password", nSiteGuid, "users_connection");
            if (oPass != null && oPass != DBNull.Value)
            {
                sPass = oPass.ToString();

                using (AesManaged aes = new AesManaged())
                {

                    string Key = Utils.GetValFromConfig("SecureSiteGuidKey");
                    string IV = Utils.GetValFromConfig("SecureSiteGuidIV");

                    aes.Key = Convert.FromBase64String(Key);
                    aes.IV = Convert.FromBase64String(IV);

                    sPass = Convert.ToBase64String(Utils.encryptStringToBytes_AES(sPass, aes.Key, aes.IV));
                }
            }

            return sPass;


        }

        public static void AddToDynamicData(string type, string value, ref List<UserDynamicDataContainer> dynDataList)
        {
            if (!string.IsNullOrEmpty(value))
            {
                UserDynamicDataContainer oGender = new UserDynamicDataContainer();
                oGender.m_sDataType = type;
                oGender.m_sValue = value;

                dynDataList.Add(oGender);
            }
        }

        public static string ConvertFBInterestsToJsonString(List<FBInterestData> interests)
        {

            Dictionary<string, List<string>> interestDict = new Dictionary<string, List<string>>();
            foreach (FBInterestData item in interests)
            {
                if (!interestDict.ContainsKey(item.category))
                {
                    interestDict.Add(item.category, new List<string>());
                }
                interestDict[item.category].Add(item.name);
            }
            JavaScriptSerializer ser = new JavaScriptSerializer();
            return ser.Serialize(interestDict);
        }

        public static UserBasicData GetFBBasicData(FBUser fbUser, string sEncryptToken, string sPic)
        {
            UserBasicData ubd = new UserBasicData();
            ubd.m_sUserName = fbUser.email;
            ubd.m_sLastName = fbUser.last_name;
            ubd.m_sFirstName = fbUser.first_name;
            ubd.m_sEmail = fbUser.email;
            ubd.m_sFacebookID = fbUser.id;
            ubd.m_sFacebookToken = sEncryptToken;
            ubd.m_sFacebookImage = sPic;

            return ubd;
        }

        public static string GetPassword()
        {
            string[] chuncks = Guid.NewGuid().ToString().Split('-');
            return chuncks[0];
        }

        public static string GetValFromKVP(List<ApiObjects.KeyValuePair> vals, string key)
        {
            string res = string.Empty;

            foreach (ApiObjects.KeyValuePair ekv in vals)
            {
                if (ekv.key == key)
                {
                    res = ekv.value;
                    break;
                }
            }

            return res;
        }

        static public void AddParameter(string name, string val, ref string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                parameters += "&";
            }

            parameters += name + "=" + val;
        }

        static public string ConvertToFBDate(DateTime date)
        {
            return string.Format("{0}-{1}-{2}T{3}:{4}:{5}", date.Year.ToString(), date.Month.ToString(), date.Day.ToString(), date.Hour.ToString(), date.Minute.ToString(), date.Second.ToString());
        }

        public static string GetActions(eUserAction eAction)
        {
            CommaDelimitedStringCollection lActions = new CommaDelimitedStringCollection();

            if (eUserAction.POST == (eAction & eUserAction.POST))
            {
                lActions.Add(((int)eUserAction.POST).ToString());
            }
            if (eUserAction.LIKE == (eAction & eUserAction.LIKE))
            {
                lActions.Add(((int)eUserAction.LIKE).ToString());
            }
            if (eUserAction.SHARE == (eAction & eUserAction.SHARE))
            {
                lActions.Add(((int)eUserAction.SHARE).ToString());
            }
            if (eUserAction.WANTS_TO_WATCH == (eAction & eUserAction.WANTS_TO_WATCH))
            {
                lActions.Add(((int)eUserAction.WANTS_TO_WATCH).ToString());
            }
            if (eUserAction.WATCHES == (eAction & eUserAction.WATCHES))
            {
                lActions.Add(((int)eUserAction.WATCHES).ToString());
            }

            return lActions.ToString();
        }

        public static string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        //Some headers must be modified via the property and cannot be changed by modifing the req.Headers obj
        private static void SetRequestHeaders(WebRequest webRequest, Dictionary<string, string> headerDict)
        {
            foreach (KeyValuePair<string, string> header in headerDict)
            {
                switch (header.Key.ToLower())
                {
                    case "content-type":
                        webRequest.ContentType = header.Value;
                        break;

                    default:
                        webRequest.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }

        internal static Dictionary<string, string> KvpListToDictionary(ref List<ApiObjects.KeyValuePair> lExtraParams)
        {
            Dictionary<string, string> dResult = new Dictionary<string, string>();

            if (lExtraParams != null && lExtraParams.Count > 0)
            {
                for (int i = 0; i < lExtraParams.Count; i++)
                {
                    if (lExtraParams[i] != null && !string.IsNullOrEmpty(lExtraParams[i].key) && !string.IsNullOrEmpty(lExtraParams[i].value))
                    {
                        dResult[lExtraParams[i].key] = lExtraParams[i].value;
                    }
                }
            }

            return dResult;
        }
        internal static bool TryWriteToUserLog(string sMessage, int nGroupID, string sSiteGuid)
        {
            bool res = true;
            try
            {
                res = Core.Users.Module.WriteLog(nGroupID, sSiteGuid, sMessage, "WS_Social");
            }
            catch (Exception ex)
            {
                res = false;
                #region Logging
                StringBuilder sb = new StringBuilder("Failed to write to user log");
                sb.Append(String.Concat(" Msg: ", sMessage));
                sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                log.Error("Error - " + sb.ToString(), ex);
                #endregion
            }

            return res;
        }

        static public DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }



        static public List<StatisticsView> DecodeSearchJsonObject(string sObj, ref int totalItems)
        {
            List<StatisticsView> documents = null;
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                    if (totalItems > 0)
                    {
                        documents = jsonObj.SelectToken("hits.hits").Select(item => new StatisticsView()
                        {

                            ID = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken),
                            GroupID = ((tempToken = item.SelectToken("fields.group_id")) == null ? 0 : (int)tempToken),
                            MediaID = ((tempToken = item.SelectToken("fields.media_id")) == null ? 0 : (int)tempToken)
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch Media request. Execption={0}", ex.Message), ex);
            }

            return documents;
        }

        static public UserResponseObject Signin(Int32 nGroupID, string sSiteGuid, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject response = null;
            try
            {
                response = Core.Users.Module.AutoSignIn(nGroupID, sSiteGuid, string.Empty, sIP, deviceID, bPreventDoubleLogins);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("exception caught in social utils (AutoSignIn). ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }
            return response;
        }


        public static List<int> EnumerableStringToInt(IEnumerable<string> input)
        {
            List<int> lRes = new List<int>();

            foreach (string item in input)
            {
                int temp;
                if (int.TryParse(item, out temp))
                {
                    lRes.Add(temp);
                }
            }

            return lRes;
        }

        public static string ChangePicUrlDim(string sPicUrl, string sDimension)
        {
            string sRes = string.Empty;

            if (!string.IsNullOrEmpty(sPicUrl))
            {
                int nPicSizeIndex = sPicUrl.LastIndexOf('_');
                int nSuffix = sPicUrl.LastIndexOf('.');
                string sTempUrl = (nPicSizeIndex > -1) ? sPicUrl.Substring(0, nPicSizeIndex + 1) : string.Empty;
                string sTempSuffix = (nSuffix > -1) ? sPicUrl.Substring(nSuffix) : string.Empty;

                sRes = string.Format("{0}{1}{2}", sTempUrl, sDimension, sTempSuffix);
            }

            return sRes;
        }

        public static string CreateSocialActionId(string sSiteGuid, int nSocialPlatform, int nUserAction, int nAssetId, int nAssetType)
        {
            string id = string.Concat(sSiteGuid, "::", nSocialPlatform, "::", nUserAction, "::", nAssetId, "::", nAssetType);

            return id;
        }

        public static string CreateSocialFriendActionId(string userId, string friendId, int socialPlatform, int userAction, int assetId, int assetType)
        {
            string id = string.Concat(userId, "::", friendId, "::", socialPlatform, "::", userAction, "::", assetId, "::", assetType);

            return id;
        }

        #region create client to WCF service

        internal static class BindingFactory
        {
            internal static Binding CreateInstance()
            {
                WSHttpBinding binding = new WSHttpBinding();
                binding.Security.Mode = SecurityMode.None;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.UseDefaultWebProxy = true;
                return binding;
            }

        }
        #endregion

        internal static DataTable InitSocialPrivacySettings()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("site_guid", typeof(string));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("action_id", typeof(int));
            dt.Columns.Add("social_platform", typeof(int));            
            dt.Columns.Add("internal_share", typeof(int));
            dt.Columns.Add("external_share", typeof(int));
            dt.Columns.Add("external_privacy", typeof(int));
            return dt;
        }


    }
}
