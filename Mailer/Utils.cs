using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using KLogMonitor;
using System.Reflection;

namespace Mailer
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object lck = new object();
        static public string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader)
        {
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

            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream);
                string resultString = sr.ReadToEnd();

                sr.Close();
                oWebRequest = null;
                oWebResponse = null;
                return resultString;
            }
            catch (WebException ex)
            {
                log.Error(string.Empty, ex);
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
