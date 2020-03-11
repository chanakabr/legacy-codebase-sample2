using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Data;
using System.Linq;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;
using System.Net.Http;

namespace TVinciShared
{
    public class WS_Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient();

        public static bool IsNodeExists(ref XmlNode theItem, string sXpath)
        {
            return XmlUtils.IsNodeExists(ref theItem, sXpath);
        }

        public static string GetNodeValue(ref XmlNode theItem, string sXpath)
        {
            return XmlUtils.GetNodeValue(ref theItem, sXpath);
        }

        public static string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            return XmlUtils.GetItemParameterVal(ref theNode, sParameterName);
        }

        public static string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
        {
            return XmlUtils.GetNodeParameterVal(ref theNode, sXpath, sParameterName);
        }

        //public static string GetIP2CountryCode(string sIP)
        //{
        //    if (string.IsNullOrEmpty(sIP))
        //    {
        //        return "Israel";
        //    }

        //    string retVal = string.Empty;
        //    string[] splited = sIP.Split('.');

        //    long nIPVal = Int64.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
        //    retVal = DAL.UtilsDal.GetIP2CountryCode(nIPVal);
        //    return retVal;
        //}

        public static Int32 GetGroupID(string sWSName, string sModuleName, string sUN, string sPass, string sIP)
        {
            try
            {
                Int32 nGroupID = DAL.UtilsDal.GetGroupID(sUN, sPass, sModuleName, sIP, sWSName);
                return nGroupID;
            }
            catch (Exception ex)
            {
                log.Error("exception - " + ex.StackTrace + " sWSName: " + sWSName + " | sModuleName: " + sModuleName + " | sUN: " + sUN + " | sPass: " + sPass + " | sIP: " + sIP);
            }
            return 0;
        }

        public static Int32 GetGroupID(string sWSName, string sUN, string sPass)
        {
            try
            {
                Int32 nGroupID = DAL.UtilsDal.GetGroupID(sUN, sPass, sWSName);
                return nGroupID;
            }
            catch (Exception ex)
            {
                log.Error("exception - " + ex.StackTrace + " sWSName: " + sWSName + " | sUN: " + sUN + " | sPass: " + sPass);
            }
            return 0;
        }

        public static string GetSecretCode(string sWSName, string sModuleName, string sUN, ref Int32 nGroupID)
        {
            string sSecret = DAL.UtilsDal.GetSecretCode(sWSName, sModuleName, sUN, ref nGroupID);
            return sSecret;
        }

        public static bool GetWSUNPass(Int32 nGroupID, string sWSFunctionName, string sWSName, string sIP, ref string sWSUN, ref string sWSPassword)
        {
            sWSUN = string.Empty;
            sWSPassword = string.Empty;

            bool res = DAL.UtilsDal.GetWSUNPass(nGroupID, sIP, sWSFunctionName, sWSName, ref sWSUN, ref sWSPassword);
            return res;
        }

        public static bool GetWSCredentials(Int32 nGroupID, string sWSName, ref string sWSUN, ref string sWSPassword)
        {
            sWSUN = string.Empty;
            sWSPassword = string.Empty;

            bool res = DAL.UtilsDal.GetWSCredentials(nGroupID, sWSName, ref sWSUN, ref sWSPassword);
            return res;
        }

        public static bool GetAllWSCredentials(string sIP, ref DataTable modules)
        {
            bool res = DAL.UtilsDal.GetAllWSCredentials(sIP, ref modules);
            return res;
        }

        public static int GetModuleImplID(int nGroupID, int nModuleID, string connectionKey)
        {
            return DAL.UtilsDal.GetModuleImplID(nGroupID, nModuleID, connectionKey);
        }

        public static string GetModuleImplName(int nGroupID, int nModuleID, string connectionKey, int operatorId = -1)
        {
            return DAL.UtilsDal.GetModuleImplName(nGroupID, nModuleID, connectionKey, operatorId);
        }

        public static string SendXMLHttpReq(string url, string requestBody, string sSoapHeader, string contentType = "text/xml; charset=utf-8",
                                            string sUsernameField = "", string sUsername = "", string sPasswordField = "", string sPassword = "", string sMethod = "post")
        {
            HttpMethod method = GetHttpMethod(sMethod);

            HttpRequestMessage request = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(requestBody, Encoding.UTF8,
                (string.IsNullOrEmpty(contentType) ? "text/xml; charset=utf-8" : contentType))
            };

            if (!string.IsNullOrEmpty(sUsernameField) || !string.IsNullOrEmpty(sPasswordField) || !string.IsNullOrEmpty(sUsername) || !string.IsNullOrEmpty(sPassword))
            {
                request.Headers.Add(sUsernameField, sUsername);
                request.Headers.Add(sPasswordField, sPassword);
            }

            try
            {
                using (var response = httpClient.SendAsync(request).ExecuteAndWait())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        log.Error($"XML Http request not successful. url = {url}, status = {response.StatusCode}");
                    }

                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error when sending XML http request. url = {url} ex={ex}");
            }

            return string.Empty;
        }

        private static HttpMethod GetHttpMethod(string sMethod)
        {
            HttpMethod method = HttpMethod.Post;

            if (!string.IsNullOrEmpty(sMethod))
            {
                string loweredMethod = sMethod.ToLower();

                if (loweredMethod == "post")
                {
                    method = HttpMethod.Delete;
                }
                else if (loweredMethod == "get")
                {
                    method = HttpMethod.Get;
                }
                else if (loweredMethod == "put")
                {
                    method = HttpMethod.Put;
                }
                else if (loweredMethod == "delete")
                {
                    method = HttpMethod.Delete;
                }
            }

            return method;
        }

        public static bool TrySendHttpPostRequest(string sUrl, string sToSend, string sContentType,
            Encoding encoding, ref string sResult, ref string sErrorMsg)
        {
            bool res = true;
            WebRequest webRequest = null;
            Stream dataStream = null;
            WebResponse response = null;
            StreamReader reader = null;
            try
            {

                // build request
                webRequest = WebRequest.Create(sUrl);
                webRequest.Method = "POST";
                webRequest.ContentType = sContentType;
                byte[] dataToSend = encoding.GetBytes(sToSend);
                webRequest.ContentLength = dataToSend.Length;

                // send request
                dataStream = webRequest.GetRequestStream();
                dataStream.Write(dataToSend, 0, dataToSend.Length);
                dataStream.Close();
                dataStream = null;

                // handle response
                response = (WebResponse)webRequest.GetResponse();
                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                sResult = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                res = false;
                sErrorMsg = ex.Message;
            }
            finally
            {
                #region Disposing
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (dataStream != null)
                {
                    dataStream.Close();
                    dataStream = null;
                }
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                #endregion
            }

            return res;
        }


        /*
         * Before you change anything in this method, keep in mind it is used in Cinepolis billing process. Good luck :)
         * 
         */
        public static string BuildDelimiterSeperatedString(List<KeyValuePair<string, string>> lst, string sDelimiter, bool bIsPutDelimiterAtStart, bool bIsPutDelimiterAtEnd)
        {
            StringBuilder sb = new StringBuilder();
            bool bAppendDelimiter = bIsPutDelimiterAtStart;
            if (lst != null && lst.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in lst)
                {
                    if (bAppendDelimiter)
                        sb.Append(sDelimiter);
                    sb.Append(String.Concat(kvp.Key, "=", kvp.Value));
                    bAppendDelimiter = true;
                }
                if (bIsPutDelimiterAtEnd)
                    sb.Append(sDelimiter);
            }

            return sb.ToString();
        }

        public static Dictionary<string, string> TryParseJSONToDictionary(string sJSON, List<string> lstOfKeyNamesTrimmedLowercase, ref bool bIsParsingSuccessful, ref string sErrorMsg)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            StringBuilder sbErrorMsg = new StringBuilder();
            JsonTextReader jtr = null;
            bIsParsingSuccessful = true;
            try
            {
                bool bParseThisIteration = false;
                jtr = new JsonTextReader(new StringReader(sJSON));
                string sKey = string.Empty;
                while (jtr.Read())
                {
                    if (jtr.Value != null)
                    {
                        string sCurrent = jtr.Value.ToString().Trim().ToLower();
                        if (lstOfKeyNamesTrimmedLowercase.Contains(sCurrent))
                        {
                            bParseThisIteration = true;
                            sKey = sCurrent;
                            continue;
                        }
                    }
                    if (bParseThisIteration)
                    {
                        if (jtr.Value != null && sKey.Length > 0)
                        {
                            res.Add(sKey, jtr.Value.ToString().Trim().ToLower());
                        }
                        else
                        {
                            bIsParsingSuccessful = false;
                            sbErrorMsg.Append(String.Concat("Error occurred trying parsing value for key: ", sKey, "|"));
                        }
                        sKey = string.Empty;
                        bParseThisIteration = false;
                    }
                } // end while
            }
            catch (Exception ex)
            {
                bIsParsingSuccessful = false;
                sbErrorMsg.Append(String.Concat(ex.Message, "|"));
                log.Error(sbErrorMsg.ToString(), ex);
            }
            finally
            {
                #region Disposing
                if (jtr != null)
                {
                    jtr.Close();
                    jtr = null;
                }
                #endregion
            }
            if (!bIsParsingSuccessful)
                sErrorMsg = sbErrorMsg.ToString();

            return res;
        }

        public static string GetCatalogSignature(string signString, string SignatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = SignatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            using (HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret)))
            {
                retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            }
            return retVal;
        }

        public static string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
                if (string.IsNullOrEmpty(result))
                {
                    log.Debug($"GetTcmConfigValue - missing key {sKey} or empty result");
                }
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static bool GetTcmBoolValue(string sKey)
        {
            bool result = false;
            try
            {

                result = TCMClient.Settings.Instance.GetValue<bool>(sKey);
            }
            catch (Exception ex)
            {
                result = false;
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static int GetTcmIntValue(string sKey)
        {
            int result = 0;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<int>(sKey);
            }
            catch (Exception ex)
            {
                result = 0;
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static double GetTcmDoubleValue(string sKey)
        {
            double result = 0;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<double>(sKey);
            }
            catch (Exception ex)
            {
                result = 0;
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static DateTime GetTcmDateTimeValue(string sKey)
        {
            DateTime result = DateTime.UtcNow;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<DateTime>(sKey);
                if (result == null)
                    log.Debug($"GetTcmConfigValue - missing key {sKey} or empty result");
            }
            catch (Exception ex)
            {
                result = DateTime.UtcNow; ;
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static T GetTcmGenericValue<T>(string sKey)
        {
            T result = default(T);
            try
            {
                result = TCMClient.Settings.Instance.GetValue<T>(sKey);
                if (result == null)
                    log.Debug($"GetTcmConfigValue - missing key {sKey} or empty result");
            }
            catch (Exception ex)
            {
                result = default(T);
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static void InitTcmConfig()
        {
            try
            {
                ApplicationConfiguration.Initialize(true);
            }
            catch (Exception ex)
            {
                log.Error("TvinciShared.Ws_Utils - Init=" + ex.Message, ex);
            }
        }

        public static bool IsGroupIDContainedInConfig(long lGroupID, string rawStrFromConfig, char cSeperator)
        {
            bool res = false;
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(cSeperator);
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(lGroupID);
                }
            }

            return res;
        }

        /// <summary>
        /// This function gets the number of items in a collection, a page index and a range. It checks whether the range of items is valid among the
        /// the totlal list number. 
        /// </summary>
        /// <param name="nNumOfMedias">Number of items in a collection</param>
        /// <param name="nPageIndex">The requested page index</param>
        /// <param name="nValidRange">The range of items request within a page index (updated if required)</param>
        /// <returns>True if valid, false if not valid</returns>
        public static bool ValidatePageSizeAndPageIndexAgainstToltalList(int nNumOfMedias, int nPageIndex, ref int nValidRange)
        {
            bool bIsValidRange = false;
            if (nValidRange > 0 || nPageIndex > 0)
            {
                int nSizePageIndexMultiplicity = nPageIndex * nValidRange;
                if (nSizePageIndexMultiplicity < nNumOfMedias)
                {
                    if (nNumOfMedias - nSizePageIndexMultiplicity < nValidRange)
                    {
                        nValidRange = nNumOfMedias - nSizePageIndexMultiplicity;
                    }

                    bIsValidRange = true;
                }
            }
            else if (nValidRange == 0 && nPageIndex == 0)   // Returning all items in collection
            {
                bIsValidRange = true;
            }

            return bIsValidRange;
        }
    }
}
