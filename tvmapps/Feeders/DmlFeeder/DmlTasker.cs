using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using TVinciShared;
using KLogMonitor;
using System.Reflection;

namespace DmlFeeder
{
    public class DmlTasker : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // m_uniqueKey unique connection key, m_url address of the get request and m_lastDate the invoke interval 
        private string m_lastDate = string.Empty;
        private string m_url = string.Empty;
        private string m_uniqueKey = string.Empty;
        private int m_groupID = 0;

        private DmlTasker(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            string[] seperator = { "||" };
            string[] splited = engrameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 4)
            {
                m_groupID = int.Parse(splited[0]);
                m_lastDate = splited[1];
                m_url = splited[2];
                m_uniqueKey = splited[3];
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new DmlTasker(nTaskID, nIntervalInSec, engrameters);
        }

        private XmlDocument GetDmlXmlString()
        {
            String ret = string.Empty;

            string sFullURL = string.Format("{0}?fromDate=\"{1}\"", m_url, m_lastDate);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sFullURL);
            request.Headers["X-DML-API-KEY"] = m_uniqueKey;
            request.Method = "GET";
            request.ContentType = "text/xml; encoding='utf-8'";

            HttpWebResponse myHttpWebResponse = null;
            XmlDocument dmlXMLDocument = null;
            XmlTextReader dmlXMLReader = null;

            try
            {
                //Get Response
                myHttpWebResponse = (HttpWebResponse)request.GetResponse();

                //Now load the XML Document
                dmlXMLDocument = new XmlDocument();

                //Load response stream into XMLReader
                dmlXMLReader = new XmlTextReader(myHttpWebResponse.GetResponseStream());
                dmlXMLDocument.Load(dmlXMLReader);

                string res = string.IsNullOrEmpty(dmlXMLDocument.InnerXml) ? "Empty res" : "Full res";
                log.Debug("GetDMLRemote - " + string.Format("{0}", res));
            }
            catch (Exception ex)
            {
                log.Error("GetDMLRemote - " + ex.ToString(), ex);
            }

            return dmlXMLDocument;
        }

        protected override bool DoTheTaskInner()
        {
            log.Debug("Start task - " + string.Format("Group:{0}, LastDate:{1}, Url:{2}, UniqueKey:{3} ", m_groupID, m_lastDate, m_url, m_uniqueKey));
            // Get Dml xml using http post request
            DateTime d = DateTime.UtcNow;
            XmlDocument dmlXMLDocument = GetDmlXmlString();

            StringWriter writer = new StringWriter();
            XmlDocument mediaOutXML = new XmlDocument();
            XSLT_transform_handlar.DmlTransform DmlTransformer = new XSLT_transform_handlar.DmlTransform();

            DmlTransformer.Init();

            // ingest media
            try
            {
                DmlTransformer.Transform(dmlXMLDocument, writer);
                mediaOutXML.LoadXml(writer.ToString());

                string exeptionString = string.Empty;
                TvinciImporter.ImporterImpl.DoTheWorkInner(mediaOutXML.OuterXml, m_groupID, "", ref exeptionString, false);
                log.Debug("Importer res - " + exeptionString.ToString());

                string mediaListParameters = string.Format("<medias>{0}</medias>", exeptionString);
                SaveIngestDataToFTP(writer.ToString(), dmlXMLDocument.InnerXml.ToString(), mediaListParameters);

            }
            catch (Exception ex)
            {
                log.Error("Ingest fail - " + ex.ToString(), ex);
            }

            string parameters = string.Format("{0}||{1}||{2}||{3}", m_groupID, d.ToString("yyyy-MM-ddTHH:mm:ss"), m_url, m_uniqueKey);

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PARAMETERS", "=", parameters);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nTaskID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            log.Debug("Ending task," + d.ToString("yyyy-MM-ddTHH:mm:ss"));
            return true;
        }

        private void SaveIngestDataToFTP(string sOrigXML, string sNormXML, string mediaListParameters)
        {
            int ingestID = IngestionUtils.InsertIngestToDB(DateTime.Parse(m_lastDate), 2, m_groupID);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(mediaListParameters);

            XmlNodeList xmlNL = doc.GetElementsByTagName("media");

            foreach (XmlElement el in xmlNL)
            {
                string mediaID = el.GetAttribute("tvm_id");
                string status = el.GetAttribute("status");
                string coGuid = el.GetAttribute("co_guid");
                int nTVMID = 0;
                if (!string.IsNullOrEmpty(mediaID))
                {
                    nTVMID = int.Parse(mediaID);
                }

                IngestionUtils.InsertIngestMediaData(ingestID, nTVMID, coGuid, status);
            }



            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            files.Add("original.xml", IngestionUtils.StringToBytes(sOrigXML));
            files.Add("normalized.xml", IngestionUtils.StringToBytes(sNormXML));
            IngestionUtils.UploadIngestToFTP(ingestID, files);
        }
    }
}
