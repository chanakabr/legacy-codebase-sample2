using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace RoviFeeder
{
    public class Rovi_CMTFeeder : RoviBaseFeeder
    {
        private string   m_url;
        private int      m_fromID;
        private int      m_groupID;


        public Rovi_CMTFeeder()
        {
            m_url       = string.Empty;
            m_fromID    = 0;
            m_groupID   = 0;
        }

        public Rovi_CMTFeeder(string url, int fromID, int nGroupID)
        {
            m_url       = url;
            m_fromID    = fromID;
            m_groupID   = nGroupID;
        }

        private string ContentURL
        {
            get
            {
                if (string.IsNullOrEmpty(m_url))
                {
                    return "https://choice-ce.nowtilus.tv/services/tvinci/v1/marketing/campaigns/";
                }

                return m_url;
            }
        }

        public override bool Ingest()
        {
            Dictionary<int, string> dCampaignUrls = RoviFeederUtils.GetPresentationsDict(ContentURL);

            if (m_fromID > 0)
            {
                dCampaignUrls = dCampaignUrls.Where(x => x.Key >= m_fromID).ToDictionary(d => d.Key, d => d.Value);
            }

            if (dCampaignUrls == null || dCampaignUrls.Count == 0)
            {
                return false;
            }

            Dictionary<int, string> dErrorCampaignUrls = new Dictionary<int, string>();
            Dictionary<int, string> dRetryCampaignUrls = new Dictionary<int, string>();


            XslCompiledTransform Xslt = new XslCompiledTransform();
            Xslt.Load(ConfigurationManager.AppSettings["XSL_PATH"].ToString() + "ChannelBuild.xslt");

            foreach (KeyValuePair<int, string> entry in dCampaignUrls)
            {
                bool res = false;

                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        string campaignUrl = entry.Value;

                        double dTime = 0.0;

                        res = TryIngestItem(campaignUrl, Xslt, out dTime);

                        if (res) { break; }
                    }
                    catch
                    {
                        dCampaignUrls[entry.Key] = entry.Value;
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                if (!res)
                {
                    dCampaignUrls[entry.Key] = entry.Value;
                }

                System.Threading.Thread.Sleep(1000);
            }

            // filter erroneous campaign
            dRetryCampaignUrls = dRetryCampaignUrls.Where(d => !dRetryCampaignUrls.ContainsKey(d.Key)).ToDictionary(d => d.Key, d => d.Value);

            dCampaignUrls.Clear();
            dCampaignUrls = new Dictionary<int, string>(dRetryCampaignUrls);

            //
            // RETRY
            //
            System.Threading.Thread.Sleep(1000);

            if (dRetryCampaignUrls != null && dRetryCampaignUrls.Count > 0)
            {
                int retryCount = 2;

                for (int i = 0; i < retryCount; i++)
                {
                    foreach (KeyValuePair<int, string> entry in dRetryCampaignUrls)
                    {
                        try
                        {
                            string campaignUrl = entry.Value;

                            double dTime = 0.0;

                            if (TryIngestItem(campaignUrl, Xslt, out dTime))
                            {
                                dCampaignUrls.Remove(entry.Key);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (dCampaignUrls == null || dCampaignUrls.Count == 0)
                    {
                        break;
                    }

                    dRetryCampaignUrls.Clear();
                    dRetryCampaignUrls = new Dictionary<int, string>(dCampaignUrls);

                    System.Threading.Thread.Sleep(1000 * 60 * 60);  // sleep an hour, then try again  // * (i+1));
                }

            }

            Xslt = null;

            return true;
        }

        private bool TryIngestItem(string sVodUrl, XslCompiledTransform transformer, out double dTime)
        {
            bool res = false;
            bool doIngest = true;

            DateTime dStart = DateTime.UtcNow;
            string campaignXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodUrl, "", "", "application/json", "", "", "", "", "get");
            campaignXML = campaignXML.Replace("xml:lang", "lang");

            dTime = DateTime.UtcNow.Subtract(dStart).TotalMilliseconds;

            if (string.IsNullOrEmpty(campaignXML))
            {
                return false;
            }

            XmlDocument XMLD = new XmlDocument();

            XMLD.LoadXml(campaignXML);

            string sMovieID = sVodUrl.Split('/').Last();

            string appXmlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rovi_xml");
            if (!Directory.Exists(appXmlDir))
            {
                Directory.CreateDirectory(appXmlDir);
            }

            string xmlFileOut = Path.Combine(appXmlDir, sMovieID + ".xml");  //string xmlFileOut = string.Format("E:/Projects/rovi_xml/{0}.xml", sMovieID);
            XMLD.Save(xmlFileOut);

            try
            {
                RoviFeeder.RoviCMT.RoviNowtilusVodApi roviResult;
                XmlSerializer serializer = new XmlSerializer(typeof(RoviFeeder.RoviCMT.RoviNowtilusVodApi));

                using (TextReader reader = new StringReader(campaignXML))
                {
                    roviResult = (RoviFeeder.RoviCMT.RoviNowtilusVodApi)serializer.Deserialize(reader);

                    RoviFeeder.RoviCMT.RoviNowtilusVodApiCampaign roviTitle = roviResult.Campaign;

                    if (!RoviFeederUtils.Validate(roviTitle))
                    {
                        File.Move(xmlFileOut, Path.ChangeExtension(xmlFileOut, ".err"));
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            if (doIngest)
            {
                using (StringWriter writer = new StringWriter())
                {
                    try
                    {
                        XmlReader reader = XmlReader.Create(new StringReader(XMLD.InnerXml));
                        StringWriter output = new StringWriter();
                        XmlWriter writers = XmlWriter.Create(writer);
                        transformer.Transform(reader, writers);
                    }
                    catch
                    {
                        File.Move(xmlFileOut, Path.ChangeExtension(xmlFileOut, ".err"));
                        throw;
                    }

                    XMLD.LoadXml(writer.ToString());
                }

                string exeptionString = string.Empty;
                string sCoGuid = string.Empty;
                int nChannelID = 0;

                res = TvinciImporter.ImporterImpl.ProcessChannelItems(XMLD, ref sCoGuid, ref nChannelID, ref exeptionString, m_groupID);
                if (!res)
                {
                    File.Move(xmlFileOut, Path.ChangeExtension(xmlFileOut, ".err"));
                }

                XMLD = null;
            }

            return res;
        }
    }
}
