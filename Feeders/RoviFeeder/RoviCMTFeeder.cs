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
using XSLT_transform_handlar;

namespace RoviFeeder
{
    public class RoviCMTFeeder : RoviBaseFeeder
    {
        public RoviCMTFeeder()
        {
            m_url = string.Empty;
            m_fromID = 0;
            m_groupID = 0;
        }

        public RoviCMTFeeder(string url, int fromID, int nGroupID)
        {
            m_url = url;
            m_fromID = fromID;
            m_groupID = nGroupID;
        }


        private string ContentURL
        {
            get
            {
                if (string.IsNullOrEmpty(m_url))
                {
                    return "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/marketing/campaigns/";
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


            RoviTransform transformer = new RoviTransform();
            transformer.Init();

            foreach (KeyValuePair<int, string> entry in dCampaignUrls)
            {
                bool res = false;

                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        string campaignUrl = entry.Value;

                        res = TryIngestItem(campaignUrl, transformer);

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

                            if (TryIngestItem(campaignUrl, transformer))
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

            transformer = null;

            return true;
        }

        private bool TryIngestItem(string sVodUrl, RoviTransform transformer)
        {
            bool res = false;

            string campaignXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodUrl, "", "", "application/json", "", "", "", "", "get");
            campaignXML = campaignXML.Replace("xml:lang", "lang");

            if (string.IsNullOrEmpty(campaignXML))
            {
                return false;
            }

            XmlDocument XMLD = new XmlDocument();

            XMLD.LoadXml(campaignXML);

            try
            {
                RoviFeeder.CMT_XSD.RoviNowtilusVodApi roviResult;
                XmlSerializer serializer = new XmlSerializer(typeof(RoviFeeder.CMT_XSD.RoviNowtilusVodApi));

                using (TextReader reader = new StringReader(campaignXML))
                {
                    roviResult = (RoviFeeder.CMT_XSD.RoviNowtilusVodApi)serializer.Deserialize(reader);

                    RoviFeeder.CMT_XSD.RoviNowtilusVodApiCampaign roviTitle = roviResult.Campaign;

                    if (!RoviFeederUtils.Validate(roviTitle))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    transformer.TransformA(XMLD, writer, RoviTransform.assetType.CMT);
                }
                catch
                {
                    return false; ;
                }

                XMLD.LoadXml(writer.ToString());
            }

            string exeptionString = string.Empty;
            string sCoGuid = string.Empty;
            int nChannelID = 0;

            res = TvinciImporter.ImporterImpl.ProcessChannelItems(XMLD, ref sCoGuid, ref nChannelID, ref exeptionString, m_groupID);

            IngestNotificationStatus configurationStatus = res == true ? IngestNotificationStatus.SUCCESS : IngestNotificationStatus.ERROR;

            RoviFeederUtils.SendIngestNotification(configurationStatus, sVodUrl, exeptionString);
            if (!res)
            {
                return false;
            }

            XMLD = null;

            return res;
        }
    }
}
