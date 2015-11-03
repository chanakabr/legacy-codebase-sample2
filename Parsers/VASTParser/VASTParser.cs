using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Parsers.TvinciVASTParser;
using System.IO;
using TVinciShared;
using KLogMonitor;
using System.Reflection;

namespace VASTParser
{
    public class VASTParser
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected string m_playerID;
        protected string m_hostName;
        protected string m_category = "default";
        protected int m_mediaID;
        protected int m_groupID;
        protected string m_adType;
        protected VAST m_vastObj;


        public VASTParser(int mediaID, int groupID, string adType)
        {
            Initialize(mediaID, groupID, adType);
            //if (!string.IsNullOrEmpty(m_playerID) && IsMediaWithAds())
            //{

            //    //string xmlStr = GetVASTXml();
            //    ////m_xmlDoc = new XmlDocument();
            //    ////m_xmlDoc.Load(xmlStr);
            //    //XmlSerializer xs = new XmlSerializer(typeof(VAST));
            //    //object result = xs.Deserialize(new StringReader(xmlStr));
            //    //VAST vasRes = result as VAST;
            //    //if (vasRes != null)
            //    //{
            //    //    m_vastObj = vasRes;
            //    //    //CachingManager.CachingManager.SetCachedData(string.Format("vast_obj_{0}", m_playerID), vasRes, 86400, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            //    //}
            //}

        }

        public VASTParser()
        {


        }





