using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.IO;
using KLogMonitor;
using System.Reflection;
using TVinciShared;

namespace Mailer
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient(null, false);
        private static object lck = new object();

        public static string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader)
        {

            string responseFromServer = null;
            try
            {
                var contentType = "text/xml; charset=utf-8";
                using (var postData = new StringContent(sToSend, Encoding.UTF8, contentType))
                using (var response = httpClient.PostAsync(sUrl, postData).ExecuteAndWait())
                {
                    response.EnsureSuccessStatusCode();
                    responseFromServer = response.Content.ReadAsStringAsync().ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                log.Error($"SendXMLHttpReq > Error issuing POST:{sUrl}, data:{sToSend}", ex);
            }

            return responseFromServer;
        }

        public static string SendPostHttpReq(string url, string body)
        {
            string responseFromServer = null;
            try
            {
                using (var postData = new StringContent(body, Encoding.UTF8, "application/json"))
                using (var response = httpClient.PostAsync(url, postData).ExecuteAndWait())
                {
                    response.EnsureSuccessStatusCode();
                    responseFromServer = response.Content.ReadAsStringAsync().ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                log.Error($"SendPostHttpReq > Error issuing POST:{url}, data:{body}", ex);
            }

            return responseFromServer;
        }

        public static string GetGroupMcKey(int groupId)
        {
            string mcKey = string.Empty;

            string key = string.Format("mcKeyGroup_{0}", groupId);

            if (string.IsNullOrEmpty(mcKey = TvinciCache.WSCache.Instance.Get<string>(key)))
            {
                lock (lck)
                {
                    if (string.IsNullOrEmpty(mcKey = TvinciCache.WSCache.Instance.Get<string>(key)))
                    {
                        mcKey = ODBCWrapper.Utils.GetSafeStr(ODBCWrapper.Utils.GetTableSingleVal("groups", "mail_settings", groupId, 86400, "MAIN_CONNECTION_STRING"));

                        if (!string.IsNullOrEmpty(mcKey))
                            TvinciCache.WSCache.Instance.Add(key, mcKey);
                    }
                }
            }

            return mcKey;
        }
    }
}
