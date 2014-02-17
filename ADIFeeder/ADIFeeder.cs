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

namespace ADIFeeder
{
    public class ADIFeeder : ScheduledTasks.BaseTask
    {
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

            public FileStruct(string id, string url, string duration, string CDN, string preProviderID, string breakProviderID, string overlayProviderID, string postProviderID, string breakpoints, string overlaypoints)
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
            }
        }

        private Dictionary<string, string> m_basicsDict;
        private Dictionary<string, string> m_stringMetaDict;
        private Dictionary<string, string> m_numMetaDict;
        private Dictionary<string, string> m_boolMetaDict;
        private Dictionary<string, List<string>> m_tagsDict;
        private Dictionary<string, FileStruct> m_filesDict;
        private Dictionary<string, string> m_picRatios;

        private const string GEO_BLOCK_RULE_KEY = "Geo Block Rule";
        private const string DEVICE_RULE_KEY = "Device Rule";


        private string m_sResXml;

        private int m_nGroupID = 0;
        private string m_sXML;
        List<object> mmm;
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

        private string getBoolStr(string sVal)
        {
            string retVal = "0";
            if (sVal.ToLower().Equals("y"))
            {
                retVal = "1";
            }
            return retVal;
        }

        private bool isCreateSeriesXML()
        {
            bool retVal = false;
            if (m_tagsDict != null && m_tagsDict.Count > 0)
            {
                if (m_tagsDict.ContainsKey("Series name"))
                {
                    if (!isSeriesExist(m_tagsDict["Series name"][0], 149))
                    {
                        if (m_numMetaDict != null && m_numMetaDict.ContainsKey("Episode number"))
                        {
                            int episodeNum = int.Parse(m_numMetaDict["Episode number"]);
                            if (episodeNum == 1)
                            {
                                retVal = true;
                                return retVal;
                            }
                        }
                        if (m_boolMetaDict != null && m_boolMetaDict.ContainsKey("Season Premiere"))
                        {
                            int seasonPremiere = int.Parse(m_boolMetaDict["Season Premiere"]);
                            if (seasonPremiere == 1)
                            {
                                retVal = true;
                                return retVal;
                            }
                        }
                    }
                }

            }

            return retVal;
        }

        private string BuildSeriesXML()
        {
            string retVal = string.Empty;
            StringBuilder sb = new StringBuilder();

            sb.Append("<feed>");
            sb.Append("<export>");
            sb.Append("<media ");
            if (m_basicsDict != null && m_basicsDict.Count > 0)
            {
                if (m_basicsDict.ContainsKey("co_guid"))
                {
                    if (m_basicsDict.ContainsKey("is_active"))
                    {
                        sb.AppendFormat(" co_guid=\"{0}\" action=\"insert\" is_active=\"{1}\" >", m_basicsDict["co_guid"], m_basicsDict["is_active"]);
                    }
                    else
                    {
                        sb.AppendFormat(" co_guid=\"{0}\" action=\"insert\" is_active=\"true\" >", m_basicsDict["co_guid"]);
                    }  
                }
                sb.Append("<basic>");
                if (m_basicsDict.ContainsKey("name"))
                {

                    string seriesName = string.Empty;

                    if (m_tagsDict != null && m_tagsDict.ContainsKey("Series name"))
                    {
                        seriesName = m_tagsDict["Series name"][0];
                    }

                    sb.AppendFormat("<name><value lang=\"eng\">{0}</value></name>", seriesName);

                }
                if (m_basicsDict.ContainsKey("description"))
                {
                    sb.AppendFormat("<description><value lang=\"eng\">{0}</value></description>", m_basicsDict["description"]);
                }

                sb.AppendFormat("<media_type>{0}</media_type>", "Series");


                if (m_basicsDict.ContainsKey("thumb"))
                {

                    sb.AppendFormat("<thumb url=\"{0}\"/>", m_basicsDict["thumb"]);

                }
                sb.AppendFormat("<epg_identifier>0</epg_identifier>");
                sb.Append("<rules>");
                //sb.Append("<watch_per_rule></watch_per_rule>");
                sb.Append("<watch_per_rule>Parent allowed</watch_per_rule>");
                if(m_basicsDict.ContainsKey(DEVICE_RULE_KEY))
                    sb.Append(String.Concat("<device_rule>", m_basicsDict[DEVICE_RULE_KEY], "</device_rule>"));
                sb.Append("</rules>");


                sb.Append("<dates>");
                if (m_basicsDict.ContainsKey("start"))
                {
                    sb.AppendFormat("<start>{0}</start>", m_basicsDict["start"]); //parseDateTimeSTR(m_basicsDict["start"]));
                }
                if (m_basicsDict.ContainsKey("end"))
                {
                    sb.AppendFormat("<catalog_end>{0}</catalog_end>", m_basicsDict["end"]); //parseDateTimeSTR(m_basicsDict["end"]));
                }
                if (m_basicsDict.ContainsKey("final"))
                {
                    sb.AppendFormat("<final_end>{0}</final_end>", m_basicsDict["final"]); //parseDateTimeSTR(m_basicsDict["final"]));
                }
                sb.AppendFormat("</dates>");
                if (m_picRatios != null && m_picRatios.Count > 0)
                {
                    sb.Append("<pic_ratios>");
                    foreach (KeyValuePair<string, string> kvp in m_picRatios)
                    {
                        sb.AppendFormat("<ratio thumb=\"{0}\" ratio=\"{1}\" />", kvp.Value, kvp.Key);
                    }
                    sb.Append("</pic_ratios>");
                }
                sb.Append("</basic>");

            }
            sb.Append("<structure>");
            if (m_stringMetaDict != null && m_stringMetaDict.Count > 0)
            {
                sb.Append("<strings>");
                foreach (KeyValuePair<string, string> kvp in m_stringMetaDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\" ml_handling=\"duplicate\"><value lang=\"eng\">{1}</value></meta>", kvp.Key, kvp.Value);
                }
                sb.Append("</strings>");
            }
            if (m_boolMetaDict != null && m_boolMetaDict.Count > 0)
            {
                sb.Append("<booleans>");
                foreach (KeyValuePair<string, string> kvp in m_boolMetaDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\">{1}</meta>", kvp.Key, kvp.Value);
                }
                sb.Append("</booleans>");
            }
            if (m_numMetaDict != null && m_numMetaDict.Count > 0)
            {
                sb.Append("<doubles>");
                foreach (KeyValuePair<string, string> kvp in m_numMetaDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\">{1}</meta>", kvp.Key, kvp.Value);
                }
                sb.Append("</doubles>");
            }
            if (m_tagsDict != null && m_tagsDict.Count > 0)
            {
                sb.Append("<metas>");
                foreach (KeyValuePair<string, List<string>> kvp in m_tagsDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\" ml_handling=\"unique\">", kvp.Key);
                    foreach (string value in kvp.Value)
                    {
                        sb.Append("<container>");
                        sb.AppendFormat("<value lang=\"eng\">{0}</value>", value);
                        sb.Append("</container>");
                    }
                    sb.Append("</meta>");
                }
                sb.Append("</metas>");
            }
            sb.Append("</structure>");

            sb.Append("</media>");
            sb.Append("</export>");
            sb.Append("</feed>");

            return sb.ToString();
        }

        private string BuildNormalizedXML()
        {
            string retVal = string.Empty;
            StringBuilder sb = new StringBuilder();
            StringBuilder seriesSB = new StringBuilder();
            sb.Append("<feed>");
            sb.Append("<export>");
            sb.Append("<media ");
            if (m_basicsDict != null && m_basicsDict.Count > 0)
            {
                if (m_basicsDict.ContainsKey("co_guid"))
                {
                    if (m_basicsDict.ContainsKey("is_active"))
                    {
                        sb.AppendFormat(" co_guid=\"{0}\" action=\"insert\" is_active=\"{1}\" >", m_basicsDict["co_guid"], m_basicsDict["is_active"]);
                    }
                    else
                    {
                        sb.AppendFormat(" co_guid=\"{0}\" action=\"insert\" is_active=\"true\" >", m_basicsDict["co_guid"]);
                    }        

                }
                sb.Append("<basic>");
                if (m_basicsDict.ContainsKey("name"))
                {
                    string sName = string.Empty;
                    if (m_nGroupID == 148)
                    {
                        if (m_basicsDict.ContainsKey("mediatype"))
                        {
                            string sMediaType = m_basicsDict["mediatype"];
                            string seriesName = string.Empty;
                            string seasonNum = string.Empty;
                            string episodeNum = string.Empty;
                            if (sMediaType.ToLower().Equals("episode") && m_stringMetaDict != null && m_stringMetaDict.ContainsKey("Episode name"))
                            {
                                if (m_tagsDict != null && m_tagsDict.ContainsKey("Series name"))
                                {
                                    seriesName = m_tagsDict["Series name"][0];
                                    sName = seriesName + " - ";
                                }
                                if (m_numMetaDict != null && m_numMetaDict.ContainsKey("Season number"))
                                {
                                    seasonNum = m_numMetaDict["Season number"];
                                    if (seasonNum != "0")
                                    {
                                        sName = string.Format(sName + "Season {0}, ", seasonNum);
                                    }
                                }
                                if (m_numMetaDict != null && m_numMetaDict.ContainsKey("Episode number"))
                                {
                                    episodeNum = m_numMetaDict["Episode number"];
                                    sName = string.Format(sName + "Episode {0}", episodeNum);
                                }

                                //sName = string.Format("{0}-{1}{2}", seriesName, string.isseasonNum, episodeNum);
                            }
                            else
                            {
                                sName = m_basicsDict["name"];
                            }
                        }

                    }
                    else if (m_nGroupID == 149)
                    {
                        if (m_tagsDict != null && m_tagsDict.ContainsKey("Series name") && m_tagsDict["Series name"].Count > 0)
                        {
                            sName = m_tagsDict["Series name"][0];
                            updateSeriesCoGuid(sName, m_basicsDict["co_guid"], 149);
                        }
                    }

                    sb.AppendFormat("<name><value lang=\"eng\">{0}</value></name>", sName);

                }
                if (m_basicsDict.ContainsKey("description"))
                {
                    string sDescription = m_basicsDict["description"];

                    if (string.IsNullOrEmpty(sDescription) && !string.IsNullOrEmpty(m_stringMetaDict["Short summary"]))
                    {
                        sDescription = m_stringMetaDict["Short summary"];
                    }

                    sb.AppendFormat("<description><value lang=\"eng\">{0}</value></description>", sDescription);
                }
                if (m_basicsDict.ContainsKey("mediatype"))
                {
                    sb.AppendFormat("<media_type>{0}</media_type>", m_basicsDict["mediatype"]);

                }
                if (m_basicsDict.ContainsKey("thumb"))
                {
                    sb.AppendFormat("<thumb url=\"{0}\"/>", m_basicsDict["thumb"]);

                }
                sb.AppendFormat("<epg_identifier>0</epg_identifier>");
                sb.Append("<rules>");
                sb.Append("<watch_per_rule>Parent allowed</watch_per_rule>");
                if(m_basicsDict.ContainsKey(GEO_BLOCK_RULE_KEY))
                    sb.Append(String.Concat("<geo_block_rule>", m_basicsDict[GEO_BLOCK_RULE_KEY], "</geo_block_rule>"));
                if(m_basicsDict.ContainsKey(DEVICE_RULE_KEY))
                    sb.Append(String.Concat("<device_rule>", m_basicsDict[DEVICE_RULE_KEY], "</device_rule>"));
                sb.Append("</rules>");


                sb.Append("<dates>");
                if (m_basicsDict.ContainsKey("start"))
                {
                    sb.AppendFormat("<start>{0}</start>", m_basicsDict["start"]); //parseDateTimeSTR(m_basicsDict["start"]));
                }
                if (m_basicsDict.ContainsKey("end"))
                {
                    sb.AppendFormat("<catalog_end>{0}</catalog_end>", m_basicsDict["end"]); //parseDateTimeSTR(m_basicsDict["end"]));
                }
                if (m_basicsDict.ContainsKey("final"))
                {
                    sb.AppendFormat("<final_end>{0}</final_end>", m_basicsDict["final"]); //parseDateTimeSTR(m_basicsDict["final"]));
                }
                sb.AppendFormat("</dates>");
                if (m_picRatios != null && m_picRatios.Count > 0)
                {
                    sb.Append("<pic_ratios>");
                    foreach (KeyValuePair<string, string> kvp in m_picRatios)
                    {
                        sb.AppendFormat("<ratio thumb=\"{0}\" ratio=\"{1}\" />", kvp.Value, kvp.Key);
                    }
                    sb.Append("</pic_ratios>");
                }
                sb.Append("</basic>");

            }
            sb.Append("<structure>");
            if (m_stringMetaDict != null && m_stringMetaDict.Count > 0)
            {
                sb.Append("<strings>");
                foreach (KeyValuePair<string, string> kvp in m_stringMetaDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\" ml_handling=\"duplicate\"><value lang=\"eng\">{1}</value></meta>", kvp.Key, kvp.Value);
                }
                sb.Append("</strings>");
            }
            if (m_boolMetaDict != null && m_boolMetaDict.Count > 0)
            {
                sb.Append("<booleans>");
                foreach (KeyValuePair<string, string> kvp in m_boolMetaDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\">{1}</meta>", kvp.Key, kvp.Value);
                }
                sb.Append("</booleans>");
            }
            if (m_numMetaDict != null && m_numMetaDict.Count > 0)
            {
                sb.Append("<doubles>");
                foreach (KeyValuePair<string, string> kvp in m_numMetaDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\">{1}</meta>", kvp.Key, kvp.Value);
                }
                sb.Append("</doubles>");
            }
            if (m_tagsDict != null && m_tagsDict.Count > 0)
            {
                sb.Append("<metas>");
                foreach (KeyValuePair<string, List<string>> kvp in m_tagsDict)
                {
                    sb.AppendFormat("<meta name=\"{0}\" ml_handling=\"unique\">", kvp.Key);

                    foreach (string value in kvp.Value)
                    {
                        sb.Append("<container>");
                        sb.AppendFormat("<value lang=\"eng\">{0}</value>", value);
                        sb.Append("</container>");
                    }

                    sb.Append("</meta>");
                }
                sb.Append("</metas>");
            }
            sb.Append("</structure>");
            if (m_filesDict != null && m_filesDict.Count > 0)
            {
                sb.Append("<files>");
                foreach (KeyValuePair<string, FileStruct> kvp in m_filesDict)
                {
                    string ppvModule = m_stringMetaDict["PPVModule"];
                    if (string.IsNullOrEmpty(ppvModule) || ppvModule.Equals("PREV99990") || kvp.Key.ToLower().Equals("trailer"))
                    {
                        sb.AppendFormat("<file handling_type=\"Clip\" assetDuration=\"{0}\"  type=\"{1}\" quality=\"HIGH\" billing_type=\"none\" cdn_name=\"Direct Link\" cdn_code=\"{2}\" co_guid=\"{3}\" pre_rule=\"{4}\" break_rule=\"{5}\" overlay_rule=\"{6}\" post_rule=\"{7}\" break_points=\"{8}\" overlay_points=\"{9}\" />", kvp.Value.m_duration, kvp.Key, kvp.Value.m_url, kvp.Value.m_id, kvp.Value.m_PreProviderID, kvp.Value.m_BreakProviderID, kvp.Value.m_OverlayProviderID, kvp.Value.m_PostProviderID, kvp.Value.m_Breakpoints, kvp.Value.m_Overlaypoints);
                    }
                    else
                    {
                        sb.AppendFormat("<file handling_type=\"Clip\" assetDuration=\"{0}\"  type=\"{1}\" quality=\"HIGH\" billing_type=\"Tvinci\" cdn_name=\"Direct Link\" cdn_code=\"{2}\" co_guid=\"{3}\" ppv_module=\"{4}\" pre_rule=\"{5}\" break_rule=\"{6}\" overlay_rule=\"{7}\" post_rule=\"{8}\" break_points=\"{9}\" overlay_points=\"{10}\" />", kvp.Value.m_duration, kvp.Key, kvp.Value.m_url, kvp.Value.m_id, ppvModule, kvp.Value.m_PreProviderID, kvp.Value.m_BreakProviderID, kvp.Value.m_OverlayProviderID, kvp.Value.m_PostProviderID, kvp.Value.m_Breakpoints, kvp.Value.m_Overlaypoints);
                    }
                }
                sb.Append("</files>");

            }
            sb.Append("</media>");
            sb.Append("</export>");
            sb.Append("</feed>");

            return sb.ToString();
        }

        protected override bool DoTheTaskInner()
        {
            m_stringMetaDict = new Dictionary<string, string>();
            m_numMetaDict = new Dictionary<string, string>();
            m_boolMetaDict = new Dictionary<string, string>();
            m_tagsDict = new Dictionary<string, List<string>>();
            m_basicsDict = new Dictionary<string, string>();
            m_filesDict = new Dictionary<string, FileStruct>();
            m_picRatios = new Dictionary<string, string>();
            bool retVal = false;
            int nGroupID = m_nGroupID;

            m_sResXml = string.Empty;

            if (m_nGroupID != 0 && !string.IsNullOrEmpty(m_sXML))
            {
                XmlDocument theDoc = new XmlDocument();
                theDoc.LoadXml(m_sXML);
                XmlNodeList adiNodes = theDoc.SelectNodes("/ADI");
                for (int i = 0; i < adiNodes.Count; i++)
                {
                    XmlNode adiNode = adiNodes[i];
                    string sCoGuid = XmlUtils.GetNodeParameterVal(ref adiNode, "Metadata/AMS", "Asset_ID");
                    string sProvider = XmlUtils.GetNodeParameterVal(ref adiNode, "Metadata/AMS", "Provider");
                    string sPrviderID = XmlUtils.GetNodeParameterVal(ref adiNode, "Metadata/AMS", "Provider_ID");
                    m_basicsDict.Add("co_guid", sCoGuid);
                    m_tagsDict.Add("Provider", sProvider.Split(';').ToList<string>());
                    m_tagsDict.Add("Provider ID", sPrviderID.Split(';').ToList<string>());
                    string sStartDate = string.Empty;
                    string sEndDate = string.Empty;
                    string sFinalEndDate = string.Empty;
                    string sMediaName = string.Empty;
                    string sDescription = string.Empty;
                    string sSummaryShort = string.Empty;
                    string sDuration = string.Empty;
                    string sMediaType = string.Empty;
                    string sBreakpoints = string.Empty;
                    bool isEpisode = false;
                    bool isSeriesType = false;
                    XmlNodeList assetsList = adiNode.SelectNodes("Asset");
                    if (assetsList != null)
                    {
                        for (int j = 0; j < assetsList.Count; j++)
                        {
                            XmlNode assetNode = assetsList[j];
                            string sAssetType = XmlUtils.GetNodeParameterVal(ref assetNode, "Metadata/AMS", "Asset_Class");
                            string assetProduct = XmlUtils.GetNodeParameterVal(ref assetNode, "Metadata/AMS", "Product");
                            XmlNode metaNode = assetNode.SelectSingleNode("Metadata");
                            XmlNodeList appDataList = metaNode.SelectNodes("App_Data");
                            switch (sAssetType.ToLower())
                            {
                                case ("title"):
                                    {
                                        for (int k = 0; k < appDataList.Count; k++)
                                        {
                                            XmlNode appDataNode = appDataList[k];
                                            string sName = XmlUtils.GetItemParameterVal(ref appDataNode, "Name");
                                            string sValue = XmlUtils.GetItemParameterVal(ref appDataNode, "Value");
                                            switch (sName.ToLower())
                                            {
                                                case ("licensing_window_start"):
                                                    {
                                                        DateTime dDate = DateTime.Parse(sValue);
                                                        string sStart = dDate.AddHours(-8).ToString("dd/MM/yyyy HH:mm:ss");
                                                        m_basicsDict.Add("start", sStart);
                                                        m_stringMetaDict.Add("Licensing window start", sStart);
                                                        break;
                                                    }
                                                case ("show_type"):
                                                    {
                                                        if (string.IsNullOrEmpty(sMediaType))
                                                        {
                                                            sMediaType = sValue;
                                                        }
                                                        if (sMediaType.ToLower().Equals("movie") || sMediaType.ToLower().Equals("episode"))
                                                        {
                                                            nGroupID = 148;
                                                        }
                                                        else
                                                        {
                                                            nGroupID = 149;
                                                        }
                                                        break;
                                                    }
                                                case ("licensing_window_end"):
                                                    {
                                                        //Todo : - 1 month
                                                        DateTime dDate = DateTime.Parse(sValue);

                                                        sFinalEndDate = DateTime.MaxValue.ToString("dd/MM/yyyy HH:mm:ss");
                                                        sEndDate = dDate.AddHours(-8).ToString("dd/MM/yyyy HH:mm:ss");
                                                        m_basicsDict.Add("end", sEndDate);
                                                        m_basicsDict.Add("final", sFinalEndDate);
                                                        m_stringMetaDict.Add("Licensing window end", sEndDate);
                                                        break;
                                                    }
                                                case ("title"):
                                                    {
                                                        m_basicsDict.Add("name", sValue);
                                                        break;
                                                    }
                                                case ("run_time"):
                                                    {
                                                        sDuration = sValue;
                                                        break;
                                                    }
                                                case ("summary_long"):
                                                    {
                                                        m_basicsDict.Add("description", sValue);
                                                        break;
                                                    }
                                                case ("summary_short"):
                                                    {
                                                        m_stringMetaDict.Add("Short summary", sValue);
                                                        break;
                                                    }
                                                case ("title_brief"):
                                                    {
                                                        m_stringMetaDict.Add("Short title", sValue);
                                                        break;
                                                    }
                                                case ("year"):
                                                    {
                                                        m_numMetaDict.Add("Release year", sValue);
                                                        break;
                                                    }
                                                case ("billing_id"):
                                                    {
                                                        m_stringMetaDict.Add("Billing ID", sValue);
                                                        m_stringMetaDict.Add("PPVModule", string.Format("{0}{1}", assetProduct, sValue));
                                                        break;
                                                    }

                                                case ("episode_id"):
                                                    {
                                                        string sEpisodeNum = string.Empty;
                                                        string sSeasonNum = "0";

                                                        string sPattern = "^s(?<season>[0-9]*):ep(?<episode>[0-9]*)$";

                                                        Match m = Regex.Match(sValue.ToLower(), sPattern);

                                                        if (m.Success)
                                                        {
                                                            sSeasonNum = m.Groups["season"].Value;
                                                            sEpisodeNum = m.Groups["episode"].Value;
                                                        }
                                                        else
                                                        {
                                                            sEpisodeNum = sValue;
                                                            sSeasonNum = "0";
                                                        }

                                                        if (!string.IsNullOrEmpty(sEpisodeNum))
                                                        {
                                                            m_numMetaDict.Add("Episode number", sEpisodeNum);
                                                        }
                                                        if (!string.IsNullOrEmpty(sSeasonNum))
                                                        {
                                                            m_numMetaDict.Add("Season number", sSeasonNum);
                                                        }

                                                        isEpisode = true;

                                                        break;
                                                    }
                                                //case ("seasonnumber"):
                                                //    {
                                                //        isEpisode = true;
                                                //        m_numMetaDict.Add("Season number", sValue);
                                                //        break;
                                                //    }
                                                //case ("season_number"):
                                                //    {
                                                //        m_numMetaDict.Add("Episode number", sValue);
                                                //        break;
                                                //    }
                                                case ("episode_name"):
                                                    {
                                                        m_stringMetaDict.Add("Episode name", sValue);
                                                        break;
                                                    }
                                                case ("season_premiere"):
                                                    {
                                                        m_boolMetaDict.Add("Season premiere", getBoolStr(sValue));
                                                        break;
                                                    }
                                                case ("season_finale"):
                                                    {
                                                        m_boolMetaDict.Add("season_finale", getBoolStr(sValue));
                                                        break;
                                                    }
                                                case ("closed_captioning"):
                                                    {
                                                        m_boolMetaDict.Add("Closed captions available", getBoolStr(sValue));
                                                        break;
                                                    }
                                                case ("interactive"):
                                                    {
                                                        m_boolMetaDict.Add("interactive", getBoolStr(sValue));
                                                        break;
                                                    }
                                                case ("seasonpackageassetid"):
                                                    {
                                                        m_stringMetaDict.Add("SeasonPackageAssetID", sValue);
                                                        break;
                                                    }
                                                case ("rating"):
                                                    {
                                                        AddTag("Rating", sValue);
                                                        break;
                                                    }
                                                case ("genre"):
                                                    {
                                                        AddTag("Genre", sValue);
                                                        break;
                                                    }
                                                case ("product"):
                                                    {
                                                        AddTag("Product", sValue);
                                                        break;
                                                    }
                                                case ("seriesid"):
                                                    {
                                                        AddTag("Series name", sValue);
                                                        break;
                                                    }
                                                case ("actors_display"):
                                                    {
                                                        AddTag("Main cast", sValue);
                                                        break;
                                                    }
                                                case ("category"):
                                                    {
                                                        AddTag("Category", sValue);
                                                        break;
                                                    }
                                                case ("advisories"):
                                                    {
                                                        AddTag("Rating advisories", sValue);
                                                        break;
                                                    }
                                                case ("director"):
                                                    {
                                                        AddTag("Director", sValue);
                                                        break;
                                                    }
                                                case ("territory"):
                                                    {
                                                        AddTag("Territory", sValue);
                                                        break;
                                                    }
                                                case ("ad_tag_1"):
                                                    {
                                                        if (sValue.EndsWith(";"))
                                                            AddTag("Ad Tag 1", sValue.Substring(0, sValue.Length - 1));
                                                        else
                                                            AddTag("Ad Tag 1", sValue);
                                                        break;
                                                    }
                                                case ("ad_tag_2"):
                                                    {
                                                        if (sValue.EndsWith(";"))
                                                            AddTag("Ad Tag 2", sValue.Substring(0, sValue.Length - 1));
                                                        else
                                                            AddTag("Ad Tag 2", sValue);
                                                        break;
                                                    }
                                                case ("ad_tag_3"):
                                                    {
                                                        if (sValue.EndsWith(";"))
                                                            AddTag("Ad Tag 3", sValue.Substring(0, sValue.Length - 1));
                                                        else
                                                            AddTag("Ad Tag 3", sValue);
                                                        break;
                                                    }
                                                case ("hash_tag"):
                                                    {
                                                        m_stringMetaDict.Add("Hashtag", sValue);
                                                        break;
                                                    }
                                                case ("ad_break"):
                                                    {
                                                        if (sValue.EndsWith(";"))
                                                            sBreakpoints = sValue.Substring(0, sValue.Length - 1);
                                                        else
                                                            sBreakpoints = sValue;
                                                        break;
                                                    }
                                                case ("geo_block_rule"):
                                                    {
                                                        if (sValue.ToLower() == "singapore")
                                                            m_basicsDict.Add(GEO_BLOCK_RULE_KEY, "Singapore only");
                                                        else
                                                            m_basicsDict.Add(GEO_BLOCK_RULE_KEY, string.Empty);
                                                        break;
                                                    }
                                                case ("device_rule"):
                                                    {
                                                        m_basicsDict.Add(DEVICE_RULE_KEY, sValue);
                                                        break;
                                                    }
                                                case ("asset_is_active"):
                                                    {
                                                        m_boolMetaDict.Add("is_active", sValue.ToLower());
                                                        break;
                                                    }
                                                case ("i_channel_category"):
                                                    {
                                                        AddTag("Free", sValue);
                                                        break;
                                                    }
                                                default:
                                                    break;
                                            }
                                        }
                                        sStartDate = string.Empty;
                                        break;
                                    }
                                default:
                                    break;
                            }
                            sMediaType = GetTVMMediaTyoe(sMediaType, ref nGroupID);
                            m_nGroupID = nGroupID;
                            m_basicsDict.Add("mediatype", sMediaType);
                            XmlNodeList fileNodes = assetNode.SelectNodes("Asset");
                            if (fileNodes != null && fileNodes.Count > 0)
                            {
                                string[] durationArr = sDuration.Split(':');
                                TimeSpan ts = new TimeSpan(int.Parse(durationArr[0]), int.Parse(durationArr[1]), int.Parse(durationArr[2]));
                                int durationSec = (int)ts.TotalSeconds;
                                for (int f = 0; f < fileNodes.Count; f++)
                                {
                                    XmlNode fileNode = fileNodes[f];
                                    XmlNode fileMetaNode = fileNode.SelectSingleNode("Metadata");
                                    string assetID = XmlUtils.GetNodeParameterVal(ref fileNode, "Metadata/AMS", "Asset_ID");
                                    string assetType = XmlUtils.GetNodeParameterVal(ref fileNode, "Metadata/AMS", "Asset_Class");
                                    string assetName = XmlUtils.GetNodeParameterVal(ref fileNode, "Metadata/AMS", "Asset_Name");

                                    string duration = string.Empty;
                                    string url = string.Empty;
                                    string cdn = "Direct Link";
                                    string fileURL = XmlUtils.GetNodeParameterVal(ref fileNode, "Content", "Value");

                                    string fileType = "Main";
                                    if (!(assetType.ToLower().Equals("preview")) && !(assetType.ToLower().Equals("movie")) && !(assetType.ToLower().Equals("box cover")) && !(assetType.ToLower().Equals("poster")) && nGroupID != 149)
                                    {
                                        fileType = GetFileType(assetID);
                                        //if (fileURL.ToLower().Contains("iphone"))
                                        //{
                                        //    fileType = "iPhone Main";
                                        //}

                                        //else if (fileURL.ToLower().Contains("stb"))
                                        //{
                                        //    fileType = "STB Main";
                                        //}
                                        //else if (fileURL.ToLower().Contains("ipad"))
                                        //{
                                        //    fileType = "iPad Main";
                                        //}
                                        //else if (fileURL.ToLower().Contains("pc"))
                                        //{
                                        //    fileType = "Main";
                                        //}
                                        FileStruct file = createFileStruct(assetID, fileURL, durationSec.ToString(), "Akamai", sBreakpoints, string.Empty, true);
                                        if (!m_filesDict.ContainsKey(fileType))
                                        {
                                            m_filesDict.Add(fileType, file);
                                        }
                                    }
                                    else
                                    {
                                        if (assetType.Equals("movie") && nGroupID != 149)
                                        {
                                            fileType = "iOS Clear";
                                            XmlNodeList fileAppData = fileMetaNode.SelectNodes("App_Data");
                                            int prevDurationSec = 0;
                                            if (fileAppData != null && fileAppData.Count > 0)
                                            {
                                                for (int t = 0; t < fileAppData.Count; t++)
                                                {
                                                    XmlNode fileAppDataNode = fileAppData[t];
                                                    string sFileNameData = XmlUtils.GetItemParameterVal(ref fileAppDataNode, "Name");
                                                    string sFileValueData = XmlUtils.GetItemParameterVal(ref fileAppDataNode, "Value");
                                                    if (sFileNameData.ToLower().Equals("run_time"))
                                                    {
                                                        string[] prevDurArr = sFileValueData.Split(':');
                                                        TimeSpan prevTS = new TimeSpan(int.Parse(prevDurArr[0]), int.Parse(prevDurArr[1]), int.Parse(prevDurArr[2]));
                                                        prevDurationSec = (int)prevTS.TotalSeconds;

                                                    }
                                                    else if (sFileNameData.ToLower().Equals("languages"))
                                                    {
                                                        string[] isoLangsArr = sFileValueData.Split(';');
                                                        if (isoLangsArr != null && isoLangsArr.Length > 0)
                                                        {
                                                            foreach (string str in isoLangsArr)
                                                            {
                                                                string langStr = getFullLanguageStr(str);
                                                                AddTag("Audio language", langStr);
                                                            }
                                                        }
                                                    }

                                                }

                                            }
                                            FileStruct file = createFileStruct(assetID, fileURL, durationSec.ToString(), "Direct Link", sBreakpoints, string.Empty, true);
                                            m_filesDict.Add(fileType, file);
                                        }
                                        else if (assetType.Equals("preview") && nGroupID != 149)
                                        {
                                            fileType = "Trailer";
                                            XmlNodeList fileAppData = fileMetaNode.SelectNodes("App_Data");
                                            int prevDurationSec = 0;
                                            if (fileAppData != null && fileAppData.Count > 0)
                                            {
                                                for (int t = 0; t < fileAppData.Count; t++)
                                                {
                                                    XmlNode fileAppDataNode = fileAppData[t];
                                                    string sFileNameData = XmlUtils.GetItemParameterVal(ref fileAppDataNode, "Name");
                                                    string sFileValueData = XmlUtils.GetItemParameterVal(ref fileAppDataNode, "Value");
                                                    if (sFileNameData.ToLower().Equals("run_time"))
                                                    {
                                                        string[] prevDurArr = sFileValueData.Split(':');
                                                        TimeSpan prevTS = new TimeSpan(int.Parse(prevDurArr[0]), int.Parse(prevDurArr[1]), int.Parse(prevDurArr[2]));
                                                        prevDurationSec = (int)prevTS.TotalSeconds;

                                                        break;
                                                    }

                                                }

                                            }
                                            FileStruct file = createFileStruct(assetID, fileURL, prevDurationSec.ToString(), "Direct Link", sBreakpoints, string.Empty, false);
                                            m_filesDict.Add(fileType, file);
                                        }
                                        else if (assetType.Equals("poster"))
                                        {
                                            m_basicsDict.Add("thumb", fileURL);
                                            m_picRatios.Add("16:9", fileURL);
                                        }
                                        else if (assetType.Equals("box cover"))
                                        {
                                            m_picRatios.Add("2:3", fileURL);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string normXml = BuildNormalizedXML();
            string seriesXML = string.Empty;
            if (isCreateSeriesXML())
            {
                seriesXML = BuildSeriesXML();
            }
            if (!string.IsNullOrEmpty(normXml))
            {
                //string notifyXml = string.Empty;
                m_sResXml = string.Empty;
                retVal = TvinciImporter.ImporterImpl.DoTheWorkInner(normXml, nGroupID, string.Empty, ref m_sResXml, false);
                if (!string.IsNullOrEmpty(seriesXML))
                {
                    string refXml = string.Empty;
                    TvinciImporter.ImporterImpl.DoTheWorkInner(seriesXML, 149, string.Empty, ref refXml, false);
                    // TvinciImporter.ImporterImpl.UploadDirectory(149);
                }
                // TvinciImporter.ImporterImpl.UploadDirectory(nGroupID);
                ADDIngestToDBAndSaveFiles(m_sXML, normXml, m_sResXml);
            }
            return retVal;
        }

        private FileStruct createFileStruct(string assetID, string fileURL, string durationInSec, string cdn, string breakpoints, string overlaypoints, bool isPutAdDetails)
        {
            string adProvider;
            if (!isPutAdDetails)
                return new FileStruct(assetID, fileURL, durationInSec, cdn, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            adProvider = ConfigurationManager.AppSettings["ADIFeederDefaultAdProvider"];
            if (adProvider == null)
                adProvider = string.Empty;
            return new FileStruct(assetID, fileURL, durationInSec, cdn, adProvider, adProvider, adProvider, adProvider, breakpoints, overlaypoints);
        }

        private void ADDIngestToDBAndSaveFiles(string sOrigXML, string sNormXML, string sResponseXML)
        {
            Logger.Logger.Log("AddIngestToFTP", "Start: Original: " + sOrigXML + " Normalized :" + sNormXML + " Response :" + sResponseXML, "IngetLog");
            DateTime createDate = DateTime.UtcNow;

            int ingestID = IngestionUtils.InsertIngestToDB(createDate, 2, m_nGroupID);


            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sResponseXML);
            XmlNode xn = doc.FirstChild;
            string mediaID = XmlUtils.GetItemParameterVal(ref xn, "tvm_id");
            string status = XmlUtils.GetItemParameterVal(ref xn, "status");
            string coGuid = XmlUtils.GetItemParameterVal(ref xn, "co_guid");
            Logger.Logger.Log("AddIngestToFTP", "After Parse", "IngetLog");
            int nTVMID = 0;
            if (!string.IsNullOrEmpty(mediaID))
            {
                nTVMID = int.Parse(mediaID);
            }

            IngestionUtils.InsertIngestMediaData(ingestID, nTVMID, coGuid, status);

            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            files.Add("original.xml", IngestionUtils.StringToBytes(sOrigXML));
            files.Add("normalized.xml", IngestionUtils.StringToBytes(sNormXML));
            Logger.Logger.Log("AddIngestToFTP", "After Add To Dict", "IngetLog");
            IngestionUtils.UploadIngestToFTP(ingestID, files);
            Logger.Logger.Log("AddIngestToFTP", "End", "IngetLog");
        }

        //private void InsertIngestFileData(int nIngestID, int nMediaID, string sCoGuid, string sStatus)
        //{
        //    ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("ingest_media");
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ingest_id", nIngestID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", nMediaID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", sCoGuid);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("result_status", sStatus);

        //    insertQuery.Execute();
        //    insertQuery.Finish();

        //}

        //private int InsertIngestToDB(DateTime createDate, string ingestFileType, string sIP)
        //{
        //    ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("ingests");
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("orig_type", ingestFileType);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", m_nGroupID);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", createDate);
        //    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", sIP);

        //    insertQuery.Execute();
        //    insertQuery.Finish();

        //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    selectQuery += "SELECT ID FROM ingests WHERE ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
        //    selectQuery += "AND";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", createDate);
        //    DataTable dt = selectQuery.Execute("query", true);
        //    selectQuery.Finish();

        //    int ingestID = 0;

        //    if (dt != null)
        //    {
        //        if (dt.DefaultView.Count > 0)
        //        {
        //            ingestID = int.Parse(dt.Rows[0][0].ToString());
        //        }
        //    }

        //    return ingestID;
        //}

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
                            break;
                        }
                    case ("3"):
                        {
                            retVal = "iPad Main";
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

        private string GetTVMMediaTyoe(string mediaType, ref int groupID)
        {
            string retVal = string.Empty;
            if (mediaType.ToLower().Equals("movie"))
            {
                retVal = "Movie";
                groupID = 148;
            }
            else if (mediaType.ToLower().Equals("series"))
            {
                retVal = "Episode";
                groupID = 148;
            }
            return retVal;
        }

        private void AddTag(string tagName, string tagValue)
        {
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
        }

        public string GetResXml()
        {
            return m_sResXml;
        }
    }
}
