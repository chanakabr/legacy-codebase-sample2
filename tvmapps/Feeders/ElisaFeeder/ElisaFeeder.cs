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

namespace ElisaFeeder
{
    public class ElisaFeeder : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private Int32 nGroupID;
        private string sUrl;

        public ElisaFeeder(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            sUrl = "http://pasture.saunalahti.fi/tvinci.xml";

            if (!string.IsNullOrEmpty(engrameters))
            {
                sUrl = engrameters;
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new ElisaFeeder(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            bool retValReg = true;
            bool retValVir = true;

            log.Debug("Elisa Feeder Start - Start feed");
            try
            {
                // For Real
                string sXml = WS_Utils.SendXMLHttpReq(sUrl, "", "");

                //// For Testing
                //XmlDocument testDoc = new XmlDocument();
                //testDoc.Load((System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).ToString() + "\\Elisa partial XML new 4.xml").Replace("file://", ""));
                //string sXml = testDoc.InnerXml;

                log.Debug("Elisa XML: HttpWebRequest response is :" + sXml);

                string notifyXml = string.Empty;

                string sRegularXml = null;
                string sVirtualXml = null;

                SeparateMedias(sXml, ref sRegularXml, ref sVirtualXml);

                Int32 nRegularGroupID = 135;
                Int32 nVirtualGroupID = 136;

                log.Debug("Elisa Group ID: " + nRegularGroupID.ToString());
                if (nRegularGroupID != 0)
                {
                    retValReg = TvinciImporter.ImporterImpl.DoTheWorkInner(sRegularXml, nRegularGroupID, string.Empty, ref notifyXml, false);
                }

                log.Debug("Elisa Group ID: " + nVirtualGroupID.ToString());
                if (nVirtualGroupID != 0)
                {
                    retValVir = TvinciImporter.ImporterImpl.DoTheWorkInner(sVirtualXml, nVirtualGroupID, string.Empty, ref notifyXml, false);
                }

                // Upload directories to FTP after retrieving ll metadata
                TvinciImporter.ImporterImpl.UploadDirectory(135);
                TvinciImporter.ImporterImpl.UploadDirectory(136);
            }
            catch (Exception ex)
            {
                log.Error("Elisa HttpWebRequest ERROR " + ex.Message, ex);
            }

            return (retValReg & retValVir);
        }

        private void SeparateMedias(string sXml, ref string sRegularXml, ref string sVirtualXml)
        {
            // Get Medias by type (regular/virtual)
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sXml);
            XmlNodeList oRegulars = doc.SelectNodes("/feed/export/media[@Account_Name='regular']");
            XmlNodeList oVirtuals = doc.SelectNodes("/feed/export/media[@Account_Name='virtual']");

            // Create XmlDocument for the regular media
            XmlDocument oRegularDoc = new XmlDocument();
            XmlNode oRegularDocDeclarationNode = oRegularDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            oRegularDoc.AppendChild(oRegularDocDeclarationNode);

            XmlNode oRegularFeedNode = oRegularDoc.CreateElement("feed");
            oRegularDoc.AppendChild(oRegularFeedNode);

            XmlNode oRegularExportNode = oRegularDoc.CreateElement("export");
            oRegularFeedNode.AppendChild(oRegularExportNode);

            foreach (XmlNode media in oRegulars)
            {
                XmlNode importedNode = oRegularDoc.ImportNode(media, true);
                oRegularExportNode.AppendChild(importedNode);
            }

            sRegularXml = oRegularDoc.InnerXml;

            // Create XmlDocument for the virtual media
            XmlDocument oVirtualDoc = new XmlDocument();
            XmlNode oVirtualDocDeclarationNode = oVirtualDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            oVirtualDoc.AppendChild(oVirtualDocDeclarationNode);

            XmlNode oVirtualFeedNode = oVirtualDoc.CreateElement("feed");
            oVirtualDoc.AppendChild(oVirtualFeedNode);

            XmlNode oVirtualExportNode = oVirtualDoc.CreateElement("export");
            oVirtualFeedNode.AppendChild(oVirtualExportNode);

            foreach (XmlNode media in oVirtuals)
            {
                XmlNode importedNode = oVirtualDoc.ImportNode(media, true);
                oVirtualExportNode.AppendChild(importedNode);
            }

            sVirtualXml = oVirtualDoc.InnerXml;
        }

        private int GetGroupID(string sXml)
        {
            return 134;
        }

        protected bool IsThumbnailValid(string sXML, int nGroupID)
        {
            bool retVal = false;
            XmlDocument theDoc = new XmlDocument();
            try
            {
                theDoc.LoadXml(sXML);
                XmlNodeList theItems = theDoc.SelectNodes("/feed/export/media");
                if (theItems != null && theItems.Count > 0)
                {
                    XmlNode theMediaNode = theItems[0];
                    string sThumb = XmlUtils.GetNodeParameterVal(ref theMediaNode, "basic/thumb", "url");
                    if (!string.IsNullOrEmpty(sThumb))
                    {
                        string sUrl = string.Empty;
                        if (sThumb.Contains("http"))
                        {
                            sUrl = sThumb;
                        }
                        else
                        {
                            object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
                            string sPicsBasePath = string.Empty;
                            if (oPicsBasePath != null && oPicsBasePath != System.DBNull.Value)
                            {
                                sUrl = sPicsBasePath + "/" + sThumb;
                            }
                        }
                        HttpWebResponse httpResponse = null;
                        if (!string.IsNullOrEmpty(sUrl))
                        {
                            try
                            {
                                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(sUrl);
                                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                                if (httpResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    retVal = true;
                                }
                                else
                                {
                                    log.Error("HP Thumbnail ERROR " + sThumb + " AbertisFeeder Abertis Feeder missing thumbnail " + sThumb);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("HP Thumbnail ERROR - " + sThumb + " AbertisFeeder Abertis Feeder missing thumbnail " + sThumb, ex);
                                retVal = false;
                            }
                            finally
                            {
                                if (httpResponse != null)
                                {
                                    httpResponse.Close();
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("HP Thumbnail XML ERROR - " + ex.Message);
                retVal = false;
            }
            return retVal;
        }
    }
}
