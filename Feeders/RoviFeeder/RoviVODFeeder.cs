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
    public class Rovi_VODFeeder : RoviBaseFeeder
    {
        private string   m_url;
        private int      m_fromID;
        private int      m_groupID;

        public Rovi_VODFeeder()
        {
            m_url       = string.Empty;
            m_fromID    = 0;
            m_groupID   = 0;
        }

        public Rovi_VODFeeder(string url, int fromID, int nGroupID)
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
                    return "https://choice-ce.nowtilus.tv/services/tvinci/v1/content/movies";
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

                        double dTime = 0.0;

                        res = TryIngestItem(movieUrl, transformer, out dTime);

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

                            double dTime = 0.0;

                            if (TryIngestItem(movieUrl, transformer, out dTime))
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
        }

        private bool TryIngestItem(string sVodUrl, RoviTransform transformer, out double dTime)
        {
            bool res = false;
            bool doIngest = true;

            DateTime dStart = DateTime.UtcNow;
            string movieXML = TVinciShared.WS_Utils.SendXMLHttpReq(sVodUrl, "", "", "application/json", "", "", "", "", "get");
            movieXML = movieXML.Replace("xml:lang", "lang");

            dTime = DateTime.UtcNow.Subtract(dStart).TotalMilliseconds;

            if (string.IsNullOrEmpty(movieXML))
            {
                return false;
            }

            XmlDocument XMLD = new XmlDocument();

            XMLD.LoadXml(movieXML);

            string sMovieID = sVodUrl.Split('/').Last();

            string appXmlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rovi_xml");
            if (!Directory.Exists(appXmlDir))
            {
                Directory.CreateDirectory(appXmlDir);
            }

            string xmlFileOut = Path.Combine(appXmlDir, sMovieID + ".xml");
            XMLD.Save(xmlFileOut);

            try
            {
                RoviFeeder.VOD_object.RoviNowtilusVodApi roviResult;
                XmlSerializer serializer = new XmlSerializer(typeof(RoviFeeder.VOD_object.RoviNowtilusVodApi));

                using (TextReader reader = new StringReader(movieXML))
                {
                    roviResult = (RoviFeeder.VOD_object.RoviNowtilusVodApi)serializer.Deserialize(reader);

                    RoviFeeder.VOD_object.RoviNowtilusVodApiPresentation roviTitle = roviResult.Presentation;

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
                        transformer.Transform(Path.GetDirectoryName(xmlFileOut), Path.GetFileName(xmlFileOut), writer);
                    }
                    catch
                    {
                        File.Move(xmlFileOut, Path.ChangeExtension(xmlFileOut, ".err"));
                        throw;
                    }

                    XMLD.LoadXml(writer.ToString());
                }

                string exeptionString = string.Empty;

                res = TvinciImporter.ImporterImpl.DoTheWorkInner(XMLD.OuterXml, m_groupID, "", ref exeptionString, false);
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
