using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Data;

namespace TVinciShared
{
    public class WS_Utils
    {

        static public bool IsNodeExists(ref XmlNode theItem, string sXpath)
        {
            return XmlUtils.IsNodeExists(ref theItem, sXpath);
        }

        static public string GetNodeValue(ref XmlNode theItem, string sXpath)
        {
            return XmlUtils.GetNodeValue(ref theItem, sXpath);
        }

        static public string GetItemParameterVal(ref XmlNode theNode, string sParameterName)
        {
            return XmlUtils.GetItemParameterVal(ref theNode, sParameterName);
        }

        static public string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
        {
            return XmlUtils.GetNodeParameterVal(ref theNode, sXpath, sParameterName);
        }

        static public string GetIP2CountryCode(string sIP)
        {
            if (string.IsNullOrEmpty(sIP))
            {
                return "Israel";
            }

            string retVal = string.Empty;
            string[] splited = sIP.Split('.');

            long nIPVal = Int64.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
            retVal = DAL.UtilsDal.GetIP2CountryCode(nIPVal);
            return retVal;
        }

        static public Int32 GetGroupID(string sWSName , string sModuleName, string sUN, string sPass, string sIP)
        {
            try
            {
                Int32 nGroupID = DAL.UtilsDal.GetGroupID(sUN, sPass, sModuleName, sIP, sWSName); 
                return nGroupID;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("exception", ex.StackTrace + " sWSName: " + sWSName + " | sModuleName: " + sModuleName + " | sUN: " + sUN + " | sPass: " + sPass + " | sIP: " + sIP, "ws_utils");
            }
            return 0;
        }

        static public Int32 GetGroupID(string sWSName, string sUN, string sPass)
        {
            try
            {
                Int32 nGroupID = DAL.UtilsDal.GetGroupID(sUN, sPass, sWSName);
                return nGroupID;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("exception", ex.StackTrace + " sWSName: " + sWSName + " | sUN: " + sUN + " | sPass: " + sPass, "ws_utils");
            }
            return 0;
        }

        static public string GetSecretCode(string sWSName, string sModuleName, string sUN , ref Int32 nGroupID)
        {
            string sSecret = DAL.UtilsDal.GetSecretCode(sWSName, sModuleName, sUN, ref nGroupID);
            return sSecret;
        }

        static public bool GetWSUNPass(Int32 nGroupID, string sWSFunctionName , string sWSName , string sIP , ref string sWSUN, ref string sWSPassword)
        {
            sWSUN = string.Empty;
            sWSPassword = string.Empty;

            bool res = DAL.UtilsDal.GetWSUNPass(nGroupID, sIP, sWSFunctionName, sWSName, ref sWSUN, ref sWSPassword);
            return res;
        }

        static public bool GetWSCredentials(Int32 nGroupID, string sWSName, ref string sWSUN, ref string sWSPassword)
        {
            sWSUN = string.Empty;
            sWSPassword = string.Empty;

            bool res = DAL.UtilsDal.GetWSCredentials(nGroupID, sWSName, ref sWSUN, ref sWSPassword);
            return res;
        }

        static public bool GetAllWSCredentials(string sIP, ref DataTable modules)
        {
            bool res = DAL.UtilsDal.GetAllWSCredentials(sIP, ref modules);
            return res;
        }

        static public int GetModuleImplID(int nGroupID, int nModuleID)
        {
            return DAL.UtilsDal.GetModuleImplID(nGroupID, nModuleID);
        }

