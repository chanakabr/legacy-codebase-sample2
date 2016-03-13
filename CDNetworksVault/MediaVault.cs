using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using keygen4Lib;
using System.Net;
using System.Web;

namespace CDNetworksVault
{
    public class MediaVault
    {
        protected string m_sPrivateKey;
        protected string m_sUser_ID;
        protected Int32 m_nTTl;
        public MediaVault(string sPrivateKey , string sUser_ID , Int32 nTTl)
        {
            m_sPrivateKey = sPrivateKey;
            m_sUser_ID = sUser_ID;
            m_nTTl = nTTl;
        }

        protected string GetServerIP()
        {
            if (CachingManager.CachingManager.Exist("___server_ip") == true)
            {
                return CachingManager.CachingManager.GetCachedData("___server_ip").ToString();
            }
            string whatIsMyIp = "https://platform-us.tvinci.com/whatismyip.aspx?onlyip=1";
            WebClient wc = new WebClient();
            string response = System.Text.UTF8Encoding.UTF8.GetString(wc.DownloadData(whatIsMyIp));
            CachingManager.CachingManager.SetCachedData("___server_ip", response, 80000, System.Web.Caching.CacheItemPriority.AboveNormal, 0, true);
            return response;
        }

        public string GetURL(string sBaseURL)
        {
            Logger.Logger.Log("Get URL", sBaseURL, "GetURL");
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            System.Uri u = new Uri(sBaseURL);
            string[] sSegments = u.Segments;
            string sPath = "";
            string sSchema = u.Scheme;
            string sHost = u.Host;
            string sFileName = "";
            for (int i = 0; i < sSegments.Length; i++)
            {
                string sSeg = sSegments[i];
                if (i < sSegments.Length - 2)
                    sPath += sSeg;
                else
                    sFileName += sSeg;
            }
            string sRtmp = sSchema + "://" + sHost + sPath;
            string sContent_url = sRtmp + sFileName;
            Logger.Logger.Log("Get URL", "Before New", "GetURL");
            Ikeygen authobj = new keygen4Lib.keygen();
            Logger.Logger.Log("Get URL", "After New", "GetURL");
            //IPHostEntry host = Dns.Resolve(Dns.GetHostName());
            //string sServer_ip = host.AddressList[0].ToString();
            string sServer_ip = GetServerIP();
            String authkey = authobj.GetCode(sContent_url, m_sUser_ID, m_nTTl.ToString(), sIP, sServer_ip, m_sPrivateKey);
            //authkey = "$filmoadmin";
            string sRet = sRtmp + sFileName + "?key=" + authkey;
            return sRet;
        }
    }
}
