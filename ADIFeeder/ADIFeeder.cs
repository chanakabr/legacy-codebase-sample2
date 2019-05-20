using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TVinciShared;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Configuration;
using System.Xml.Serialization;
using KLogMonitor;
using System.Reflection;

namespace ADIFeeder
{
    public class ADIFeeder : ScheduledTasks.BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public struct FileStruct
        {
            public string m_url;
            public string m_duration;
            public string m_CDN;
            public string m_id;
            public string m_PreProviderID;
            public string m_BreakProviderID;
            public string m_OverlayProviderID;
            public string m_PostProviderID;
            public string m_Breakpoints;
            public string m_Overlaypoints;
            public string m_ProductCode;

            public FileStruct(string id, string url, string duration, string CDN, string preProviderID, string breakProviderID, string overlayProviderID, string postProviderID, string breakpoints, string overlaypoints, string productCode)
            {
                m_url = url;
                m_id = id;
                m_CDN = CDN;
                m_duration = duration;
                m_PreProviderID = preProviderID;
                m_BreakProviderID = breakProviderID;
                m_OverlayProviderID = overlayProviderID;
                m_PostProviderID = postProviderID;
                m_Breakpoints = breakpoints;
                m_Overlaypoints = overlaypoints;
                m_ProductCode = productCode;
            }
        }

        public Dictionary<string, string> m_basicsDict;
        public Dictionary<string, string> m_stringMetaDict;
        public Dictionary<string, string> m_numMetaDict;
        public Dictionary<string, string> m_boolMetaDict;
        public Dictionary<string, List<string>> m_tagsDict;
        public Dictionary<string, FileStruct> m_filesDict;
        public Dictionary<string, string> m_picRatios;
        public List<string> m_virtualTags;
        public List<string> m_regularTags;

        public const string GEO_BLOCK_RULE_KEY = "Geo Block Rule";
        public const string DEVICE_RULE_KEY = "Device Rule";

        private string m_sResXml;

