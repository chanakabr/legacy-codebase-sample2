using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;
using System.Configuration;
using System.IO;
using KLogMonitor;
using System.Reflection;
using CachingProvider.LayeredCache;

namespace APILogic
{
    public class MaxMind
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /********************************************************************************************
        * Use Http Web Service of "MaxMind" to find out oabout safe/risk Proxy
        * This method get ProxyRule(1=Enabled , 2= Disable), ProxyLevel  of risk (1=Medium, 2 = High)  
        * And return true = proxy is reliable or false = proxy in risk (by the level)
        ********************************************************************************************/
        public static bool IsProxyAllowed(int nProxyRule, double dProxyLevel, string sIP, int nGroupID)
        {
            bool bAllowed = true;

            string sProxyResponse = string.Empty;

            if (nProxyRule == 1) //Enable - need to check the proxy Nature
            {
                //call maxMind service
                string liceneKey = GetWSURL("LicenseKeyMaxMind");//default liceneKey
                //Get Licene Key from DB by group
                DataTable dt = DAL.ApiDAL.Get_LicenseKeyMaxMind(nGroupID);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        liceneKey = dt.Rows[0]["LicenseKeyMaxMind"].ToString();
                    }
                }
                string url = GetWSURL("URLMaxMind");

                log.DebugFormat("IsProxyAllowed rule:{0}, level:{1}, ip:{2}, groupId:{3}, liceneKey:{4}, url:{5}", nProxyRule, dProxyLevel, sIP, nGroupID, liceneKey, url);

                if (!string.IsNullOrEmpty(liceneKey) && !string.IsNullOrEmpty(url))
                {
                    url = url.Replace("LicenseKey", liceneKey).Replace("ipAddress", sIP);
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Method = "GET";
                    req.Referer = string.Empty;
                    try
                    {
                        HttpWebResponse objResponse = (HttpWebResponse)req.GetResponse();
                        using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                        {
                            sProxyResponse = sr.ReadToEnd();
                            sr.Close();
                        }
                        if (!string.IsNullOrEmpty(sProxyResponse))
                        {
                            log.DebugFormat("IsProxyAllowed ip:{0}, response:{1}", sIP, sProxyResponse);

                            //proxyScore= 0.00 (safe) / 1 or 2 (meduim risk) / 3 (high risk) 
                            string[] aProxyResponse = sProxyResponse.Split('=');
                            double dProxyResponseLevel = -1.0;
                            //if proxy in risk , by the proxyLevel then bAllowed = false , else do nothing !!!!!
                            if (aProxyResponse != null && aProxyResponse.Count() > 0)
                            {
                                try
                                {
                                    dProxyResponseLevel = APILogic.Utils.GetDoubleSafeVal(aProxyResponse[1]);
                                    if (dProxyResponseLevel >= dProxyLevel) // the proxy risk value from the webService is equal / greater than the one in the DB
                                        bAllowed = false;
                                }
                                catch
                                {
                                    bAllowed = true;
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse resp = (ex.Response as HttpWebResponse);
                        if (resp != null)
                        {
                            log.Error("Geo Blocks - MaxMind.IsProxyAllowed : Call to MaxMind failed . exception :" + ex.Message, ex);
                        }
                        bAllowed = true;
                    }
                }

                
            }
            return bAllowed;
        }

        public static bool IsProxyAllowed(int shouldCheckProxy, int groupId, string ip)
        {
            bool isProxyAllowed = true;
            if (shouldCheckProxy == 0)
            {
                return isProxyAllowed;
            }
            
            try
            {
                string key = LayeredCacheKeys.GetProxyIpKey(ip);
                if (!LayeredCache.Instance.Get<bool>(key, ref isProxyAllowed, APILogic.Utils.IsProxyAllowedForIp, new Dictionary<string, object>() { { "ip", ip } }, groupId,
                                                    LayeredCacheConfigNames.IS_PROXY_ALLOWED_FOR_IP_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheConfigNames.GET_PROXY_IP_INVALIDATION_KEY }))
                {
                    log.ErrorFormat("Failed checking IsProxyAllowed from LayeredCache, ip: {0}, key: {1}", ip, key);
                    /* ********************* TODO: Ask Ira what to do when failing layered cache / exception *************************
                     * should return that proxy is allowed or not?
                    */
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsProxyAllowed for ip: {0}", ip), ex);
            }

            return isProxyAllowed;
        }

        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

    }
}
