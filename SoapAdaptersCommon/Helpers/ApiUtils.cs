using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Newtonsoft.Json.Linq;

namespace AdapaterCommon.Helpers
{
    public static class ApiUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string APPLICATION_JSON_CONTENT_TYPE = "application/json";

        public static readonly List<string> INVALID_KS_STATUS_CODES = new List<string>() { "500015", "500016" };

        public static string BuildApiUrl(string apiBaseUrl, string service, string action)
        {
            return string.Format("{0}api_v3/service/{1}/action/{2}", apiBaseUrl.EndsWith("/") ? apiBaseUrl : string.Concat(apiBaseUrl, "/"), service, action);  
        }

        public static bool ApiCall(string apiUrl, string json, out object result)
        {
            result = null;

            HttpWebResponse httpWebResponse = null;

            try
            {
                string httpResult = HttpPost(apiUrl, json, APPLICATION_JSON_CONTENT_TYPE, out httpWebResponse);
                if (ParseAndLogApiResponse(httpWebResponse, httpResult, out result))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception in API Call URL = {0}, body = {1}", apiUrl, json), ex);
                return false;
            }
            return false;
        }

        public static string StartSessionFlowWithSha1(string apiBaseUrl, int partnerId, string appTokenId, string appToken, string apiVersion)
        {
            string response = null;

            string anonymousKs = OttUser_AnonymousLogin(apiBaseUrl, partnerId, apiVersion);

            if (string.IsNullOrEmpty(anonymousKs))
            {
                log.ErrorFormat("Failed to get anonymous KS for partnerId = {0}", partnerId);
                return response;
            }

            string hashedKsWithAppToken = EncryptionUtils.HashStringWithSha1(string.Concat(anonymousKs, appToken));

            response = AppToken_StartSession(apiBaseUrl, appTokenId, hashedKsWithAppToken, anonymousKs, apiVersion);

            return response;
        }

        public static string GetErrorResponseStatusCode(object result)
        {
            JObject jsonResult = (JObject)result;
            string response = null;
            var error = jsonResult["error"];
            if (error != null)
            {
                response = (string)error["code"];
            }
            return response;
        }

        public static string HttpGet(string url)
        {
            HttpWebResponse response = null;

            try
            {
                log.DebugFormat("Get call URL: {0}", url);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "GET";
                response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                return sr.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)ex.Response;
                    log.ErrorFormat("GET call error. Error code: {0}, ex: {1}", (int)response.StatusCode, ex);
                }
                else
                    log.ErrorFormat("GET call Error. ex: {0}", ex);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return null;
        }
        public static string HttpPost(string url, string data, string contentType, out HttpWebResponse httpWebResponse)
        {
            log.DebugFormat("Post request. URL: {0}, Data: {1}", url, data);

            httpWebResponse = null;
            string result = String.Empty;

            try
            {
                // create request
                var request = (HttpWebRequest)WebRequest.Create(url);

                // prepare post data
                var postData = Encoding.ASCII.GetBytes(data);

                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = data.Length;

                // insert data to request
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                // convert to HttpWebResponse
                httpWebResponse = (HttpWebResponse)request.GetResponse();

                return new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null)
                {
                    // update status code
                    httpWebResponse = (HttpWebResponse)ex.Response;

                    // log response data
                    using (var stream = ex.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            log.ErrorFormat("Exception received: {0} ex: {1}", reader.ReadToEnd(), ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception:", ex);
            }

            return result;
        }

        private static string OttUser_AnonymousLogin(string apiBaseUrl, int partnerId, string apiVersion)
        {
            string response = null;

            string json = string.Format("{{\"partnerId\":\"{0}\", \"apiVersion\": \"{1}\"}}", partnerId, apiVersion);
            string url = BuildApiUrl(apiBaseUrl, "ottUser", "anonymousLogin");
            HttpWebResponse httpWebResponse = null;

            try
            {
                string httpResult = HttpPost(url, json, APPLICATION_JSON_CONTENT_TYPE, out httpWebResponse);

                object result = null;
                if (ParseAndLogApiResponse(httpWebResponse, httpResult, out result))
                {
                    response = (string)((JObject)result)["ks"];
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception in anoymousLogin for partnerId = {0}", partnerId), ex);
            }
            return response;
        }

        private static string AppToken_StartSession(string apiBaseUrl, string appTokenId, string tokenHash, string anonymousKs, string apiVersion)
        {
            string response = null;

            string json = string.Format("{{\"id\":\"{0}\", \"tokenHash\":\"{1}\", \"ks\":\"{2}\", \"apiVersion\": \"{3}\"}}", appTokenId, tokenHash, anonymousKs, apiVersion);
            string url = BuildApiUrl(apiBaseUrl, "appToken", "startSession");
            HttpWebResponse httpWebResponse = null;

            try
            {
                string httpResult = HttpPost(url, json, APPLICATION_JSON_CONTENT_TYPE, out httpWebResponse);
                object result;
                if (ParseAndLogApiResponse(httpWebResponse, httpResult, out result))
                {
                    response = (string)((JObject)result)["ks"];
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception in StartSession for appTokenId = {0}", appTokenId), ex);
            }
            return response;
        }

        private static bool ParseAndLogApiResponse(HttpWebResponse httpWebResponse, string httpResult, out object result)
        {
            result = null;
            if (httpWebResponse == null || string.IsNullOrEmpty(httpResult))
            {
                log.ErrorFormat("unexpected error on HTTP Post");
                return false;
            }
            if (httpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                log.ErrorFormat("received HTTP error status code on HTTP Post. HTTP status code = {0}", httpWebResponse.StatusCode);
                return false;
            }

            JObject jsonResponse = JObject.Parse(httpResult);
            result = jsonResponse["result"];

            if (result == null)
            {
                log.ErrorFormat("received response with unexpected format from API - no result property");
                return false;
            }
            if (result is JObject && ((JObject)result)["error"] != null)
            {
                log.ErrorFormat("received error from API. error = {0}", ((JObject)result)["error"]);
                return false;
            }
            
            return true;
        }

        

        
    }
}
