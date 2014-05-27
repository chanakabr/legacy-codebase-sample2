using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using XSLT_transform_handlar;

namespace RoviFeeder
{
    public class RoviEpisodeFeeder : RoviBaseFeeder
    {
        public RoviEpisodeFeeder()
        {
            m_url = string.Empty;
            m_fromID = 0;
            m_groupID = 0;
        }

        public RoviEpisodeFeeder(string url, int fromID, int nGroupID)
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
                    return "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/content/seasons";
                }

                return m_url;
            }
        }

        public override bool Ingest()
        {
            Dictionary<int, string> dSeasonsUrls = RoviFeederUtils.GetPresentationsDict(ContentURL);

            if (dSeasonsUrls == null || dSeasonsUrls.Count == 0)
            {
                Logger.Logger.Log("Error", string.Format(" season url is null {0}", ContentURL), "RoviEpisodeFeeder");
                return false;
            }

            if (m_fromID > 0)
            {
                dSeasonsUrls = dSeasonsUrls.Where(x => x.Key >= m_fromID).ToDictionary(d => d.Key, d => d.Value);
            }

            if (dSeasonsUrls == null || dSeasonsUrls.Count == 0)
            {
                Logger.Logger.Log("Error", string.Format("not found season URLs in relevant IDs"), "RoviEpisodeFeeder");
                return false;
            }


            Dictionary<int, string> dErrorSeasonsUrls = new Dictionary<int, string>();
            Dictionary<int, string> dRetrySeasonsUrls = new Dictionary<int, string>();

            RoviTransform transformer = new RoviTransform();
            transformer.Init();


            foreach (KeyValuePair<int, string> entry in dSeasonsUrls)
            {
                bool res = false;

                for (int i = 0; i < 2; i++)
                {

                    try
                    {
                        string seasonsUrl = entry.Value;

                        res = TryIngestItem(seasonsUrl, transformer);
                        Logger.Logger.Log("TryIngest", string.Format("{0}, {1}", seasonsUrl, res.ToString()), "RoviEpisodeFeeder");

                        if (res) { break; }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log("Error", string.Format("exception in Ingest: {0}, {1}", entry.Value, ex.Message), "RoviEpisodeFeeder");
                        dErrorSeasonsUrls[entry.Key] = entry.Value;
                        System.Threading.Thread.Sleep(1000);
                    }

                }

                if (!res)
                {
                    dRetrySeasonsUrls[entry.Key] = entry.Value;
                }

                System.Threading.Thread.Sleep(1000);
            }

            // filter erroneous media
            dRetrySeasonsUrls = dRetrySeasonsUrls.Where(d => !dErrorSeasonsUrls.ContainsKey(d.Key)).ToDictionary(d => d.Key, d => d.Value);

            dSeasonsUrls.Clear();
            dSeasonsUrls = new Dictionary<int, string>(dRetrySeasonsUrls);

            //
            // RETRY
            //
            System.Threading.Thread.Sleep(1000);

            if (dRetrySeasonsUrls != null && dRetrySeasonsUrls.Count > 0)
            {
                int retryCount = 2;

                for (int i = 0; i < retryCount; i++)
                {
                    foreach (KeyValuePair<int, string> entry in dRetrySeasonsUrls)
                    {
                        try
                        {
                            string seasonsUrl = entry.Value;

                            if (TryIngestItem(seasonsUrl, transformer))
                            {
                                dSeasonsUrls.Remove(entry.Key);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("Error", string.Format("exception in retryIngest: {0}, {1}", entry.Value, ex.Message), "RoviEpisodeFeeder");
                            continue;
                        }
                    }

                    if (dSeasonsUrls == null || dSeasonsUrls.Count == 0)
                    {
                        break;
                    }

                    dRetrySeasonsUrls.Clear();
                    dRetrySeasonsUrls = new Dictionary<int, string>(dSeasonsUrls);

                    System.Threading.Thread.Sleep(1000 * 60 * 60);  // sleep an hour, then try again  // * (i+1));
                }

            }

            transformer = null;

            return true;
        }

        private bool TryIngestItem(string sVodUrl, RoviTransform transformer)
        {
            bool res = false;

            string seasonsXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodUrl, "", "", "application/json", "", "", "", "", "get");
            seasonsXML = seasonsXML.Replace("xml:lang", "lang");

            if (string.IsNullOrEmpty(seasonsXML))
            {
                Logger.Logger.Log("Error", string.Format("seasonXML is empty {0}", sVodUrl), "RoviEpisodeFeeder");
                return false;
            }

            XmlDocument XMLD = new XmlDocument();

            XMLD.LoadXml(seasonsXML);

            try
            {
                RoviFeeder.EpisodeXSD.RoviNowtilusVodApi roviResult;
                XmlSerializer serializer = new XmlSerializer(typeof(RoviFeeder.EpisodeXSD.RoviNowtilusVodApi));

                //using (TextReader reader = new StringReader(seasonsXML))
                //{
                //    roviResult = (RoviFeeder.EpisodeXSD.RoviNowtilusVodApi)serializer.Deserialize(reader);

                //    RoviFeeder.EpisodeXSD.RoviNowtilusVodApiPresentation roviTitle = roviResult.Presentation;

                //    if (!RoviFeederUtils.Validate(roviTitle))
                //    {
                //        Logger.Logger.Log("Error", string.Format("roviTitle is not valid"), "RoviEpisodeFeeder");
                //        return false;
                //    }
                //}
            }
            catch
            {
                Logger.Logger.Log("Error", string.Format("Error in Deserialization"), "RoviEpisodeFeeder");
                return false;
            }

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    transformer.TransformA(XMLD, writer, RoviTransform.assetType.EPISODE);
                }
                catch
                {
                    Logger.Logger.Log("Error", string.Format("Error in transforming"), "RoviEpisodeFeeder");
                    return false;
                }

                XMLD.LoadXml(writer.ToString());
            }

            string exeptionString = string.Empty;

            res = TvinciImporter.ImporterImpl.DoTheWorkInner(XMLD.OuterXml, m_groupID, "", ref exeptionString, false);

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