        static public string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader, string contentType = "text/xml; charset=utf-8",
                                            string sUsernameField = "", string sUsername = "", string sPasswordField = "", string sPassword = "", string sMethod = "post")
        {

            //Create the HTTP POST request and the authentication headers
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
            oWebRequest.Method = (string.IsNullOrEmpty(sMethod) ? "post" : sMethod);
            oWebRequest.ContentType = (string.IsNullOrEmpty(contentType) ? "text/xml; charset=utf-8" : contentType);
            //oWebRequest.Headers["SOAPAction"] = sSoapHeader;

            if (!string.IsNullOrEmpty(sUsernameField) || !string.IsNullOrEmpty(sPasswordField) || !string.IsNullOrEmpty(sUsername) || !string.IsNullOrEmpty(sPassword))
            {
                oWebRequest.Headers[sUsernameField] = sUsername;
                oWebRequest.Headers[sPasswordField] = sPassword;
            }

            byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
            //oWebRequest.ContentLength = encodedBytes.Length;
            //oWebRequest.AllowWriteStreamBuffering = true;

            //Send the request
            if (string.Compare(oWebRequest.Method, "post", true) == 0)
            {
                using (Stream requestStream = oWebRequest.GetRequestStream())
                {
                    requestStream.Write(encodedBytes, 0, encodedBytes.Length);
                    requestStream.Close();
                }
            }

            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                using (StreamReader sr = new StreamReader(receiveStream))
                {
                    string resultString = sr.ReadToEnd();

                    sr.Close();

                    oWebRequest = null;
                    oWebResponse = null;

                    return resultString;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                WebResponse errRsp = ex.Response;

                if (errRsp == null)
                {
                    return string.Empty;
                }

                using (StreamReader rdr = new StreamReader(errRsp.GetResponseStream()))
                {
                    return rdr.ReadToEnd();
                }
            }
        }

        static public string SendXMLHttpReqWithHeaders(string sUrl, string sToSend, Dictionary<string,string> postHeaders, string contentType = "text/xml; charset=utf-8",
                                            string sUsernameField = "", string sUsername = "", string sPasswordField = "", string sPassword = "", string sMethod = "post")
        {

            //Create the HTTP POST request and the authentication headers
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
            oWebRequest.Method = (string.IsNullOrEmpty(sMethod) ? "post" : sMethod);
            oWebRequest.ContentType = (string.IsNullOrEmpty(contentType) ? "text/xml; charset=utf-8" : contentType);

            foreach (string header in postHeaders.Keys)
            {
                oWebRequest.Headers[header] = postHeaders[header];
            }

            if (!string.IsNullOrEmpty(sUsernameField) || !string.IsNullOrEmpty(sPasswordField) || !string.IsNullOrEmpty(sUsername) || !string.IsNullOrEmpty(sPassword))
            {
                oWebRequest.Headers[sUsernameField] = sUsername;
                oWebRequest.Headers[sPasswordField] = sPassword;
            }

            byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);

            //Send the request
            if (string.Compare(oWebRequest.Method, "post", true) == 0)
            {
                using (Stream requestStream = oWebRequest.GetRequestStream())
                {
                    requestStream.Write(encodedBytes, 0, encodedBytes.Length);
                    requestStream.Close();
                }
            }

            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                using (StreamReader sr = new StreamReader(receiveStream))
                {
                    string resultString = sr.ReadToEnd();

                    sr.Close();

                    oWebRequest = null;
                    oWebResponse = null;

                    return resultString;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                WebResponse errRsp = ex.Response;

                if (errRsp == null)
                {
                    return string.Empty;
                }

                using (StreamReader rdr = new StreamReader(errRsp.GetResponseStream()))
                {
                    return rdr.ReadToEnd();
                }
            }
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
                if (result == null)
                    throw new NullReferenceException("missing key");
            }
            catch (Exception ex)
            {
                result = string.Empty;
                Logger.Logger.Log("TvinciShared.Ws_Utils", "Key=" + sKey + "," + ex.Message , "Tcm");
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
                Logger.Logger.Log("TvinciShared.Ws_Utils", "Key=" + sKey + "," + ex.Message, "Tcm");
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
                Logger.Logger.Log("TvinciShared.Ws_Utils", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }

        public static DateTime GetTcmDateTimeValue(string sKey)
        {
            DateTime result = DateTime.UtcNow;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<DateTime>(sKey);
            }
            catch (Exception ex)
            {
                result = DateTime.UtcNow; ;
                Logger.Logger.Log("TvinciShared.Ws_Utils", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }

        public static T GetTcmGenericValue<T>(string sKey)
        {
            T result = default(T);
            try
            {
                result = TCMClient.Settings.Instance.GetValue<T>(sKey);
            }
            catch (Exception ex)
            {
                result = default(T);
                Logger.Logger.Log("TvinciShared.Ws_Utils", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }

        public static void InitTcmConfig()
        {
            try
            {
                TCMClient.Settings.Instance.Init();
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("TvinciShared.Ws_Utils", "Init=" + ex.Message, "Tcm");
            }

        }


    }
}
