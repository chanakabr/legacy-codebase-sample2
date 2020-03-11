using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using KLogMonitor;
using XSLT_transform_handlar;

namespace RoviFeeder
{
    public class RoviMovieFeeder : RoviBaseFeeder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public RoviMovieFeeder()
        {
            m_url = string.Empty;
            m_fromID = 0;
            m_groupID = 0;
        }

        public RoviMovieFeeder(string url, int fromID, int nGroupID)
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
                    return "https://choice-ce.nowtilus.tv/services/tvinci/v1.1/content/movies";
                }

                return m_url;
            }
        }

        public override bool Ingest()
        {
            Dictionary<int, string> dMovieUrls = RoviFeederUtils.GetPresentationsDict(ContentURL);

            if (dMovieUrls == null || dMovieUrls.Count == 0)
            {
                return false;
            }

            if (m_fromID > 0)
            {
                dMovieUrls = dMovieUrls.Where(x => x.Key >= m_fromID).ToDictionary(d => d.Key, d => d.Value);
            }

            if (dMovieUrls == null || dMovieUrls.Count == 0)
            {
                return false;
            }

            RoviTransform transformer = new RoviTransform();
            transformer.Init();

            foreach (KeyValuePair<int, string> entry in dMovieUrls)
            {
                bool res = false;
                string movieUrl = entry.Value;
                try
                {
                    res = TryIngestItem(movieUrl, transformer);
                    log.Error("Error - " + string.Format("{0}, {1}", movieUrl, res.ToString()));
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("{0}, {1}", movieUrl, ex.Message), ex);
                }

                System.Threading.Thread.Sleep(1000);
            }

            return true;


            /*
            Dictionary<int, string> dErrorMovieUrls = new Dictionary<int, string>();
            Dictionary<int, string> dRetryMovieUrls = new Dictionary<int, string>();

            RoviTransform transformer = new RoviTransform();
            transformer.Init();


            foreach (KeyValuePair<int, string> entry in dMovieUrls)
            {
                bool res = false;

                for (int i = 0; i < 2; i++)
                {

                    try
                    {
                        string movieUrl = entry.Value;

                        res = TryIngestItem(movieUrl, transformer);

                        if (res) { break; }
                    }
                    catch
                    {
                        dErrorMovieUrls[entry.Key] = entry.Value;
                        System.Threading.Thread.Sleep(1000);
                    }

                }

                if (!res)
                {
                    dRetryMovieUrls[entry.Key] = entry.Value;
                }

                System.Threading.Thread.Sleep(1000);
            }

            // filter erroneous media
            dRetryMovieUrls = dRetryMovieUrls.Where(d => !dErrorMovieUrls.ContainsKey(d.Key)).ToDictionary(d => d.Key, d => d.Value);

            dMovieUrls.Clear();
            dMovieUrls = new Dictionary<int, string>(dRetryMovieUrls);

            //
            // RETRY
            //
            System.Threading.Thread.Sleep(1000);

            if (dRetryMovieUrls != null && dRetryMovieUrls.Count > 0)
            {
                int retryCount = 2;

                for (int i = 0; i < retryCount; i++)
                {
                    foreach (KeyValuePair<int, string> entry in dRetryMovieUrls)
                    {
                        try
                        {
                            string movieUrl = entry.Value;

                            if (TryIngestItem(movieUrl, transformer))
                            {
                                dMovieUrls.Remove(entry.Key);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (dMovieUrls == null || dMovieUrls.Count == 0)
                    {
                        break;
                    }

                    dRetryMovieUrls.Clear();
                    dRetryMovieUrls = new Dictionary<int, string>(dMovieUrls);

                    System.Threading.Thread.Sleep(1000 * 60 * 60);  // sleep an hour, then try again  // * (i+1));
                }

            }

            transformer = null;

            return true;
            */
        }

        private bool TryIngestItem(string sVodUrl, RoviTransform transformer)
        {
            bool res = false;

            string movieXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodUrl, "", "", "application/json", "", "", "", "", "get");
            movieXML = movieXML.Replace("xml:lang", "lang");

            if (string.IsNullOrEmpty(movieXML))
            {
                return false;
            }

            XmlDocument XMLD = new XmlDocument();

            XMLD.LoadXml(movieXML);

            try
            {
                RoviFeeder.MoviesXSD.RoviNowtilusVodApi roviResult;
                XmlSerializer serializer = new XmlSerializer(typeof(RoviFeeder.MoviesXSD.RoviNowtilusVodApi));

                using (TextReader reader = new StringReader(movieXML))
                {
                    roviResult = (RoviFeeder.MoviesXSD.RoviNowtilusVodApi)serializer.Deserialize(reader);

                    RoviFeeder.MoviesXSD.RoviNowtilusVodApiPresentation roviTitle = roviResult.Presentation;

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
                    transformer.TransformA(XMLD, writer, RoviTransform.assetType.MOVIE);
                }
                catch
                {
                    return false;
                }

                XMLD.LoadXml(writer.ToString());
            }

            string exeptionString = string.Empty;
            try
            {
                res = TvinciImporter.ImporterImpl.DoTheWorkInner(XMLD.OuterXml, m_groupID, "", ref exeptionString, false);

                IngestNotificationStatus configurationStatus = res == true ? IngestNotificationStatus.SUCCESS : IngestNotificationStatus.ERROR;

                RoviFeederUtils.SendIngestNotification(configurationStatus, sVodUrl, exeptionString);
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("{0}", ex.Message), ex);
            }

            return res;
        }
    }
}
