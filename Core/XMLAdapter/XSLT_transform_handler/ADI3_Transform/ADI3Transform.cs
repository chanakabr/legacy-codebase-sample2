using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using XMLAdapter;
using System.Xml;
using System.Xml.Xsl;
using System.Configuration;
using System.Data;

namespace XSLT_transform_handlar
{
    public sealed class ADI3_XSLT_Transformer : BaseTransformHandler
    {
        // TODO: later on, read these configuration information fron outside
        string XSL_DEFAULT_FILE = ConfigurationManager.AppSettings["XSL_PATH"].ToString() + "ADI3_transform.xsl";
        string XSL_SERIES_DEFAULT_FILE = ConfigurationManager.AppSettings["XSL_PATH"].ToString() + "ADI3_transform_series.xsl";

        private XslCompiledTransform m_oXsltSeries = new XslCompiledTransform();

        // transform and add the physical url files
        public override void Transform(string pathInputFile, string nameInputFile, StringWriter output)
        {
            using (output)
            {
                WriteBundleID(pathInputFile + nameInputFile);
                m_oXslt.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);

                // convert string to stream
                byte[] byteArray = Encoding.UTF8.GetBytes(output.ToString());
                MemoryStream stream = new MemoryStream(byteArray);

                StreamWriter writer = new StreamWriter(stream);

                AddingPhysicalFileUrls(writer, pathInputFile + nameInputFile, output);
            }
        }

        public override void Transform(XmlDocument inputXml, StringWriter output)
        {
            using (output)
            {
                XmlReader reader = XmlReader.Create(new StringReader(inputXml.InnerXml));
                XmlWriter writer = XmlWriter.Create(output);
                m_oXslt.Transform(reader, writer);
            }
        }