        private Linear GetAdVideoLinearNode(ref Impression_type impression)
        {
            Linear retVal = null;
            if (m_vastObj != null && m_vastObj.AdCollection != null && m_vastObj.AdCollection.Count > 0)
            {
                foreach (Ad ad in m_vastObj.AdCollection)
                {
                    if (ad != null && ad.InLine != null && ad.InLine.Creatives != null)
                    {
                        foreach (Creative creative in ad.InLine.Creatives)
                        {
                            if (creative != null && creative.Linear != null)
                            {
                                if (creative.Linear.MediaFiles != null && creative.Linear.MediaFiles.Count > 0)
                                {
                                    if (ad.InLine.ImpressionCollection != null && ad.InLine.ImpressionCollection.Count > 0)
                                    {
                                        impression = ad.InLine.ImpressionCollection[0];
                                    }
                                    retVal = creative.Linear;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        protected virtual bool IsMediaWithAds()
        {
            return true;
        }

        private CompanionAds GetCompanionAds()
        {
            CompanionAds retVal = null;
            if (m_vastObj != null && m_vastObj.AdCollection != null && m_vastObj.AdCollection.Count > 0)
            {
                foreach (Ad ad in m_vastObj.AdCollection)
                {
                    if (ad != null && ad.InLine != null && ad.InLine.Creatives != null)
                    {
                        foreach (Creative creative in ad.InLine.Creatives)
                        {
                            if (creative != null && creative.CompanionAds != null && creative.CompanionAds.Count > 0)
                            {
                                retVal = creative.CompanionAds;
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        private Companion_type GetCompanionAdNode(string w, string h)
        {
            Companion_type retVal = null;
            if (m_vastObj != null && m_vastObj.AdCollection != null && m_vastObj.AdCollection.Count > 0)
            {
                foreach (Ad ad in m_vastObj.AdCollection)
                {
                    if (ad != null && ad.InLine != null && ad.InLine.Creatives != null)
                    {
                        foreach (Creative creative in ad.InLine.Creatives)
                        {
                            if (creative != null && creative.CompanionAds != null && creative.CompanionAds.Count > 0)
                            {
                                foreach (Companion_type comapanionAd in creative.CompanionAds)
                                {
                                    if (comapanionAd != null && comapanionAd.width.Equals(w) && comapanionAd.height.Equals(h))
                                    {
                                        retVal = comapanionAd;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        public string GetCompanionAdXml(string w, string h)
        {
            string retVal = string.Empty;
            if (IsMediaWithAds())
            {
                Companion_type ct = GetCompanionAdNode(w, h);
                if (ct != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlElement root = xmlDoc.CreateElement("response");
                    root.SetAttribute("type", "vast_ad_companion");
                    xmlDoc.AppendChild(root);
                    XmlElement adElement = xmlDoc.CreateElement("companionAd");
                    adElement.SetAttribute("width", ct.width);
                    adElement.SetAttribute("height", ct.height);
                    string sHTMLSource = ct.HTMLResource.Replace(".swf", string.Format(".swf?clickTAG={0}", ct.CompanionClickThrough));
                    XmlCDataSection cDataSec = xmlDoc.CreateCDataSection(sHTMLSource);

                    adElement.AppendChild(cDataSec);
                    adElement.SetAttribute("clickthrough", ct.CompanionClickThrough);
                    if (ct.TrackingEvents != null & ct.TrackingEvents.TrackingCollection != null && ct.TrackingEvents.TrackingCollection.Count > 0)
                    {
                        adElement.SetAttribute("creativeView", ct.TrackingEvents.TrackingCollection[0].Value);
                    }
                    root.AppendChild(adElement);
                    retVal = xmlDoc.OuterXml;
                }
            }
            return retVal;
        }

        protected virtual string GetVASTXml()
        {
            //ToDo - return real URL by real player key
            string url = @"http://admatcher.videostrip.com/?categories=default&puid=23941324&host=ximon.nl&fmt=vast20";
            string retXml = WS_Utils.SendXMLHttpReq(url, string.Empty, string.Empty);
            return retXml;
        }

        public virtual string GetVastURL()
        {
            string retVal = @"http://admatcher.videostrip.com/?categories=default&puid=23941324&host=ximon.nl&fmt=vast20";
            return retVal;
        }

        public void SetXml(string xmlStr)
        {

            //m_xmlDoc.Load(xmlStr);
            XmlSerializer xs = new XmlSerializer(typeof(VAST));
            object result = xs.Deserialize(new StringReader(xmlStr));
            VAST vasRes = result as VAST;
            if (vasRes != null)
            {
                m_vastObj = vasRes;
                //CachingManager.CachingManager.SetCachedData(string.Format("vast_obj_{0}", m_playerID), vasRes, 86400, System.Web.Caching.CacheItemPriority.AboveNormal, 0, false);
            }
        }

        protected virtual void Initialize(int mediaID, int groupID, string adType)
        {
        }

        public string GetAdFileXml()
        {
            string retVal = string.Empty;
            if (IsMediaWithAds())
            {
                StringBuilder sb = new StringBuilder();
                Impression_type impression = null;
                Linear linearNode = GetAdVideoLinearNode(ref impression);

                if (linearNode != null)
                {

                    MediaFile mf = linearNode.MediaFiles[0];
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlElement root = xmlDoc.CreateElement("package");
                    xmlDoc.AppendChild(root);
                    //XmlElement packageElement = xmlDoc.CreateElement("package");
                    XmlElement videoElement = xmlDoc.CreateElement("video");
                    XmlElement itemElement = xmlDoc.CreateElement("item");
                    itemElement.SetAttribute("type", mf.type);
                    itemElement.SetAttribute("duration", string.Empty);
                    XmlElement srcElement = xmlDoc.CreateElement("src");
                    XmlElement impressionElement = xmlDoc.CreateElement("impression");
                    XmlCDataSection cDataSec = xmlDoc.CreateCDataSection(mf.Value);
                    XmlElement clickTagElement = xmlDoc.CreateElement("clickTag");
                    //XmlElement startTagElement = xmlDoc.CreateElement("startTag");
                    //XmlElement endTagElement = xmlDoc.CreateElement("endTag");
                    XmlElement companionElement = xmlDoc.CreateElement("companionBanner");
                    CompanionAds compType = GetCompanionAds();

                    XmlSerializer xs = new XmlSerializer(typeof(CompanionAds));
                    XmlWriterSettings xws = new XmlWriterSettings();
                    xws.OmitXmlDeclaration = true;
                    string compXml = string.Empty;
                    if (compType != null)
                    {
                        using (StringWriter writer = new StringWriter())
                        {
                            xs.Serialize(writer, compType);

                            compXml = writer.ToString();
                            try
                            {
                                XmlDocument companionDoc = new XmlDocument();
                                companionDoc.LoadXml(compXml);
                                if (companionDoc != null)
                                {
                                    foreach (XmlNode node in companionDoc.ChildNodes)
                                    {
                                        if (node.NodeType != XmlNodeType.XmlDeclaration)
                                        {
                                            root.AppendChild(root.OwnerDocument.ImportNode(node, true));
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(string.Empty, ex);
                                int i = 0;
                            }

                        }
                    }
                    //if (!string.IsNullOrEmpty(compXml))
                    //{
                    //    companionElement.InnerText = compXml;
                    //}

                    srcElement.InnerText = mf.Value;
                    itemElement.AppendChild(srcElement);
                    itemElement.AppendChild(companionElement);
                    if (impression != null)
                    {
                        XmlCDataSection impressionCData = xmlDoc.CreateCDataSection(impression.Value);
                        impressionElement.AppendChild(impressionCData);
                        itemElement.AppendChild(impressionElement);
                    }
                    if (linearNode.VideoClicks != null && linearNode.VideoClicks.ClickThrough != null)
                    {
                        XmlCDataSection clickCData = xmlDoc.CreateCDataSection(linearNode.VideoClicks.ClickThrough.Value);
                        clickTagElement.AppendChild(clickCData);
                    }
                    if (linearNode.TrackingEvents != null && linearNode.TrackingEvents.Count > 0)
                    {
                        foreach (Tracking te in linearNode.TrackingEvents)
                        {
                            if (te.@event == Event.start)
                            {
                                XmlElement startTagElement = xmlDoc.CreateElement("startTag");
                                XmlCDataSection startCData = xmlDoc.CreateCDataSection(te.Value);
                                startTagElement.AppendChild(startCData);
                                itemElement.AppendChild(startTagElement);
                            }
                            else if (te.@event == Event.complete)
                            {
                                XmlElement endTagElement = xmlDoc.CreateElement("endTag");
                                XmlCDataSection endCData = xmlDoc.CreateCDataSection(te.Value);
                                endTagElement.AppendChild(endCData);
                                itemElement.AppendChild(endTagElement);
                            }
                            else
                            {
                                XmlElement trackTagElement = xmlDoc.CreateElement(te.@event.ToString());
                                XmlCDataSection trackCData = xmlDoc.CreateCDataSection(te.Value);
                                trackTagElement.AppendChild(trackCData);
                                itemElement.AppendChild(trackTagElement);
                            }
                        }
                    }

                    itemElement.AppendChild(clickTagElement);


                    videoElement.AppendChild(itemElement);
                    root.AppendChild(videoElement);
                    retVal = xmlDoc.OuterXml;
                }
            }
            return retVal;
        }


    }
}
