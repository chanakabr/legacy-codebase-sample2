using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace TVinciShared
{
    public class Notifier
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected string m_sURL;
        protected string m_sXML;
        public Notifier(string sURL, string sXML)
        {
            m_sURL = sURL;
            m_sXML = sXML;
        }

        static public void ClearServersCache(Int32 nGroupID)
        {

            HttpContext.Current.Session["tvp_cache_error"] = "";
            Int32 nStatus = 0;
            string sURLs = "";
            object oCachecleanURL = null;
            log.Debug("Clear Cache String - Start Staging Clear Cache");
            if (!string.IsNullOrEmpty(ApplicationConfiguration.StagingClearCachePath.Value))
            {
                sURLs = ApplicationConfiguration.StagingClearCachePath.Value;
            }
            else
            {
                oCachecleanURL = ODBCWrapper.Utils.GetTableSingleVal("groups", "CACHING_SERVER_URL", nGroupID);
                if (oCachecleanURL != null && oCachecleanURL != DBNull.Value)
                {
                    sURLs = oCachecleanURL.ToString();
                }
            }
            log.Debug("Clear Cache String - Start Staging Clear Cache - strng is " + sURLs);
            if (sURLs != "")
            {
                //sURLs = oCachecleanURL.ToString();
                string[] sSep = { ";" };
                string[] sUrlsArray = sURLs.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
                HttpContext.Current.Session["tvp_cache_error"] = "";
                for (int i = 0; i < sUrlsArray.Length; i++)
                {
                    string sResp = TVinciShared.Notifier.SendGetHttpReq(sUrlsArray[i], ref nStatus);
                    HttpContext.Current.Session["tvp_cache_error"] += "Url: " + sUrlsArray[i] + " Status: " + nStatus.ToString() + "<br/>";
                }
                if (sUrlsArray.Length <= 0)
                    HttpContext.Current.Session["tvp_cache_error"] += "No Cache URLs defined for TVP <br/>";
            }
            else
            {
                HttpContext.Current.Session["tvp_cache_error"] += "No Cache URLs defined for TVP <br/>";
            }


            string sTvinciServersCache = ApplicationConfiguration.ClearCachePath.Value;

            if (!string.IsNullOrEmpty(sTvinciServersCache))
            {
                string[] sSep = { ";" };
                string[] sUrlsArray = sTvinciServersCache.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < sUrlsArray.Length; i++)
                {
                    string sResp = TVinciShared.Notifier.SendGetHttpReq(sUrlsArray[i], ref nStatus);
                    HttpContext.Current.Session["tvp_cache_error"] += "Url: " + sUrlsArray[i] + " Status: " + nStatus.ToString() + "<br/>";
                }
                if (sUrlsArray.Length <= 0)
                    HttpContext.Current.Session["tvp_cache_error"] += "No Cache URLs defined for Tvinci servers <br/>";
            }
            else
                HttpContext.Current.Session["tvp_cache_error"] += "No Cache URLs defined for Tvinci servers <br/>";
        }

        static protected Int32 GetResponseCode(HttpStatusCode theCode)
        {
            if (theCode == HttpStatusCode.OK)
                return 200;
            if (theCode == HttpStatusCode.NotFound)
                return 404;
            return 500;

        }

        static public string SendXMLHttpReq(string sUrl, string sToSend, ref Int32 nStatus)
        {
            //Create the HTTP POST request and the authentication headers
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
            oWebRequest.Method = "post";
            oWebRequest.ContentType = "text/xml";
            byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
            oWebRequest.AllowWriteStreamBuffering = true;

            //Send the request.
            Stream requestStream = oWebRequest.GetRequestStream();
            requestStream.Write(encodedBytes, 0, encodedBytes.Length);
            requestStream.Close();

            Int32 nStatusCode = -1;
            //Handle the response.
            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                Stream receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream);
                string resultString = sr.ReadToEnd();

                sr.Close();
                oWebRequest = null;
                oWebResponse = null;
                nStatus = nStatusCode;
                return resultString;
            }
            catch (Exception ex)
            {
                log.Error("Notifier - SendXMLHttpReq exception:" + ex.Message + " to: " + sUrl + " contnet: " + sToSend, ex);
                nStatusCode = 404;
                return "";
            }
        }

        static public string SendGetHttpReq(string sUrl, ref Int32 nStatus)
        {
            return SendGetHttpReq(sUrl, ref nStatus, "", "");
        }

        static public string SendGetHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword)
        {
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            HttpWebResponse oWebResponse = null;
            Stream receiveStream = null;
            Int32 nStatusCode = -1;
            try
            {
                oWebRequest.Credentials = new NetworkCredential(sUserName, sPassword);
                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                string resultString = "";
                if (oWebResponse.ContentLength < 10000000 && oWebResponse.ContentLength > 0)
                {
                    HttpStatusCode sCode = oWebResponse.StatusCode;
                    nStatusCode = GetResponseCode(sCode);
                    receiveStream = oWebResponse.GetResponseStream();

                    StreamReader sr = new StreamReader(receiveStream);
                    resultString = sr.ReadToEnd();
                    oWebResponse.Close();
                    sr.Close();
                }
                else
                {
                    oWebResponse.Close();
                }

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
                return "";
            }
        }


        virtual public void Notify()
        {
            Int32 nStatus = 0;
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    SendXMLHttpReq(m_sURL, m_sXML, ref nStatus);
                    if (nStatus == 200)
                        break;
                    Int32 nToSleep = 1000;
                    if (i == 1)
                        nToSleep = 5000;
                    if (i == 2)
                        nToSleep = 15000;
                    if (i == 3)
                        nToSleep = 60000;
                    /*
                    if (i == 4)
                        nToSleep = 60000 * 5;
                    if (i == 5)
                        nToSleep = 60000 * 10;
                    if (i == 6)
                        nToSleep = 60000 * 60;
                    if (i == 7)
                        nToSleep = 60000 * 60 * 2;
                    if (i == 8)
                        nToSleep = 60000 * 60 * 3;
                    if (i == 9)
                        nToSleep = 60000 * 60 * 5;
                    */
                    System.Threading.Thread.Sleep(nToSleep);
                }
                catch (Exception ex)
                {
                    log.Error("Notifier - Notify exception:" + ex.Message + " to: " + m_sURL, ex);
                }
            }
        }

        public void NotifyGet()
        {
            Int32 nStatus = 0;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    SendGetHttpReq(m_sURL, ref nStatus);
                    log.Debug("NotifierGet - message sent recieved stats: " + nStatus.ToString());
                    if (nStatus == 200)
                        break;
                    Int32 nToSleep = 1000;
                    if (i == 1)
                        nToSleep = 5000;
                    if (i == 2)
                        nToSleep = 15000;
                    if (i == 3)
                        nToSleep = 60000;
                    if (i == 4)
                        nToSleep = 60000 * 5;
                    if (i == 5)
                        nToSleep = 60000 * 10;
                    if (i == 6)
                        nToSleep = 60000 * 60;
                    if (i == 7)
                        nToSleep = 60000 * 60 * 2;
                    if (i == 8)
                        nToSleep = 60000 * 60 * 3;
                    if (i == 9)
                        nToSleep = 60000 * 60 * 5;
                    System.Threading.Thread.Sleep(nToSleep);
                }
                catch (Exception ex)
                {
                    log.Error("Notifier - NotifyGet exception:" + ex.Message + " to: " + m_sURL, ex);
                }
            }
        }
    }
}