        private int m_nGroupID = 0;
        private string m_sXML;
        public ADIFeeder(Int32 nTaskID, Int32 nIntervalInSec, string engrameters, int groupID)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            m_sXML = engrameters;
            m_nGroupID = groupID;
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters, int groupID)
        {
            return new ADIFeeder(nTaskID, nIntervalInSec, engrameters, groupID);
        }

        private bool isCreateSeriesXML()
        {
            string sMediaType = GetMetaValue(m_basicsDict, "mediatype");

            if (!sMediaType.ToLower().Equals("news") &&
                !sMediaType.ToLower().Equals("education") &&
                !sMediaType.ToLower().Equals("episode"))
            {
                return false;
            }

            // check if series exist, if so, we don't have to create it again
            if (!isSeriesExist(GetTagValueByIndex(m_tagsDict, "Series name", 0), 149))
            {
                // check if first episode or if it premiere, only in these cases we create media series
                if (GetMetaValue(m_numMetaDict, "Episode number") == "1" || GetMetaValue(m_numMetaDict, "Season Premiere") == "1")
                {
                    return true;
                }
            }

            return false;
        }

        private string GetRegularMediaName()
        {
            string sName = string.Empty;
            if (m_nGroupID == 148)
            {
                string sMediaType = GetMetaValue(m_basicsDict, "mediatype");

                if (sMediaType.ToLower().Equals("episode"))
                {
                    string seriesId = GetTagValueByIndex(m_tagsDict, "Series name", 0);
                    string episodeID = GetMetaValue(m_numMetaDict, "episode id");

                    sName = string.Format("{0} -  Episode  {1}", seriesId, episodeID);
                }
                else if (sMediaType.ToLower().Equals("news") || sMediaType.ToLower().Equals("education"))
                {
                    string seriesId = GetTagValueByIndex(m_tagsDict, "Series name", 0);
                    string episodeName = GetMetaValue(m_stringMetaDict, "Episode name");

                    sName = string.Format("{0} - {1}", seriesId, episodeName);

                }
                else if (sMediaType.ToLower().Equals("extra"))
                {
                    sName = GetMetaValue(m_stringMetaDict, "Episode name");
                }
                else
                {
                    sName = GetMetaValue(m_basicsDict, "name");
                }

            }
            else if (m_nGroupID == 149)
            {
                sName = GetTagValueByIndex(m_tagsDict, "Series name", 0);
                updateSeriesCoGuid(sName, GetMetaValue(m_basicsDict, "co_guid"), 149);
            }

            return sName;
        }

        private void InitTvinciXMLObject(ref ADIFeederXSD.feed CmediaFeed)
        {
            CmediaFeed.export = new ADIFeederXSD.feedExport();
            CmediaFeed.export.media = new ADIFeederXSD.feedExportMedia();
            CmediaFeed.export.media.basic = new ADIFeederXSD.feedExportMediaBasic();
            CmediaFeed.export.media.basic.name = new ADIFeederXSD.feedExportMediaBasicName();
            CmediaFeed.export.media.basic.name.value = new ADIFeederXSD.feedExportMediaBasicNameValue();

            CmediaFeed.export.media.basic.description = new ADIFeederXSD.feedExportMediaBasicDescription();
            CmediaFeed.export.media.basic.description.value = new ADIFeederXSD.feedExportMediaBasicDescriptionValue();
            CmediaFeed.export.media.basic.thumb = new ADIFeederXSD.feedExportMediaBasicThumb();
            CmediaFeed.export.media.basic.rules = new ADIFeederXSD.feedExportMediaBasicRules();
            CmediaFeed.export.media.basic.dates = new ADIFeederXSD.feedExportMediaBasicDates();
            CmediaFeed.export.media.basic.pic_ratios = new ADIFeederXSD.feedExportMediaBasicPic_ratios();
            CmediaFeed.export.media.basic.pic_ratios.ratio = new ADIFeederXSD.feedExportMediaBasicPic_ratiosRatio();
        }

        private string BuildNormalizedXML(bool isSeries)
        {
            ADIFeederXSD.feed CmediaFeed = new ADIFeederXSD.feed();

            InitTvinciXMLObject(ref CmediaFeed);

            CmediaFeed.export.media.co_guid = GetMetaValue(m_basicsDict, "co_guid");
            CmediaFeed.export.media.action = "insert";

            string sIsActive = GetMetaValue(m_basicsDict, "Active");
            if (string.IsNullOrEmpty(sIsActive))
            {
                CmediaFeed.export.media.is_active = true;
            }
            else
            {
                CmediaFeed.export.media.is_active = sIsActive.ToLower().Equals("true") ? true : false;
            }

            CmediaFeed.export.media.basic.name.value.Value = isSeries == false ? GetRegularMediaName() : GetTagValueByIndex(m_tagsDict, "Series name", 0);
            CmediaFeed.export.media.basic.name.value.lang = "eng";
            string sDescription = GetMetaValue(m_basicsDict, "description");

            if (string.IsNullOrEmpty(sDescription))
            {
                sDescription = GetMetaValue(m_stringMetaDict, "Short summary");
            }

            CmediaFeed.export.media.basic.description.value.Value = sDescription;
            CmediaFeed.export.media.basic.description.value.lang = "eng";

            if (isSeries == false)
            {
                CmediaFeed.export.media.basic.media_type = GetMetaValue(m_basicsDict, "mediatype");
            }
            else
            {
                switch (GetMetaValue(m_basicsDict, "mediatype"))
                {
                    case "Episode":
                        {
                            CmediaFeed.export.media.basic.media_type = "Series";
                            break;
                        }
                    case "News":
                        {
                            CmediaFeed.export.media.basic.media_type = "News Series";
                            break;
                        }
                    case "Education":
                        {
                            CmediaFeed.export.media.basic.media_type = "Education Series";
                            break;
                        }
                    default:
                        {
                            CmediaFeed.export.media.basic.media_type = string.Empty;
                            break;
                        }
                }
            }

            CmediaFeed.export.media.basic.thumb.url = GetMetaValue(m_basicsDict, "thumb");
            CmediaFeed.export.media.basic.epg_identifier = 0;
            CmediaFeed.export.media.basic.rules.watch_per_rule = "Parent allowed";
            CmediaFeed.export.media.basic.rules.geo_block_rule = GetMetaValue(m_basicsDict, GEO_BLOCK_RULE_KEY);
            CmediaFeed.export.media.basic.rules.device_rule = GetMetaValue(m_basicsDict, DEVICE_RULE_KEY);
            CmediaFeed.export.media.basic.dates.start = GetMetaValue(m_basicsDict, "start");
            CmediaFeed.export.media.basic.dates.catalog_end = GetMetaValue(m_basicsDict, "end");
            CmediaFeed.export.media.basic.dates.final_end = GetMetaValue(m_basicsDict, "final");

            foreach (KeyValuePair<string, string> kvp in m_picRatios)
            {
                CmediaFeed.export.media.basic.pic_ratios.ratio.ratio = kvp.Key;
                CmediaFeed.export.media.basic.pic_ratios.ratio.thumb = kvp.Value;
            }

            List<ADIFeederXSD.feedExportMediaStructureMeta> ExportMediaStringList = new List<ADIFeederXSD.feedExportMediaStructureMeta>();
            foreach (KeyValuePair<string, string> kvp in m_stringMetaDict)
            {
                ADIFeederXSD.feedExportMediaStructureMeta currStringMeta = new ADIFeederXSD.feedExportMediaStructureMeta();
                currStringMeta.name = kvp.Key;
                currStringMeta.value = new ADIFeederXSD.feedExportMediaStructureMetaValue();
                currStringMeta.value.lang = "eng";
                currStringMeta.ml_handling = "duplicate";
                currStringMeta.value.Value = kvp.Value;

                ExportMediaStringList.Add(currStringMeta);
            }

            List<ADIFeederXSD.feedExportMediaStructureMeta1> ExportMediaBoolList = new List<ADIFeederXSD.feedExportMediaStructureMeta1>();
            foreach (KeyValuePair<string, string> kvp in m_boolMetaDict)
            {
                ADIFeederXSD.feedExportMediaStructureMeta1 currBoolMeta = new ADIFeederXSD.feedExportMediaStructureMeta1();
                currBoolMeta.name = kvp.Key;
                currBoolMeta.Value = kvp.Value;

                ExportMediaBoolList.Add(currBoolMeta);
            }

            List<ADIFeederXSD.feedExportMediaStructureMeta2> ExportMediaNumList = new List<ADIFeederXSD.feedExportMediaStructureMeta2>();
            foreach (KeyValuePair<string, string> kvp in m_numMetaDict)
            {
                double dRes;
                bool res = double.TryParse(kvp.Value, out dRes);
                if (res)
                {
                    ADIFeederXSD.feedExportMediaStructureMeta2 currNumMeta = new ADIFeederXSD.feedExportMediaStructureMeta2();
                    currNumMeta.name = kvp.Key;
                    currNumMeta.Value = dRes;
                    ExportMediaNumList.Add(currNumMeta);
                }
            }

            Dictionary<string, List<string>> metaTags = new Dictionary<string, List<string>>();
            List<ADIFeederXSD.feedExportMediaStructureMeta3> ExportTagsNumList = new List<ADIFeederXSD.feedExportMediaStructureMeta3>();
            foreach (KeyValuePair<string, List<string>> kvp in m_tagsDict)
            {
                // validate tag in group
                bool isTagExist = IsTagExist(isSeries, kvp.Key);

                if (!isTagExist)
                {
                    string logMassage = string.Format("Group tag: {0} doesn't exist", kvp.Key);
                    log.Debug("Tags Validation - "+ logMassage);
                    continue;
                }

                ADIFeederXSD.feedExportMediaStructureMeta3 currTag = new ADIFeederXSD.feedExportMediaStructureMeta3();
                currTag.name = kvp.Key;
                currTag.ml_handling = "unique";

                List<ADIFeederXSD.feedExportMediaStructureMetaContainer> conList = new List<ADIFeederXSD.feedExportMediaStructureMetaContainer>();
                foreach (string value in kvp.Value.Distinct())
                {
                    ADIFeederXSD.feedExportMediaStructureMetaContainer currCon = new ADIFeederXSD.feedExportMediaStructureMetaContainer();
                    currCon.value = new ADIFeederXSD.feedExportMediaStructureMetaContainerValue();
                    currCon.value.lang = "eng";
                    currCon.value.Value = value;

                    conList.Add(currCon);
                }

                currTag.container = conList.ToArray();
                ExportTagsNumList.Add(currTag);
            }

            CmediaFeed.export.media.structure = new ADIFeederXSD.feedExportMediaStructure();

            CmediaFeed.export.media.structure.strings = ExportMediaStringList.ToArray();
            CmediaFeed.export.media.structure.booleans = ExportMediaBoolList.ToArray();
            CmediaFeed.export.media.structure.doubles = ExportMediaNumList.ToArray();
            CmediaFeed.export.media.structure.metas = ExportTagsNumList.ToArray();

            if (isSeries == false)
            {
                List<ADIFeederXSD.feedExportMediaFile> ExFileList = new List<ADIFeederXSD.feedExportMediaFile>();
                foreach (KeyValuePair<string, FileStruct> kvp in m_filesDict)
                {
                    ADIFeederXSD.feedExportMediaFile currFileList = new ADIFeederXSD.feedExportMediaFile();
                    string ppvModule = GetMetaValue(m_stringMetaDict, "PPVModule");
                    string DTWppvModule = string.Empty;

                    string sDTWProduct = GetMetaValue(m_stringMetaDict, "DTW Product");
                    string sDTWBillingCode = GetMetaValue(m_stringMetaDict, "DTW Billing Code");
                    if (!string.IsNullOrEmpty(sDTWProduct) && !string.IsNullOrEmpty(sDTWBillingCode))
                    {
                        DTWppvModule = string.Format("{0}{1}", sDTWProduct, sDTWBillingCode);
                    }

                    currFileList.handling_type = "Clip";
                    long duration;
                    if (long.TryParse(kvp.Value.m_duration, out duration))
                    {
                        currFileList.assetDuration = duration;
                    }
                    currFileList.type = kvp.Key;
                    currFileList.quality = "HIGH";
                    currFileList.billing_type = "Tvinci";
                    currFileList.cdn_name = string.IsNullOrEmpty(kvp.Value.m_CDN) ? "Direct Link" : kvp.Value.m_CDN;
                    currFileList.cdn_code = kvp.Value.m_url;
                    currFileList.co_guid = kvp.Value.m_id;
                    currFileList.pre_rule = kvp.Value.m_PreProviderID;
                    currFileList.break_rule = kvp.Value.m_BreakProviderID;
                    currFileList.overlay_rule = kvp.Value.m_OverlayProviderID;
                    currFileList.post_rule = kvp.Value.m_PostProviderID;
                    currFileList.break_points = kvp.Value.m_Breakpoints;
                    currFileList.overlay_points = kvp.Value.m_Overlaypoints;
                    currFileList.product_code = kvp.Value.m_ProductCode;

                    if (!kvp.Key.ToLower().Equals("download main") && !kvp.Key.ToLower().Equals("Download High"))
                    {
                        currFileList.ppv_module = ppvModule;
                    }
                    else
                    {
                        currFileList.ppv_module = DTWppvModule;
                    }

                    if ((string.IsNullOrEmpty(ppvModule) && string.IsNullOrEmpty(DTWppvModule))
                        || ppvModule.Equals("PREV99990")
                        || kvp.Key.ToLower().Equals("trailer"))
                    {
                        currFileList.billing_type = "none";
                    }

                    ExFileList.Add(currFileList);
                }

                CmediaFeed.export.media.files = ExFileList.ToArray();
            }

            string theXML = string.Empty;
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ADIFeederXSD.feed));
                    serializer.Serialize(sw, CmediaFeed);
                    theXML = sw.ToString();
                }
            }
            catch
            {
            }

            return theXML;
        }

        public string sStartDate = string.Empty;
        public string sEndDate = string.Empty;
        public string sFinalEndDate = string.Empty;
        public string sDescription = string.Empty;
        public string sDuration = string.Empty;
        public string sMediaType = string.Empty;
        public string sBreakpoints = string.Empty;
        public string assetProduct = string.Empty;
        public string sAssetType = string.Empty;
        public string sPCode = string.Empty;

        private ADIMapping m_ADIMapItems = new ADIMapping();

        protected override bool DoTheTaskInner()
        {
            m_stringMetaDict = new Dictionary<string, string>();
            m_numMetaDict = new Dictionary<string, string>();
            m_boolMetaDict = new Dictionary<string, string>();
            m_tagsDict = new Dictionary<string, List<string>>();
            m_basicsDict = new Dictionary<string, string>();
            m_filesDict = new Dictionary<string, FileStruct>();
            m_picRatios = new Dictionary<string, string>();
            m_virtualTags = new List<string>();
            m_regularTags = new List<string>();

            SetupAllTagsValues();

            int nGroupID = m_nGroupID;

            m_sResXml = string.Empty;

            if (m_nGroupID != 0 && !string.IsNullOrEmpty(m_sXML))
            {
                XmlDocument theDoc = new XmlDocument();
                XmlNodeList adiNodes = theDoc.SelectNodes("/ADI");

                theDoc.LoadXml(m_sXML);

                for (int i = 0; i < adiNodes.Count; i++)
                {
                    XmlNode adiNode = adiNodes[i];
                    string sCoGuid = XmlUtils.GetNodeParameterVal(ref adiNode, "Metadata/AMS", "Asset_ID");
                    string sProvider = XmlUtils.GetNodeParameterVal(ref adiNode, "Metadata/AMS", "Provider");
                    string sPrviderID = XmlUtils.GetNodeParameterVal(ref adiNode, "Metadata/AMS", "Provider_ID");

                    m_basicsDict.Add("co_guid", sCoGuid);
                    m_tagsDict.Add("Provider", sProvider.Split(';').ToList<string>());
                    m_tagsDict.Add("Provider ID", sPrviderID.Split(';').ToList<string>());

                    XmlNodeList assetsList = adiNode.SelectNodes("Asset");
                    if (assetsList != null)
                    {
                        for (int j = 0; j < assetsList.Count; j++)
                        {
                            XmlNode assetNode = assetsList[j];
                            sAssetType = XmlUtils.GetNodeParameterVal(ref assetNode, "Metadata/AMS", "Asset_Class");
                            assetProduct = XmlUtils.GetNodeParameterVal(ref assetNode, "Metadata/AMS", "Product");
                            XmlNode metaNode = assetNode.SelectSingleNode("Metadata");
                            XmlNodeList appDataList = metaNode.SelectNodes("App_Data");

                            if (!string.IsNullOrEmpty(assetProduct))
                            {
                                AddTag("Product", assetProduct);
                            }


                            if (sAssetType.ToLower().Equals("title"))
                            {
                                for (int k = 0; k < appDataList.Count; k++)
                                {
                                    XmlNode appDataNode = appDataList[k];
                                    string sName = XmlUtils.GetItemParameterVal(ref appDataNode, "Name").ToLower();
                                    string sValue = XmlUtils.GetItemParameterVal(ref appDataNode, "Value");

                                    if (sValue == null)
                                    {
                                        continue;
                                    }

                                    if (m_ADIMapItems.lMapItems.ContainsKey(sName))
                                    {
                                        string key = m_ADIMapItems.lMapItems[sName].m_key;
                                        m_ADIMapItems.lMapItems[sName].m_method(this, sValue, key);
                                    }
                                }

                                sStartDate = string.Empty;

                                if (!m_numMetaDict.ContainsKey(""))
                                {
                                }
                            }

                            sMediaType = GetTVMMediaType(sMediaType, ref nGroupID);
                            m_nGroupID = nGroupID;
                            m_basicsDict.Add("mediatype", sMediaType);

                            XmlNodeList fileNodes = assetNode.SelectNodes("Asset");
                            if (fileNodes != null && fileNodes.Count > 0)
                            {
                                string[] durationArr = sDuration.Split(':');
                                TimeSpan ts = new TimeSpan(int.Parse(durationArr[0]), int.Parse(durationArr[1]), int.Parse(durationArr[2]));

                                for (int f = 0; f < fileNodes.Count; f++)
                                {
                                    XmlNode fileNode = fileNodes[f];
                                    XmlNode fileMetaNode = fileNode.SelectSingleNode("Metadata");
                                    string assetID = XmlUtils.GetNodeParameterVal(ref fileNode, "Metadata/AMS", "Asset_ID");
                                    string assetType = XmlUtils.GetNodeParameterVal(ref fileNode, "Metadata/AMS", "Asset_Class");
                                    string assetName = XmlUtils.GetNodeParameterVal(ref fileNode, "Metadata/AMS", "Asset_Name");
                                    string fileURL = XmlUtils.GetNodeParameterVal(ref fileNode, "Content", "Value");

                                    string cdn = string.Empty;

                                    if (assetType.ToLower().Equals("encrypted") && nGroupID == 149 ||
                                        assetType.ToLower().Equals("movie") && nGroupID == 149 ||
                                        assetType.ToLower().Equals("preview") && nGroupID == 149)
                                    {
                                        continue;
                                    }

                                    XmlNodeList fileAppData = fileMetaNode.SelectNodes("App_Data");
                                    int prevDurationSec = 0;
                                    if (fileAppData != null && fileAppData.Count > 0)
                                    {
                                        SetMetasFromFilesContant(fileAppData, ref prevDurationSec, ref cdn);
                                    }

                                    int durationSec = (int)ts.TotalSeconds;
                                    string fileType = string.Empty;
                                    sPCode = string.Empty;
                                    switch (assetType.ToLower())
                                    {
                                        case "encrypted":
                                            {
                                                fileType = GetFileType(assetID);
                                                break;
                                            }
                                        case "movie":
                                            {
                                                fileType = "iOS Clear";
                                                break;
                                            }
                                        case "preview":
                                            {
                                                fileType = "Trailer";
                                                durationSec = prevDurationSec;
                                                break;
                                            }
                                        case "poster":
                                            {
                                                m_picRatios.Add("3:4", fileURL);
                                                continue;
                                            }
                                        case "box cover":
                                            {
                                                m_basicsDict.Add("thumb", fileURL);
                                                //m_picRatios.Add("16:9", fileURL);
                                                continue;
                                            }
                                    }

                                    FileStruct file = createFileStruct(assetID, fileURL, durationSec.ToString(), cdn, sBreakpoints, string.Empty, true, sPCode);
                                    if (!m_filesDict.ContainsKey(fileType))
                                    {
                                        m_filesDict.Add(fileType, file);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string normXml = BuildNormalizedXML(false);
            string seriesXML = string.Empty;
            if (isCreateSeriesXML())
            {
                seriesXML = BuildNormalizedXML(true);
            }

            bool retVal = false;
            if (!string.IsNullOrEmpty(normXml))
            {
                m_sResXml = string.Empty;
                retVal = TvinciImporter.ImporterImpl.DoTheWorkInner(normXml, nGroupID, string.Empty, ref m_sResXml, false);
                if (!string.IsNullOrEmpty(seriesXML))
                {
                    string refXml = string.Empty;
                    TvinciImporter.ImporterImpl.DoTheWorkInner(seriesXML, 149, string.Empty, ref refXml, false);
                }

                ADDIngestToDBAndSaveFiles(m_sXML, normXml, m_sResXml);
            }
            return retVal;
        }

        private void SetMetasFromFilesContant(XmlNodeList fileAppData, ref int prevDurationSec, ref string cdn)
        {
            for (int t = 0; t < fileAppData.Count; t++)
            {
                XmlNode fileAppDataNode = fileAppData[t];
                string sFileNameData = XmlUtils.GetItemParameterVal(ref fileAppDataNode, "Name");
                string sFileValueData = XmlUtils.GetItemParameterVal(ref fileAppDataNode, "Value");

                switch (sFileNameData.ToLower())
                {
                    case "run_time":
                        {
                            string[] prevDurArr = sFileValueData.Split(':');
                            TimeSpan prevTS = new TimeSpan(int.Parse(prevDurArr[0]), int.Parse(prevDurArr[1]), int.Parse(prevDurArr[2]));
                            prevDurationSec = (int)prevTS.TotalSeconds;
                            break;
                        }
                    case "cdn_name":
                        {
                            cdn = sFileValueData;
                            break;
                        }
                    case "languages":
                        {
                            string[] isoLangsArr = sFileValueData.Split(';');
                            if (isoLangsArr != null && isoLangsArr.Length > 0)
                            {
                                foreach (string str in isoLangsArr)
                                {
                                    string langStr = getFullLanguageStr(str);

                                    if (!m_tagsDict.ContainsKey("Audio language"))
                                    {
                                        AddTag("Audio language", langStr);
                                    }
                                    else if (!m_tagsDict["Audio language"].Contains(langStr))
                                    {
                                        AddTag("Audio language", langStr);
                                    }
                                }
                            }
                            break;
                        }
                    case "subtitle_languages":
                        {
                            string[] isoLangsArr = sFileValueData.Split(';');
                            if (isoLangsArr != null && isoLangsArr.Length > 0)
                            {
                                foreach (string str in isoLangsArr)
                                {
                                    string langStr = getFullLanguageStr(str);

                                    if (!m_tagsDict.ContainsKey("Subtitle language"))
                                    {
                                        AddTag("Subtitle language", langStr);
                                    }
                                    else if (!m_tagsDict["Subtitle language"].Contains(langStr))
                                    {
                                        AddTag("Subtitle language", langStr);
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private FileStruct createFileStruct(string assetID, string fileURL, string durationInSec, string cdn, string breakpoints, string overlaypoints, bool isPutAdDetails, string pCode)
        {
            string adProvider;
            if (!isPutAdDetails)
                return new FileStruct(assetID, fileURL, durationInSec, cdn, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            adProvider = ConfigurationManager.AppSettings["ADIFeederDefaultAdProvider"];
            if (adProvider == null)
                adProvider = string.Empty;
            return new FileStruct(assetID, fileURL, durationInSec, cdn, adProvider, adProvider, adProvider, adProvider, breakpoints, overlaypoints, pCode);
        }

        private void ADDIngestToDBAndSaveFiles(string sOrigXML, string sNormXML, string sResponseXML)
        {
            log.Debug("AddIngestToFTP - Start: Original: " + sOrigXML + " Normalized :" + sNormXML + " Response :" + sResponseXML);
            DateTime createDate = DateTime.UtcNow;

            int ingestID = IngestionUtils.InsertIngestToDB(createDate, 2, m_nGroupID);


            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sResponseXML);
            XmlNode xn = doc.FirstChild;
            string mediaID = XmlUtils.GetItemParameterVal(ref xn, "tvm_id");
            string status = XmlUtils.GetItemParameterVal(ref xn, "status");
            string coGuid = XmlUtils.GetItemParameterVal(ref xn, "co_guid");
            log.Debug("AddIngestToFTP - After Parse");
            int nTVMID = 0;
            if (!string.IsNullOrEmpty(mediaID))
            {
                nTVMID = int.Parse(mediaID);
            }

            IngestionUtils.InsertIngestMediaData(ingestID, nTVMID, coGuid, status);

            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            files.Add("original.xml", IngestionUtils.StringToBytes(sOrigXML));
            files.Add("normalized.xml", IngestionUtils.StringToBytes(sNormXML));
            log.Debug("AddIngestToFTP - After Add To Dict");
            IngestionUtils.UploadIngestToFTP(ingestID, files);
            log.Debug("AddIngestToFTP - End");
        }

        private string getFullLanguageStr(string isoLangStr)
        {
            string retVal = string.Empty;
            switch (isoLangStr.ToUpper())
            {
                case ("EN"):
                    {
                        retVal = "English";
                        break;
                    }
                case ("ZH"):
                    {
                        retVal = "Chinese";
                        break;
                    }
                case ("TA"):
                    {
                        retVal = "Tamil";
                        break;
                    }
                case ("MS"):
                    {
                        retVal = "Malay";
                        break;
                    }
                case ("ID"):
                    {
                        retVal = "Indonesian";
                        break;
                    }
                case ("HI"):
                    {
                        retVal = "Hindi";
                        break;
                    }
                case ("BN"):
                    {
                        retVal = "Bengali";
                        break;
                    }
                case ("GU"):
                    {
                        retVal = "Gujarati";
                        break;
                    }
                case ("PA"):
                    {
                        retVal = "Punjabi";
                        break;
                    }
                case ("ML"):
                    {
                        retVal = "Malayalam";
                        break;
                    }
                case ("JA"):
                    {
                        retVal = "Japanese";
                        break;
                    }
                case ("KO"):
                    {
                        retVal = "Korean";
                        break;
                    }
                case ("TH"):
                    {
                        retVal = "Thai";
                        break;
                    }
                case ("TL"):
                    {
                        retVal = "Tagalog";
                        break;
                    }
                default:
                    {
                        retVal = isoLangStr;
                        break;
                    }
            }

            return retVal;
        }

        private string GetFileType(string contentType)
        {
            string retVal = string.Empty;
            string targetPlatform = contentType.Substring(4, 1);
            if (!string.IsNullOrEmpty(targetPlatform))
            {
                switch (targetPlatform)
                {
                    case ("0"):
                        {
                            retVal = "STB Main";
                            break;
                        }
                    case ("1"):
                        {
                            retVal = "Main";
                            break;
                        }
                    case ("2"):
                        {
                            retVal = "iPhone Main";
                            sPCode = GetMetaValue(m_stringMetaDict, "Product Code");
                            break;
                        }
                    case ("3"):
                        {
                            retVal = "iPad Main";
                            sPCode = GetMetaValue(m_stringMetaDict, "Product Code");
                            break;
                        }
                    case ("4"):
                        {
                            retVal = "Download Main";
                            break;
                        }
                    case ("5"):
                        {
                            retVal = "Download High";
                            break;
                        }
                    case ("6"):
                        {
                            retVal = "Android";
                            break;
                        }
                    default:
                        break;

                }

            }
            return retVal;
        }

        private bool isSeriesExist(string seriesName, int virtualGroupID)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", seriesName);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", virtualGroupID);
            selectQuery += " and status = 1";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = true;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        private void updateSeriesCoGuid(string seriesName, string sCoGuid, int virtualGroupID)
        {
            int mediaID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select id from media where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", seriesName);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", virtualGroupID);
            selectQuery += " and status = 1";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    mediaID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (mediaID > 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", "=", sCoGuid);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", mediaID);
                updateQuery += "and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", virtualGroupID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        private string parseDateTimeSTR(string val)
        {
            string retVal = string.Empty;
            string[] dateTimeArr = val.Split('T');
            string timeStr = string.Empty;
            string yearStr = string.Empty;
            if (dateTimeArr != null && dateTimeArr.Length > 0)
            {
                string dateStr = dateTimeArr[0];
                string[] dateArr = dateStr.Split('-');
                if (dateArr != null && dateArr.Length == 3)
                {
                    string year = dateArr[0];
                    string month = dateArr[2];
                    string day = dateArr[1];
                    yearStr = string.Format("{0}/{1}/{2}", day, month, year);
                }
                if (dateTimeArr.Length > 1)
                {
                    timeStr = dateTimeArr[1];

                }
                retVal = string.Format("{0} {1}", yearStr, timeStr);
            }
            return retVal;
        }

        private string GetTVMMediaType(string mediaType, ref int groupID)
        {
            switch (mediaType.ToLower())
            {
                case "movie":
                    {
                        groupID = 148;
                        return "Movie";
                    }
                case "series":
                    {
                        groupID = 148;
                        return "Episode";
                    }
                case "extra":
                    {
                        groupID = 148;
                        return "Extra";
                    }
                case "news":
                    {
                        groupID = 148;
                        return "News";
                    }
                case "education":
                    {
                        groupID = 148;
                        return "Education";
                    }
                case "clip":
                    {
                        groupID = 148;
                        return "Clip";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private string GetMetaValue(Dictionary<string, string> dic, string sKey)
        {
            string ret = string.Empty;

            // check if dictinary contains key, if it does'nt return empty string

            ret = dic.ContainsKey(sKey) == true ? dic[sKey] : string.Empty;

            return ret;
        }

        private List<string> GetTagValues(Dictionary<string, List<string>> dic, string sKey)
        {
            List<string> ret = null;

            // check if dictinary contains key, if it does'nt return null
            ret = dic.ContainsKey(sKey) == true ? dic[sKey] : null;

            return ret;
        }

        private string GetTagValueByIndex(Dictionary<string, List<string>> dic, string sKey, int index)
        {
            // check if dictinary contains key, if it does return its value by index provided
            List<string> tagsList = dic.ContainsKey(sKey) == true ? dic[sKey] : null;

            if (tagsList != null && tagsList.Count >= index)
            {
                return tagsList[index];
            }

            return string.Empty;
        }

        public void AddTag(string tagName, string tagValue)
        {
            tagValue = tagValue.TrimEnd(';');

            if (!m_tagsDict.ContainsKey(tagName))
            {
                List<string> categoryList = tagValue.Split(';').ToList<string>();
                m_tagsDict.Add(tagName, categoryList);
            }
            else
            {
                List<string> catValues = m_tagsDict[tagName];
                catValues.Add(tagValue);
                m_tagsDict[tagName] = catValues;
            }

            m_tagsDict[tagName] = m_tagsDict[tagName].Distinct().ToList();

        }

        private void SetupAllTagsValues()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(0);
            selectQuery += "select NAME, GROUP_ID from media_tags_types where GROUP_ID in (149, 148) and [STATUS] = 1";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;

                for (int i = 0; i < count; ++i)
                {
                    int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "GROUP_ID", i);
                    string sCurrTag = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", i);

                    if (nGroupID == 149)
                    {
                        m_virtualTags.Add(sCurrTag);
                    }
                    else
                    {
                        m_regularTags.Add(sCurrTag);
                    }
                }

            }
            selectQuery.Finish();
            selectQuery = null;
        }

        private bool IsTagExist(bool isSeries, string key)
        {
            // All common group tags, return true
            if (key.ToLower().Equals("free"))
            {
                return true;
            }

            if (isSeries == true)
            {
                return m_virtualTags.Contains(key);
            }
            else
            {
                return m_regularTags.Contains(key);
            }
        }

        public string GetResXml()
        {
            return m_sResXml;
        }
    }
}
