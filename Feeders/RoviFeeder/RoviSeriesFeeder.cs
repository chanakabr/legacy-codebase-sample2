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
    public class RoviSeriesFeeder : RoviBaseFeeder
    {
        public RoviSeriesFeeder()
        {
            m_url = string.Empty;
            m_fromID = 0;
            m_groupID = 0;
        }

        public RoviSeriesFeeder(string url, int fromID, int nGroupID)
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
                    return "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/content/series";
                }

                return m_url;
            }
        }

        public override bool Ingest()
        {
            Dictionary<int, string> dSeriesUrls = RoviFeederUtils.GetPresentationsDict(ContentURL);

            if (dSeriesUrls == null || dSeriesUrls.Count == 0)
            {
                return false;
            }

            if (m_fromID > 0)
            {
                dSeriesUrls = dSeriesUrls.Where(x => x.Key >= m_fromID).ToDictionary(d => d.Key, d => d.Value);
            }

            if (dSeriesUrls == null || dSeriesUrls.Count == 0)
            {
                return false;
            }


            Dictionary<int, string> dErrorSeriesUrls = new Dictionary<int, string>();
            Dictionary<int, string> dRetrySeriesUrls = new Dictionary<int, string>();

            RoviTransform transformer = new RoviTransform();
            transformer.Init();


            foreach (KeyValuePair<int, string> entry in dSeriesUrls)
            {
                bool res = false;

                for (int i = 0; i < 2; i++)
                {

                    try
                    {
                        string SeriesUrl = entry.Value;

                        res = TryIngestItem(SeriesUrl, transformer);

                        if (res) { break; }
                    }
                    catch
                    {
                        dErrorSeriesUrls[entry.Key] = entry.Value;
                        System.Threading.Thread.Sleep(1000);
                    }

                }

                if (!res)
                {
                    dRetrySeriesUrls[entry.Key] = entry.Value;
                }

                System.Threading.Thread.Sleep(1000);
            }

            // filter erroneous media
            dRetrySeriesUrls = dRetrySeriesUrls.Where(d => !dErrorSeriesUrls.ContainsKey(d.Key)).ToDictionary(d => d.Key, d => d.Value);

            dSeriesUrls.Clear();
            dSeriesUrls = new Dictionary<int, string>(dRetrySeriesUrls);

            //
            // RETRY
            //
            System.Threading.Thread.Sleep(1000);

            if (dRetrySeriesUrls != null && dRetrySeriesUrls.Count > 0)
            {
                int retryCount = 2;

                for (int i = 0; i < retryCount; i++)
                {
                    foreach (KeyValuePair<int, string> entry in dRetrySeriesUrls)
                    {
                        try
                        {
                            string SeriesUrl = entry.Value;

                            if (TryIngestItem(SeriesUrl, transformer))
                            {
                                dSeriesUrls.Remove(entry.Key);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (dSeriesUrls == null || dSeriesUrls.Count == 0)
                    {
                        break;
                    }

                    dRetrySeriesUrls.Clear();
                    dRetrySeriesUrls = new Dictionary<int, string>(dSeriesUrls);

                    System.Threading.Thread.Sleep(1000 * 60 * 60);  // sleep an hour, then try again  // * (i+1));
                }

            }

            transformer = null;

            return true;
        }

        private bool TryIngestItem(string sVodUrl, RoviTransform transformer)
        {
            bool res = false;

            string SeriesXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodUrl, "", "", "application/json", "", "", "", "", "get");
            SeriesXML = SeriesXML.Replace("xml:lang", "lang");

            if (string.IsNullOrEmpty(SeriesXML))
            {
                return false;
            }

            XmlDocument XMLD = new XmlDocument();

            XMLD.LoadXml(SeriesXML);

            //removed - the xsd does not match the current XML
            //try
            //{
            //    RoviFeeder.SeriesXSD.RoviNowtilusVodApi roviResult;
            //    XmlSerializer serializer = new XmlSerializer(typeof(RoviFeeder.SeriesXSD.RoviNowtilusVodApi));

            //    using (TextReader reader = new StringReader(SeriesXML))
            //    {
            //        roviResult = (RoviFeeder.SeriesXSD.RoviNowtilusVodApi)serializer.Deserialize(reader);

            //        RoviFeeder.SeriesXSD.RoviNowtilusVodApiPresentation roviTitle = roviResult.Presentation;

            //        if (!RoviFeederUtils.Validate(roviTitle))
            //        {
            //            return false;
            //        }
            //    }
            //}
            //catch
            //{
            //    return false;
            //}

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    transformer.TransformA(XMLD, writer, RoviTransform.assetType.SERIES);
                }
                catch
                {
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

