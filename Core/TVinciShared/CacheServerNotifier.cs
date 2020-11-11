using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Xml;
using KLogMonitor;

namespace TVinciShared
{
    public class CacheServerNotifier : Notifier
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected Int32 m_nGroupID;
        protected string m_sProtocol;
        protected string m_sRequest;
        protected string m_sResponse;
        public CacheServerNotifier(Int32 nGroupID, string sProtocol, string sRequest, string sResponse)
            : base("", "")
        {
            m_nGroupID = nGroupID;
            m_sProtocol = sProtocol;
            m_sRequest = sRequest;
            m_sResponse = sResponse;
        }

        static protected string RemoveUnWantedParameters(string sXML, string sTag)
        {
            try
            {
                XmlDocument theDoc = new XmlDocument();
                theDoc.LoadXml(sXML);
                if (theDoc.SelectSingleNode("root/flashvars/@site_guid") != null)
                {
                    XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["site_guid"];
                    attr.OwnerElement.RemoveAttribute("site_guid");
                }
                if (theDoc.SelectSingleNode("root/flashvars/@tvinci_guid") != null)
                {
                    XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["tvinci_guid"];
                    attr.OwnerElement.RemoveAttribute("tvinci_guid");
                }
                if (theDoc.SelectSingleNode("root/flashvars/@no_cache") != null)
                {
                    XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["no_cache"];
                    attr.OwnerElement.RemoveAttribute("no_cache");
                }
                if (theDoc.SelectSingleNode("root/flashvars/@alt_tvm") != null)
                {
                    XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["alt_tvm"];
                    attr.OwnerElement.RemoveAttribute("alt_tvm");
                }
                if (theDoc.SelectSingleNode("root/flashvars/@debug") != null)
                {
                    XmlAttribute attr = theDoc.SelectSingleNode("root/flashvars").Attributes["debug"];
                    attr.OwnerElement.RemoveAttribute("debug");
                }
                XmlNode theRequest = theDoc.SelectSingleNode(sTag);
                string sNewXML = ProtocolsFuncs.ConvertXMLToString(ref theRequest, false);
                return sNewXML;
            }
            catch (Exception ex)
            {
                log.Error("exception - " + sXML, ex);
                return sXML;
            }
        }

        public override void Notify()
        {
            m_sURL = GetGroupCachingServerListener(m_nGroupID);
            if (m_sURL == "")
                return;
            bool bIsNeedToBeCached = IsProtocolNeedToBeCached(m_sProtocol);
            if (bIsNeedToBeCached == false)
                return;
            //need to take off the no_cache and the site_guid
            string sReq = RemoveUnWantedParameters(m_sRequest, "/root");
            if (CachingManager.CachingManager.Exists(sReq) == true && CachingManager.CachingManager.GetCachedData(sReq).ToString() == "---")
                return;
            CachingManager.CachingManager.SetCachedData(sReq, "---", 10800, CacheItemPriority.Default, 0, false);
            string sCachingSeverXML = "<root><req>" + sReq + "</req>";
            sCachingSeverXML += "<res>" + RemoveUnWantedParameters(m_sResponse, "/") + "</res></root>";
            m_sXML = sCachingSeverXML;
            base.Notify();
        }

        static protected string GetGroupCachingServerListener(Int32 nGroupID)
        {
            string sCachingServerURL = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(3600);
            selectQuery += "select CACHING_SERVER_URL from groups (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object o = selectQuery.Table("query").DefaultView[0].Row["CACHING_SERVER_URL"];
                    if (o != null && o != DBNull.Value)
                    {
                        sCachingServerURL = o.ToString();
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sCachingServerURL;
        }

        static protected bool IsProtocolNeedToBeCached(string sProtocolName)
        {
            Int32 nCached = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery.SetCachedSec(3600);
            selectQuery += "select IS_CACHED from cached_protocols (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PROTOCOL_NAME", "=", sProtocolName);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCached = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_CACHED"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCached == 0)
                return false;
            return true;
        }
    }
}