        private void WriteBundleID(string file)
        {
            XmlDocument XMLDocFile = new XmlDocument();
            XMLDocFile.Load(file);

            XmlNodeList bunID = XMLDocFile.GetElementsByTagName("bundleID");
            if (bunID.Count != 0)
            {
                return;
            }

            XmlNodeList movieList = XMLDocFile.GetElementsByTagName("Movie");

            int groupID = int.Parse(ConfigurationManager.AppSettings["PARENT_GROUP_ID"].ToString());
            DataTable dt = DAL.XMLAdapterDAL.Get_GroupLangCodes(groupID);

            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        XmlElement SupportedLanguages = XMLDocFile.CreateElement("SupportedLanguages");
                        string languageCode = ODBCWrapper.Utils.GetSafeStr(dataRow["code3"]);
                        SupportedLanguages.SetAttribute("codeID", languageCode);

                        XmlElement root = XMLDocFile.SelectSingleNode("//*[local-name()='ADI3']") as XmlElement;
                        root.AppendChild(SupportedLanguages);

                        XMLDocFile.Save(@file);
                    }
                }
            }

            foreach (XmlElement el in movieList)
            {
                XmlElement movie_XMLElement = el.SelectSingleNode("*[local-name()='Ext']/*[local-name()='CmsXml']") as XmlElement;
                string stringXML = movie_XMLElement.InnerText;
                stringXML = stringXML.Replace("]]/>", "]]>");
                stringXML = stringXML.Replace(@"]]\>", "]]>");
                stringXML = stringXML.Replace("\n", "");

                XmlDocument movieXML = new XmlDocument();
                movieXML.LoadXml(stringXML);

                XmlElement bandleID_XMLElement = movieXML.SelectSingleNode("//*[local-name()='opencase']/*[local-name()='component']/*[local-name()='uuid']") as XmlElement;
                string bundleID = bandleID_XMLElement.InnerText;

                XmlElement titleID_XMLElement = movieXML.SelectSingleNode("//*[local-name()='opencase']/*[local-name()='component']/*[local-name()='Related_Identifiers']/*[local-name()='IsanVideo']/*[local-name()='identifier']") as XmlElement;
                string titleID = titleID_XMLElement.InnerText;

                XmlElement bundleIDElement = XMLDocFile.CreateElement("bundleID");
                bundleIDElement.SetAttribute("uuID", bundleID);
                bundleIDElement.SetAttribute("TitleID", titleID);

                XmlElement root = XMLDocFile.SelectSingleNode("//*[local-name()='ADI3']") as XmlElement;
                root.AppendChild(bundleIDElement);

                XMLDocFile.Save(@file);
            }

            return;
        }

        public void TransformSeries(string pathInputFile, string nameInputFile, StringWriter output)
        {
            using (output)
            {
                m_oXsltSeries.Transform(pathInputFile + nameInputFile, m_oXslArguments, output);
            }
        }

        private void AddingPhysicalFileUrls(StreamWriter transformedFile, string InputFile, StringWriter output)
        {
            XmlDocument transformedDoc = new XmlDocument();      // the final transformed file
            XmlDocument mediaPhysicalDataDoc = new XmlDocument();      // video file asset
            XmlDocument InputADI3FileDoc = new XmlDocument();

            // open and resources for the job
            //FileStream fs = (FileStream)(transformedFile.BaseStream);
            //transformedFile.Close();
            StreamReader transformedFileReader = new StreamReader(transformedFile.BaseStream);
            transformedDoc.Load(transformedFileReader);
            InputADI3FileDoc.Load(InputFile);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(mediaPhysicalDataDoc.NameTable); // Set namespace for the xml files
            nsmgr.AddNamespace("cm", "http://opencase.extend.com/cm");
            nsmgr.AddNamespace("offer", "http://www.cablelabs.com/namespaces/metadata/xsd/offer/1");

            XmlNodeList contentGroupList = InputADI3FileDoc.GetElementsByTagName("ContentGroup");  // bandle all the content groups
            XmlNodeList movieList = InputADI3FileDoc.GetElementsByTagName("Movie");         // bandle all the movies

            // iterate all the content groups and set all the physical data info
            foreach (XmlNode node in contentGroupList)
            {
                try
                {
                    // we get all the search parameters
                    XmlElement groupElement = node.SelectSingleNode("offer:TitleRef", nsmgr) as XmlElement;
                    string mediaGUID = groupElement.GetAttribute("uriId");
                    groupElement = node.SelectSingleNode("offer:MovieRef", nsmgr) as XmlElement;
                    string movieGUID = groupElement.GetAttribute("uriId");

                    // get the right media element from the transformed xml file
                    string sMediaElementExpression = "//media[@co_guid = '" + mediaGUID + "']";
                    XmlElement mediaElement = transformedDoc.SelectSingleNode(sMediaElementExpression) as XmlElement;

                    // get the right movie asset from the ADI3 input file
                    XmlNode movieElement = null;
                    foreach (XmlNode movieNode in movieList)
                    {
                        if ((movieNode as XmlElement).GetAttribute("uriId") == movieGUID)
                        {
                            movieElement = movieNode;
                            break;
                        }
                    }

                    // if there is no movie or if there is no media element continue to the next content group
                    if (movieElement == null || mediaElement == null)
                    {
                        continue;
                    }

                    // get all the movie physical information and load it to an xml document
                    string fileXMLData = (((movieElement).FirstChild).FirstChild).InnerText;
                    fileXMLData = fileXMLData.Replace(System.Environment.NewLine, "");

                    fileXMLData = fileXMLData.Replace("]]/>", "]]>");
                    fileXMLData = fileXMLData.Replace(@"]]\>", "]]>");
                    fileXMLData = fileXMLData.Replace("\n", "");

                    XmlDocument VideoFilesXML = new XmlDocument();
                    VideoFilesXML.LoadXml(fileXMLData);

                    // collect all the physical Asset videos and fill in a batch under Files xml element
                    XmlNodeList elemList = VideoFilesXML.SelectNodes("//*[local-name() = 'PhysicalAssetManifest']");
                    XmlElement root = transformedDoc.CreateElement("files");

                    // fill the relevent data to the xml document
                    foreach (XmlNode VideoFilesNodes in elemList)
                    {
                        string targetDeviceName = String.Empty;
                        XmlElement el = VideoFilesNodes as XmlElement;

                        string uuid = (((el.GetElementsByTagName("cm:uuid"))[0] as XmlElement)).InnerText;
                        string cdnCode = (((el.GetElementsByTagName("cm:url"))[0] as XmlElement)).InnerText;

                        {
                            if (cdnCode.Contains("TABLET"))
                            {
                                if (cdnCode.Contains("_RUS"))
                                {
                                    targetDeviceName = "Tablet Main RU";
                                }
                                else
                                {
                                    targetDeviceName = "Tablet Main";
                                }
                            }
                            else if (cdnCode.Contains("PC"))
                            {
                                targetDeviceName = "PC Main";
                            }
                            else if (cdnCode.Contains("PHONE"))
                            {
                                if (cdnCode.Contains("_RUS"))
                                {
                                    targetDeviceName = "Phone Main RU";
                                }
                                else
                                {
                                    targetDeviceName = "PHONE Main";
                                }
                            }
                            else
                            {
                                // skip all the nodes without type info
                                continue;
                            }

                            //XmlNodeList TargetDevice = el.SelectNodes("cm:targetDevices/cm:targetDevice/cm:name", nsmgr);
                            //if (TargetDevice.Count != 0)
                            //{
                            //    targetDeviceName = (TargetDevice[0] as XmlElement).InnerText;
                            //    if (targetDeviceName == "pc")
                            //    {
                            //        targetDeviceName = "PC Main";
                            //    }
                            //    else if (targetDeviceName == "tablet")
                            //    {
                            //        targetDeviceName = "Tablet Main";
                            //    }
                            //}
                            //else
                            //{
                            //    // skip all the nodes without type info
                            //    continue;
                            //}
                        }

                        XmlElement fileElement = transformedDoc.CreateElement("file");
                        fileElement.SetAttribute("type", targetDeviceName);
                        fileElement.SetAttribute("quality", "HIGH");
                        fileElement.SetAttribute("pre_rule", "null");
                        fileElement.SetAttribute("post_rule", "null");
                        fileElement.SetAttribute("handling_type", "CLIP");
                        fileElement.SetAttribute("duration", "0");
                        fileElement.SetAttribute("cdn_name", "Direct Link");
                        fileElement.SetAttribute("cdn_code", cdnCode);
                        fileElement.SetAttribute("break_rule", "null");
                        fileElement.SetAttribute("break_points", "");
                        fileElement.SetAttribute("billing_type", "");
                        fileElement.SetAttribute("assetwidth", "");
                        fileElement.SetAttribute("assetheight", "");
                        fileElement.SetAttribute("assetduration", "0");
                        fileElement.SetAttribute("ppv_module", "");
                        fileElement.SetAttribute("co_guid", uuid);

                        root.AppendChild(fileElement);
                    }

                    mediaElement.AppendChild(root);
                }
                catch
                {
                    // error handling content group data, continue to the next one
                    continue;
                }
            } // end loop continue to the next content group

            // close the reader and save the transformed output file
            StringBuilder sb = output.GetStringBuilder();
            sb.Remove(0, sb.Length);
            transformedDoc.Save(output);
            transformedFileReader.Close();
        }

        public void SetOfferNumber(int offerNumber)
        {
            ((ADI3Adapter)m_oAdapter).SetOfferNumber(offerNumber);
        }

        public override void Init()
        {
            // init the adapter
            base.m_oAdapter = new ADI3Adapter();
            m_oAdapter.Init();
            m_oXslArguments.AddExtensionObject("pda:ADIUtils", m_oAdapter as BaseXMLAdapter);

            m_oXslt.Load(XSL_DEFAULT_FILE);
            m_oXsltSeries.Load(XSL_SERIES_DEFAULT_FILE);

            base.Init();  // Init base
        }

        public void LoadXslFile(string path, string XslfileName)
        {
            // Load different XSLT file
            m_oXslt.Load(path + XslfileName);
        }
    }
}
