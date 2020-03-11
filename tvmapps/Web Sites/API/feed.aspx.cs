using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Data;
using System.Text;
using System.Net;
using System.Xml;
using System.Configuration;
using ODBCWrapper;
using KLogMonitor;
using System.Reflection;

public partial class rss : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string GetSafeQueryString(string sKey)
    {
        try
        {
            string retVal = string.Empty;
            string url = GetDecodedUrl(Request.Url.ToString());
            Uri uri = new Uri(url);
            if (!string.IsNullOrEmpty(HttpUtility.ParseQueryString(uri.Query).Get(sKey)))
            {
                retVal = HttpUtility.ParseQueryString(uri.Query).Get(sKey);
            }
            return retVal;
        }
        catch
        {
            return "";
        }
    }

    static protected DataTable GetDataTableForIDs(string sMediaIDs)
    {
        string[] sep = { "," };
        string[] sMediaIDsA = sMediaIDs.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        DataTable d = new DataTable();
        Int32 n = 0;
        d.Columns.Add(PageUtils.GetColumn("ID", n));
        System.Data.DataRow tmpRow = null;
        Int32 nCount = sMediaIDsA.Length;
        for (int i = 0; i < nCount; i++)
        {
            tmpRow = d.NewRow();
            tmpRow["ID"] = int.Parse(sMediaIDsA[i]);
            d.Rows.InsertAt(tmpRow, 0);
            d.AcceptChanges();
        }
        return d.Copy();
    }

    static protected string FriendlyEncode(string sToEncode)
    {
        string sRet = sToEncode.Replace("/", "%2f");
        if (string.IsNullOrEmpty(sRet))
            sRet = "NoTitle";
        sRet = sRet.Replace("&quot;", string.Empty);
        sRet = Uri.EscapeDataString(sRet);
        sRet = sRet.Replace("%20", "-");
        try
        {
            while (sRet.Contains('%'))
            {
                sRet = sRet.Remove(sRet.IndexOf('%'), 3);
            }
        }
        catch (Exception ex)
        {
            sRet = "NoTitle";
        }


        return sRet;

    }

    static protected double GetMediaDuration(Int32 nMediaFileID)
    {
        double dRet = 0.0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select DURATION from  media_files where  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                dRet = double.Parse(selectQuery.Table("query").DefaultView[0].Row["DURATION"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return dRet;
    }

    static protected Int32 GetMediaRating(Int32 nMediaID, ref Int32 nViews)
    {
        Int32 nRet = 100;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select VIEWS,CASE VOTES_COUNT WHEN 0 THEN 100 ELSE (VOTES_SUM/VOTES_COUNT)*20 END as ra from  media where  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ra"].ToString());
                nViews = int.Parse(selectQuery.Table("query").DefaultView[0].Row["VIEWS"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    static protected string GetMediaType(Int32 nMediaID)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select mt.name from  media_types mt,media m where m.MEDIA_TYPE_ID=mt.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                sRet = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    static public string GetNodeParameterVal(ref XmlNode theNode, string sXpath, string sParameterName)
    {
        string sVal = "";
        try
        {
            XmlNodeList theRoot = theNode.SelectNodes(sXpath);
            if (theRoot != null)
            {
                for (int j = 0; j < theRoot.Count; j++)
                {
                    XmlAttributeCollection theAttr = theRoot[j].Attributes;
                    if (theAttr != null)
                    {
                        Int32 nCount = theAttr.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            string sName = theAttr[i].Name.ToLower();
                            if (sName.ToLower().Trim() == sParameterName.ToLower().Trim())
                            {
                                if (sVal != "")
                                    sVal += ",";
                                sVal += theAttr[i].Value.ToString();
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch { }
        return sVal;
    }

    private string GetDecodedUrl(string url)
    {
        string retVal = url;
        retVal = HttpUtility.UrlDecode(url.Replace("\\x", "%"));
        return retVal;
    }

    protected void Page_Load(object sender, EventArgs e)
    {

        Int32 nGroupID = 0;
        string sCallerIP = PageUtils.GetCallerIP();
        log.Debug("RSS IP - " + sCallerIP);
        string url = GetDecodedUrl(Request.Url.ToString());
        Uri uri = new Uri(url);

        if (!string.IsNullOrEmpty(GetSafeQueryString(("group_id"))) != null && !string.IsNullOrEmpty(GetSafeQueryString("type")))
        {
            Int32 nFormGroupID = int.Parse(GetSafeQueryString("group_id"));
            nGroupID = nFormGroupID;
            bool bIPOK = IsIpValid(ref nGroupID, nFormGroupID);
            if (!string.IsNullOrEmpty(GetSafeQueryString("ext_link")))
            {
                log.Debug("Rss Calls - " + string.Format("Ext_Link is {0}", GetSafeQueryString("ext_link")));
                if (GetSafeQueryString("ext_link").ToString().Equals("true"))
                {
                    bIPOK = true;
                }
            }
            if (bIPOK == false)
            {
                log.Debug("RSS 404 - " + string.Format("404 returned from IP : {0}", sCallerIP));
                Response.Expires = -1;
                Response.StatusCode = 404;
                Response.End();
                return;
            }
            try
            {
                Int32 nChannelID = 0;
                Int32 nCountryID = 0;
                Int32 nDeviceID = 0;
                Int32 nStartIndex = 0;
                Int32 nPageSize = 30;

                string sChannelID = GetSafeQueryString("channel_id");
                string sWithImageInDescription = GetSafeQueryString("with_image_in_description");
                string sPicResize = GetSafeQueryString("pic_resize");
                string sPicSize = GetSafeQueryString("pic").ToLower();
                string sStartIndex = GetSafeQueryString("index");
                string sPageSize = GetSafeQueryString("num_of_items");
                string sFileFormat = GetSafeQueryString("format");
                string sFileQuality = GetSafeQueryString("quality");
                string sLang = GetSafeQueryString("lang");
                string sType = GetSafeQueryString("type");
                string sBaseURL = GetSafeQueryString("base_url");
                string sCountryID = GetSafeQueryString("country_id");
                string sDeviceID = GetSafeQueryString("device_id");
                string sRoles = GetSafeQueryString("roles");
                string sMediaIDs = GetSafeQueryString("media_ids");
                string sSubtitles = GetSafeQueryString("subtitles");
                string[] sep = { "," };
                string[] sRolesSep = sRoles.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (CachingManager.CachingManager.Exist("Rss_" + sRoles + "_" + sMediaIDs + "_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription) == true)
                {
                    string sAll = CachingManager.CachingManager.GetCachedData("Rss_" + sRoles + "_" + sMediaIDs + "_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription).ToString();
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sAll);
                    return;
                }
                if (sChannelID != "")
                    nChannelID = int.Parse(sChannelID);
                Int32 nLangID = 0;
                bool bIsLangMain = true;
                GetLangData(sLang, nGroupID, ref nLangID, ref bIsLangMain);
                string sChannelName = "";
                string sChannelDescription = "";
                object oChannelName = null;
                object oChannelDescription = null;
                Int32 nPicID = 0;
                object oPic = null;
                if (nChannelID != 0)
                {
                    if (bIsLangMain == true)
                        oChannelName = ODBCWrapper.Utils.GetTableSingleVal("channels", "name", nChannelID);
                    else
                        oChannelName = ODBCWrapper.Utils.GetTableSingleVal("channel_translate", "name", "channel_ID", "=", nChannelID);
                    if (oChannelName != null && oChannelName != DBNull.Value)
                        sChannelName = oChannelName.ToString();

                    if (bIsLangMain == true)
                        oChannelDescription = ODBCWrapper.Utils.GetTableSingleVal("channels", "description", nChannelID);
                    else
                        oChannelDescription = ODBCWrapper.Utils.GetTableSingleVal("channel_translate", "description", "channel_ID", "=", nChannelID);
                    if (oChannelDescription != null && oChannelDescription != DBNull.Value)
                        sChannelDescription = oChannelDescription.ToString();


                    oPic = ODBCWrapper.Utils.GetTableSingleVal("channels", "PIC_ID", nChannelID);
                    if (oPic != null && oPic != DBNull.Value)
                        nPicID = int.Parse(oPic.ToString());
                }
                StringBuilder sRet = new StringBuilder();

                if (sStartIndex != "")
                    nStartIndex = int.Parse(sStartIndex);
                if (sDeviceID != "")
                    nDeviceID = int.Parse(sDeviceID);
                if (sCountryID != "")
                    nCountryID = int.Parse(sCountryID);
                if (sPageSize != "")
                    nPageSize = int.Parse(sPageSize);
                if (nChannelID != 0 && CheackChannel(nChannelID) == false)
                {
                    log.Error("RSS 404 - " + string.Format(" check channel failed - 404 returned from IP : {0}", sCallerIP));
                    Response.Expires = -1;
                    Response.StatusCode = 404;
                    Response.End();
                    return;
                }

                if (sType == "feed")
                {
                    sRet.Append("<feed>");
                    sRet.Append("<channel id=\"").Append(nChannelID).Append("\" name=\"").Append(sChannelName).Append("\" ");
                    string sPicStr = "";
                    sPicStr = GetPicSizesXMLParts(nPicID, nGroupID);
                    sRet.Append(sPicStr);

                    Channel c = new Channel(int.Parse(sChannelID), true, 0, true, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);
                    object oLink = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                    string sLink = "";
                    if (oLink != null && oLink != DBNull.Value)
                        sLink = oLink.ToString();
                    else
                        sLink = sBaseURL;
                    if (sBaseURL != "")
                        sLink = sBaseURL;

                    DataTable d = c.GetChannelMediaDT(nStartIndex + nPageSize);
                    Int32 nTotalSize = c.GetTotalChannelSize();
                    sRet.Append(" size=\"").Append(nTotalSize).Append("\" ");
                    sRet.Append(">");
                    if (d != null)
                    {
                        Int32 nCount = d.DefaultView.Count;
                        if (nCountryID == 0)
                            nCountryID = PageUtils.GetIPCountry2();
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        for (int i1 = nStartIndex; i1 < nCount; i1++)
                        {
                            bool bAdmin = false;
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            Int32 nMediaFileID = ProtocolsFuncs.GetMediaFileID(nMediaID, sFileFormat, sFileQuality, bAdmin, nGroupID, false);
                            string sMediaType = GetMediaType(nMediaID);
                            sRet.Append("<media id=\"").Append(nMediaID).Append("\" ");

                            nPicID = 0;
                            oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                            if (oPic != null && oPic != DBNull.Value)
                                nPicID = int.Parse(oPic.ToString());

                            sPicStr = "";
                            sPicStr = GetPicSizesXMLParts(nPicID, nGroupID);
                            sRet.Append(sPicStr);
                            string sNewLink = sLink + "?media_id=" + nMediaID.ToString() + "&lang=" + sLang;

                            sRet.Append(" link=\"").Append(ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("\"");
                            sRet.Append(" >");
                            //XmlNode tNode = null;
                            System.Xml.XmlNode theInfoStruct = null;
                            ApiObjects.MediaInfoObject theInfo = null;
                            ApiObjects.MediaPersonalStatistics thePersonalStatistics = null;
                            ApiObjects.MediaStatistics theMediaStatistics = null;
                            string s = ProtocolsFuncs.GetMediaInfoInner(nMediaID, nLangID, bIsLangMain, 0, true, ref theInfoStruct, true, true, false, ref theInfo, ref thePersonalStatistics, ref theMediaStatistics);
                            sRet.Append(s);
                            sRet.Append("</media>");
                        }
                    }
                    sRet.Append("</channel>");
                    sRet.Append("</feed>");
                    CachingManager.CachingManager.SetCachedData("Rss_" + sRoles + "_" + sMediaIDs + "_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription, sRet.ToString(), 7200, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sRet.ToString());
                }
                if (sType == "rss")
                {
                    object oLink = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                    string sLink = "";
                    if (oLink != null && oLink != DBNull.Value)
                        sLink = oLink.ToString();
                    if (sBaseURL != "")
                        sLink = sBaseURL;
                    sRet.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sRet.Append("<rss version=\"2.0\" xmlns:media=\"http://search.yahoo.com/mrss/\">");
                    sRet.Append("<channel>");
                    sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelName, true)).Append("</title>");
                    sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelDescription, true)).Append("</description>");
                    sRet.Append("<image><url>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize)).Append("</url>");

                    #region to fix
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink , true)).Append("</link>"); 
                    #endregion

                    sRet.Append("</image>");


                    DataTable d = null;

                    if (sMediaIDs != "")
                        d = GetDataTableForIDs(sMediaIDs);
                    else
                    {
                        Channel c = new Channel(int.Parse(sChannelID), true, 0, true, nCountryID, nDeviceID);
                        c.SetGroupID(nGroupID);
                        d = c.GetChannelMediaDT(nStartIndex + nPageSize);
                    }


                    if (d != null)
                    {
                        Int32 nCount = d.DefaultView.Count;
                        if (nCountryID == 0)
                            nCountryID = PageUtils.GetIPCountry2();
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());

                        nLangID = int.Parse(PageUtils.GetTableSingleVal("groups", "language_id", nGroupID).ToString());


                        for (int i1 = nStartIndex; i1 < nCount; i1++)
                        {
                            bool bAdmin = false;
                            Int32 nViews = 0;
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            Int32 nMediaRating = GetMediaRating(nMediaID, ref nViews);
                            double dDuration = 0;
                            //string sLang = "";

                            #region to fix
                            //System.Xml.XmlNode theInfoStruct = null;
                            //ApiObjects.MediaInfoObject theInfo = null;
                            //ApiObjects.MediaPersonalStatistics thePersonalStatistics = null;
                            //ApiObjects.MediaStatistics theMediaStatistics = null;
                            //string s = ProtocolsFuncs.GetMediaInfoInner(nMediaID, nLangID, bIsLangMain, 0, true, ref theInfoStruct, true, false, false, ref theInfo, ref thePersonalStatistics, ref theMediaStatistics); 
                            #endregion

                            //3 calls can be reduced to 1
                            Int32 nMediaFileID = ProtocolsFuncs.GetMediaFileID(nMediaID, sFileFormat, sFileQuality, bAdmin, nGroupID, false);
                            dDuration = GetMediaDuration(nMediaFileID);
                            string sMediaType = GetMediaType(nMediaID);

                            // translation
                            string sConnectionKey = String.Format("tvp_connection_{0}", nGroupID.ToString());

                            if (ConfigurationManager.AppSettings[sConnectionKey] != null &&
                                    ConfigurationManager.AppSettings[sConnectionKey].ToString() != "")
                            {
                                DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectQuery.SetConnectionKey(sConnectionKey);

                                selectQuery += "select OriginalText from TranslationMetadata where translationid = (select id from Translation where ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TitleID", "=", sMediaType);
                                selectQuery += ") and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", nLangID);
                                if (selectQuery.Execute("query", true) != null)
                                {
                                    Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                                    if (nCount1 > 0)
                                        sMediaType = selectQuery.Table("query").DefaultView[0].Row["OriginalText"].ToString();
                                }
                                selectQuery.Finish();
                                selectQuery = null;
                            }

                            sRet.Append("<item>");

                            nPicID = 0;
                            oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                            if (oPic != null && oPic != DBNull.Value)
                                nPicID = int.Parse(oPic.ToString());

                            string sPicStr = "";
                            sPicStr = GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize);

                            //string sNewLink = sLink + "?media_id=" + nMediaID.ToString() + "&lang=" + sLang;
                            string sMediaName = "";
                            string sMediaDescription = "";
                            object oMediaName = null;
                            object oMediaDescription = null;
                            if (bIsLangMain == true)
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID);
                            else
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "name", "media_ID", "=", nMediaID);
                            if (oMediaName != null && oMediaName != DBNull.Value)
                                sMediaName = oMediaName.ToString();

                            string sNewLink = "";
                            if (sBaseURL != "stream" && sBaseURL != "stream/")
                            {
                                sNewLink = sLink;
                                if (!sLink.EndsWith("/"))
                                {
                                    sNewLink += "/";
                                }

                                sNewLink += sMediaType + "/" + FriendlyEncode(sMediaName) + "/" + nMediaID.ToString();
                            }
                            else
                            {
                                string sCDNImpl = "";
                                Int32 nCDNID = 0;
                                string sCDNNotidyURL = "";
                                DataRecordMediaViewerField d1 = new DataRecordMediaViewerField("", nMediaFileID);
                                d1.GetCDNData(ref sCDNImpl, ref nCDNID, ref sCDNNotidyURL, 3600);
                                sNewLink = d1.GetFLVSrc(nGroupID);
                            }
                            if (bIsLangMain == true)
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media", "description", nMediaID);
                            else
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "description", "media_ID", "=", nMediaID);
                            if (oMediaDescription != null && oMediaDescription != DBNull.Value)
                                sMediaDescription = oMediaDescription.ToString();
                            if (sWithImageInDescription != "0")
                                sMediaDescription += "<p><img src='" + GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize) + "'/></p>";

                            sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("</title>");
                            sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</link>");



                            string sLength = "";
                            string sUrlType = "";

                            //////////////////// performance issues ///////////////////////
                            /*
                            HttpWebRequest oWebRequest = null; 
                            HttpWebResponse oWebResponse = null;
                            try
                            {
                                oWebRequest = (HttpWebRequest)WebRequest.Create(sNewLink);
                                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                                if (oWebResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    sLength = oWebResponse.ContentLength.ToString();
                                    sUrlType = oWebResponse.ContentType;
                                }
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            catch (Exception ex)
                            {
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                           */
                            sRet.Append("<enclosure length=\"").Append(sLength).Append("\" url=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("\" type=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sUrlType, true)).Append("\" alt=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("\"/>");


                            sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaDescription, true)).Append("</description>");
                            sRet.Append("<image><url>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize)).Append("</url></image>");
                            //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink , true)).Append("</link>");
                            //sRet.Append("</image>");
                            sRet.Append("<rating>").Append(nMediaRating).Append("</rating>");
                            sRet.Append("<media:text>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaDescription, true)).Append("</media:text>");
                            sRet.Append("<media:content duration=\"" + dDuration.ToString() + " \" lang=\"\" />");
                            XmlDocument doc = new XmlDocument();

                            #region to fix
                            //doc.LoadXml("<root>" + s + "</root>");
                            //for (int i = 0; i < sRolesSep.Length; i += 2)
                            //{
                            //    string sRole = sRolesSep[i];
                            //    string sTag = sRolesSep[i + 1];
                            //    XmlNodeList n = doc.DocumentElement.SelectNodes("/root/tags_collections/tag_type[@name='" + sTag + "']");
                            //    string sParameter = "";
                            //    if (n != null && n.Count > 0)
                            //    {
                            //        for (int j = 0; j < n.Count; j++)
                            //        {
                            //            XmlNode theCur = n[j];
                            //            sParameter = GetNodeParameterVal(ref theCur, "tag", "name");
                            //        }
                            //    }
                            //    sRet.Append("<media:credit role=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sRole, true)).Append("\">").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sParameter , true)).Append("</media:credit>");
                            //}        
                            //sRet.Append(s); 
                            #endregion

                            sRet.Append("</item>");

                        }
                    }
                    sRet.Append("</channel>");
                    sRet.Append("</rss>");
                    CachingManager.CachingManager.SetCachedData("Rss_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription, sRet.ToString(), 7200, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                    Response.Clear();
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sRet.ToString());
                }
                if (sType == "msn_ent")
                {
                    object oLink = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                    string sLink = "";
                    if (oLink != null && oLink != DBNull.Value)
                        sLink = oLink.ToString();
                    if (sBaseURL != "")
                        sLink = sBaseURL;
                    if (sPicResize != "1")
                    {
                        sPicSize = "full";
                    }
                    sRet.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sRet.Append("<rss version=\"2.0\">");
                    sRet.Append("<channel>");
                    sRet.Append("<language>es-xl</language>");
                    //sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelName, true)).Append("</title>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelDescription, true)).Append("</description>");
                    //sRet.Append("<image><url>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</url>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("</image>");
                    Channel c = new Channel(int.Parse(sChannelID), true, 0, true, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);

                    DataTable d = c.GetChannelMediaDT(nStartIndex + nPageSize);
                    if (d != null)
                    {
                        Int32 nCount = d.DefaultView.Count;
                        if (nCountryID == 0)
                            nCountryID = PageUtils.GetIPCountry2();
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        for (int i1 = nStartIndex; i1 < nCount; i1++)
                        {
                            bool bAdmin = false;
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            Int32 nMediaFileID = ProtocolsFuncs.GetMediaFileID(nMediaID, sFileFormat, sFileQuality, bAdmin, nGroupID, false);
                            string sMediaType = GetMediaType(nMediaID);
                            sRet.Append("<item>");

                            nPicID = 0;
                            oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                            if (oPic != null && oPic != DBNull.Value)
                                nPicID = int.Parse(oPic.ToString());

                            string sPicStr = "";
                            if (sPicResize != "1")
                            {
                                sPicStr = GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize);
                            }
                            else
                            {
                                sPicStr = GetPicRSSXMLParts(nPicID, nGroupID, "full", string.Empty);
                            }
                            //string sNewLink = sLink + "?media_id=" + nMediaID.ToString() + "&lang=" + sLang;
                            string sMediaName = "";
                            string sBaseMediaName = "";
                            string sMediaDescription = "";
                            object oMediaName = null;
                            object oMediaMeta12 = null;
                            object oMediaDescription = null;
                            if (bIsLangMain == true)
                            {
                                oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media", "meta12_str", nMediaID);
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID);
                            }
                            else
                            {
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "name", "media_ID", "=", nMediaID);
                                oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "meta12_str", "media_ID", "=", nMediaID);
                            }
                            if (oMediaMeta12 != DBNull.Value && !String.IsNullOrEmpty(oMediaMeta12.ToString()))
                                sMediaName = oMediaMeta12.ToString();
                            else if (oMediaName != null && oMediaName != DBNull.Value)
                                sMediaName = oMediaName.ToString();

                            if (oMediaName != null && oMediaName != DBNull.Value)
                                sBaseMediaName = oMediaName.ToString();

                            string mediaIDForLink = nMediaID.ToString();
                            if (sMediaType == "Article" && ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID) != null)
                            {
                                mediaIDForLink = ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID).ToString();
                            }
                            //string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName);
                            string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName) + "/" + mediaIDForLink;

                            if (bIsLangMain == true)
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media", "description", nMediaID);
                            else
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "description", "media_ID", "=", nMediaID);
                            if (oMediaDescription != null && oMediaDescription != DBNull.Value)
                                sMediaDescription = oMediaDescription.ToString();

                            //sMediaDescription += "<p><img src='" + GetPicRSSXMLParts(nPicID, nGroupID, sPicSize) + "'/></p>";
                            string[] sPicSizes = { "123", "100" };
                            if (sPicResize == "1")
                            {
                                string fullPicSize = ProtocolsFuncs.GetPicURL(nPicID, "full");
                                string[] tempPicSize = sPicSize.Split('x');
                                if (tempPicSize != null && tempPicSize.Length > 0)
                                {
                                    string w = tempPicSize[0];
                                    string h = tempPicSize[1];
                                    sPicStr = string.Format("http://platform-us.tvinci.com/pic_resize_tool.aspx?h={0}&w={1}&c=true&u={2}", h, w, fullPicSize);
                                }
                            }
                            else
                            {
                                sPicStr = "http://platform-us.tvinci.com/pic_resize_tool.aspx?h=100&w=123&c=true&u=" + sPicStr;
                            }
                            sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("</title>");
                            sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</link>");
                            sRet.Append("<guid isPermaLink=\"true\">").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</guid>");
                            sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaDescription, true)).Append("</description>");
                            string sLength = "";
                            string sPicType = "";
                            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sPicStr);
                            HttpWebResponse oWebResponse = null;
                            try
                            {
                                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                                if (oWebResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    sLength = oWebResponse.ContentLength.ToString();
                                    sPicType = oWebResponse.ContentType;
                                }
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            catch (Exception ex)
                            {
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            sRet.Append("<enclosure length=\"").Append(sLength).Append("\" url=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicStr, true)).Append("\" type=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicType, true)).Append("\" alt=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("\"/>");
                            //sRet.Append("<image>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</image>");
                            //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink , true)).Append("</link>");
                            //sRet.Append("</image>");
                            sRet.Append("</item>");

                        }
                    }
                    sRet.Append("</channel>");
                    sRet.Append("</rss>");
                    CachingManager.CachingManager.SetCachedData("Rss_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription, sRet.ToString(), 7200, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                    Response.Clear();
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sRet.ToString());
                }
                if (sType == "msn_toolbar")
                {
                    object oLink = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                    string sLink = "";
                    if (oLink != null && oLink != DBNull.Value)
                        sLink = oLink.ToString();
                    if (sBaseURL != "")
                        sLink = sBaseURL;
                    if (sPicResize != "1")
                    {
                        sPicSize = "full";
                    }
                    sRet.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sRet.Append("<rss version=\"2.0\">");
                    sRet.Append("<channel>");
                    sRet.Append("<language>es-xl</language>");
                    //sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelName, true)).Append("</title>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelDescription, true)).Append("</description>");
                    //sRet.Append("<image><url>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</url>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("</image>");
                    Channel c = new Channel(int.Parse(sChannelID), true, 0, true, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);

                    DataTable d = c.GetChannelMediaDT(nStartIndex + nPageSize);
                    if (d != null)
                    {
                        Int32 nCount = d.DefaultView.Count;
                        if (nCountryID == 0)
                            nCountryID = PageUtils.GetIPCountry2();
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        for (int i1 = nStartIndex; i1 < nCount; i1++)
                        {
                            bool bAdmin = false;
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            Int32 nMediaFileID = ProtocolsFuncs.GetMediaFileID(nMediaID, sFileFormat, sFileQuality, bAdmin, nGroupID, false);
                            string sMediaType = GetMediaType(nMediaID);
                            sRet.Append("<item>");

                            nPicID = 0;
                            oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                            if (oPic != null && oPic != DBNull.Value)
                                nPicID = int.Parse(oPic.ToString());

                            string sPicStr = "";
                            sPicStr = GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize);

                            //string sNewLink = sLink + "?media_id=" + nMediaID.ToString() + "&lang=" + sLang;
                            string sMediaName = "";
                            string sBaseMediaName = "";
                            string sMediaDescription = "";
                            object oMediaName = null;
                            object oMediaMeta12 = null;
                            object oMediaDescription = null;
                            object oMediaStartDate = null;
                            DateTime dMediaStartDate = DateTime.Now;
                            if (bIsLangMain == true)
                            {
                                oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media", "meta12_str", nMediaID);
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID);
                            }
                            else
                            {
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "name", "media_ID", "=", nMediaID);
                                oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "meta12_str", "media_ID", "=", nMediaID);
                            }
                            if (oMediaMeta12 != DBNull.Value && !String.IsNullOrEmpty(oMediaMeta12.ToString()))
                                sMediaName = oMediaMeta12.ToString();
                            else if (oMediaName != null && oMediaName != DBNull.Value)
                                sMediaName = oMediaName.ToString();

                            if (oMediaName != null && oMediaName != DBNull.Value)
                                sBaseMediaName = oMediaName.ToString();

                            string mediaIDForLink = nMediaID.ToString();
                            if (sMediaType == "Article" && ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID) != null)
                            {
                                mediaIDForLink = ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID).ToString();
                            }
                            //string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName);
                            string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName) + "/" + mediaIDForLink;

                            if (bIsLangMain == true)
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media", "description", nMediaID);
                            else
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "description", "media_ID", "=", nMediaID);
                            if (oMediaDescription != null && oMediaDescription != DBNull.Value)
                                sMediaDescription = oMediaDescription.ToString();

                            //sMediaDescription += "<p><img src='" + GetPicRSSXMLParts(nPicID, nGroupID, sPicSize) + "'/></p>";
                            string[] sPicSizes = { "123", "100" };
                            if (sPicResize == "1")
                            {
                                string fullPicSize = ProtocolsFuncs.GetPicURL(nPicID, "full");
                                string[] tempPicSize = sPicSize.Split('x');
                                if (tempPicSize != null && tempPicSize.Length > 0)
                                {
                                    string w = tempPicSize[0];
                                    string h = tempPicSize[1];
                                    sPicStr = string.Format("http://platform-us.tvinci.com/pic_resize_tool.aspx?h={0}&w={1}&c=true&u={2}", h, w, fullPicSize);
                                }
                            }
                            else
                            {
                                sPicStr = "http://platform-us.tvinci.com/pic_resize_tool.aspx?h=100&w=123&c=true&u=" + sPicStr;
                            }
                            sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("</title>");
                            sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</link>");
                            sRet.Append("<guid isPermaLink=\"true\">").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</guid>");
                            sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaDescription, true)).Append("</description>");
                            oMediaStartDate = ODBCWrapper.Utils.GetTableSingleVal("media", "start_date", nMediaID);
                            if (oMediaStartDate != null)
                            {
                                dMediaStartDate = DateTime.Parse(oMediaStartDate.ToString());
                                sRet.Append("<pubDate>").Append(String.Format("{0:ddd, d MMM yyyy hh:mm:ss}", dMediaStartDate)).Append(" GMT").Append("</pubDate>");
                            }
                            string sLength = "";
                            string sPicType = "";
                            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sPicStr);
                            HttpWebResponse oWebResponse = null;
                            try
                            {
                                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                                if (oWebResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    sLength = oWebResponse.ContentLength.ToString();
                                    sPicType = oWebResponse.ContentType;
                                }
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            catch (Exception ex)
                            {
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            sRet.Append("<enclosure length=\"").Append(sLength).Append("\" url=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicStr, true)).Append("\" type=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicType, true)).Append("\"/>");
                            //sRet.Append("<image>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</image>");
                            //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink , true)).Append("</link>");
                            //sRet.Append("</image>");
                            sRet.Append("</item>");

                        }
                    }
                    sRet.Append("</channel>");
                    sRet.Append("</rss>");
                    CachingManager.CachingManager.SetCachedData("Rss_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription, sRet.ToString(), 7200, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                    Response.Clear();
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sRet.ToString());
                }
                if (sType == "msn_entNew")
                {
                    object oLink = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                    string sLink = "";
                    if (oLink != null && oLink != DBNull.Value)
                        sLink = oLink.ToString();
                    if (sBaseURL != "")
                        sLink = sBaseURL;

                    if (sPicResize != "1")
                    {
                        sPicSize = "full";
                    }

                    sRet.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sRet.Append("<rss version=\"2.0\">");
                    sRet.Append("<channel>");
                    sRet.Append("<language>es-xl</language>");
                    //sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelName, true)).Append("</title>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelDescription, true)).Append("</description>");
                    //sRet.Append("<image><url>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</url>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("</image>");
                    Channel c = new Channel(int.Parse(sChannelID), true, 0, true, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);

                    DataTable d = c.GetChannelMediaDT(nStartIndex + nPageSize);
                    if (d != null)
                    {
                        Int32 nCount = d.DefaultView.Count;
                        if (nCountryID == 0)
                            nCountryID = PageUtils.GetIPCountry2();
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        for (int i1 = nStartIndex; i1 < nCount; i1++)
                        {
                            bool bAdmin = false;
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            Int32 nMediaFileID = ProtocolsFuncs.GetMediaFileID(nMediaID, sFileFormat, sFileQuality, bAdmin, nGroupID, false);
                            string sMediaType = GetMediaType(nMediaID);
                            sRet.Append("<item>");

                            nPicID = 0;
                            oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                            if (oPic != null && oPic != DBNull.Value)
                                nPicID = int.Parse(oPic.ToString());

                            string sPicStr = "";
                            sPicStr = GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, "0");

                            //string sNewLink = sLink + "?media_id=" + nMediaID.ToString() + "&lang=" + sLang;
                            string sMediaName = "";
                            string sBaseMediaName = "";
                            string sMediaDescription = "";
                            object oMediaName = null;
                            object oMediaMeta12 = null;
                            object oMediaDescription = null;
                            if (bIsLangMain == true)
                            {
                                //oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media", "meta12_str", nMediaID);
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID);
                            }
                            else
                            {
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "name", "media_ID", "=", nMediaID);
                                //oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "meta12_str", "media_ID", "=", nMediaID);
                            }
                            if (oMediaMeta12 != null && oMediaMeta12 != DBNull.Value)
                                sMediaName = oMediaMeta12.ToString();
                            else if (oMediaName != null && oMediaName != DBNull.Value)
                                sMediaName = oMediaName.ToString();

                            if (oMediaName != null && oMediaName != DBNull.Value)
                                sBaseMediaName = oMediaName.ToString();

                            string mediaIDForLink = nMediaID.ToString();
                            if (sMediaType == "Article" && ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID) != null)
                            {
                                mediaIDForLink = ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID).ToString();
                            }
                            //string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName);
                            string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName) + "/" + mediaIDForLink;

                            if (string.IsNullOrEmpty(sSubtitles) || sSubtitles.Equals("1"))
                            {
                                if (bIsLangMain == true)
                                    oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media", "meta12_str", nMediaID);
                                else
                                    oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "meta12_str", "media_ID", "=", nMediaID);
                            }
                            if (oMediaDescription != null && oMediaDescription != DBNull.Value)
                                sMediaDescription = oMediaDescription.ToString();

                            //sMediaDescription += "<p><img src='" + GetPicRSSXMLParts(nPicID, nGroupID, sPicSize) + "'/></p>";
                            string[] sPicSizes = { "123", "100" };
                            if (sPicResize == "1")
                            {
                                string fullPicSize = ProtocolsFuncs.GetPicURL(nPicID, "full");
                                string[] tempPicSize = sPicSize.Split('x');
                                if (tempPicSize != null && tempPicSize.Length > 0)
                                {
                                    string w = tempPicSize[0];
                                    string h = tempPicSize[1];
                                    sPicStr = string.Format("http://platform-us.tvinci.com/pic_resize_tool.aspx?h={0}&w={1}&c=true&u={2}", h, w, fullPicSize);
                                }
                            }
                            else
                            {
                                sPicStr = "http://platform-us.tvinci.com/pic_resize_tool.aspx?h=177&w=303&c=true&u=" + sPicStr;
                            }
                            sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("</title>");
                            sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</link>");
                            sRet.Append("<guid isPermaLink=\"true\">").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</guid>");
                            sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaDescription, true)).Append("</description>");
                            string sLength = "";
                            string sPicType = "";
                            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sPicStr);
                            HttpWebResponse oWebResponse = null;
                            try
                            {
                                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                                if (oWebResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    sLength = oWebResponse.ContentLength.ToString();
                                    sPicType = oWebResponse.ContentType;
                                }
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            catch (Exception ex)
                            {
                                if (oWebResponse != null)
                                    oWebResponse.Close();
                                oWebResponse = null;
                            }
                            sRet.Append("<enclosure length=\"").Append(sLength).Append("\" url=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicStr, true)).Append("\" type=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicType, true)).Append("\" alt=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("\"/>");
                            //sRet.Append("<image>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</image>");
                            //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink , true)).Append("</link>");
                            //sRet.Append("</image>");
                            sRet.Append("</item>");

                        }
                    }
                    sRet.Append("</channel>");
                    sRet.Append("</rss>");
                    CachingManager.CachingManager.SetCachedData("Rss_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription, sRet.ToString(), 7200, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                    Response.Clear();
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sRet.ToString());
                }
                if (sType == "msn_hp")
                {
                    object oLink = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                    string sLink = "";
                    if (oLink != null && oLink != DBNull.Value)
                        sLink = oLink.ToString();
                    else
                        sLink = sBaseURL;
                    if (sBaseURL != "")
                        sLink = sBaseURL;
                    if (sPicResize != "1")
                    {
                        sPicSize = "full";
                    }
                    sRet.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    sRet.Append("<rss version=\"2.0\">");
                    sRet.Append("<CONTENT>");
                    //sRet.Append("<title>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelName, true)).Append("</title>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("<description>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sChannelDescription, true)).Append("</description>");
                    //sRet.Append("<image><url>").Append(GetPicRSSXMLParts(nPicID, nGroupID, sPicSize)).Append("</url>");
                    //sRet.Append("<link>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sLink, true)).Append("</link>");
                    //sRet.Append("</image>");
                    Channel c = new Channel(int.Parse(sChannelID), true, 0, true, nCountryID, nDeviceID);
                    c.SetGroupID(nGroupID);

                    DataTable d = c.GetChannelMediaDT(nStartIndex + nPageSize);
                    if (d != null)
                    {
                        Int32 nCount = d.DefaultView.Count;
                        if (nCountryID == 0)
                            nCountryID = PageUtils.GetIPCountry2();
                        Int32 nBlocakble = int.Parse(PageUtils.GetTableSingleVal("groups", "BLOCKS_ACTIVE", nGroupID).ToString());
                        for (int i1 = nStartIndex; i1 < nCount; i1++)
                        {
                            bool bAdmin = false;
                            Int32 nMediaID = int.Parse(d.DefaultView[i1].Row["ID"].ToString());
                            Int32 nMediaFileID = ProtocolsFuncs.GetMediaFileID(nMediaID, sFileFormat, sFileQuality, bAdmin, nGroupID, false);
                            string sMediaType = GetMediaType(nMediaID);
                            nPicID = 0;
                            oPic = ODBCWrapper.Utils.GetTableSingleVal("media", "MEDIA_PIC_ID", nMediaID);
                            if (oPic != null && oPic != DBNull.Value)
                                nPicID = int.Parse(oPic.ToString());

                            string sPicStr = "";
                            sPicStr = GetPicRSSXMLParts(nPicID, nGroupID, sPicSize, sPicResize);
                            //string sNewLink = sLink + "?media_id=" + nMediaID.ToString() + "&lang=" + sLang;
                            string sMediaName = "";
                            string sBaseMediaName = "";
                            string sMediaDescription = "";
                            object oMediaName = null;
                            object oMediaMeta12 = null;
                            object oMediaDescription = null;
                            if (bIsLangMain == true)
                            {
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID);
                                oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media", "meta12_str", nMediaID);
                            }
                            else
                            {
                                oMediaName = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "name", "media_ID", "=", nMediaID);
                                oMediaMeta12 = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "meta12_str", "media_ID", "=", nMediaID);
                            }
                            if (oMediaMeta12 != null && oMediaMeta12 != DBNull.Value)
                                sMediaName = oMediaMeta12.ToString();
                            else if (oMediaName != null && oMediaName != DBNull.Value)
                                sMediaName = oMediaName.ToString();

                            if (oMediaName != null && oMediaName != DBNull.Value)
                                sBaseMediaName = oMediaName.ToString();

                            //string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName) + "/" + nMediaID.ToString();

                            string mediaIDForLink = nMediaID.ToString();
                            if (sMediaType == "Article" && ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID) != null)
                            {
                                mediaIDForLink = ODBCWrapper.Utils.GetTableSingleVal("media", "meta9_double", nMediaID).ToString();
                            }

                            string sNewLink = sLink + "/" + sMediaType + "/" + FriendlyEncode(sBaseMediaName) + "/" + mediaIDForLink;
                            if (bIsLangMain == true)
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media", "description", nMediaID);
                            else
                                oMediaDescription = ODBCWrapper.Utils.GetTableSingleVal("media_translate", "description", "media_ID", "=", nMediaID);
                            if (oMediaDescription != null && oMediaDescription != DBNull.Value)
                                sMediaDescription = oMediaDescription.ToString();

                            if (i1 == nStartIndex)
                            {
                                string[] sep1 = { "x" };
                                //string[] sPicSizes = sPicSize.ToLower().Split(sep , StringSplitOptions.RemoveEmptyEntries);
                                string[] sPicSizes = { "123", "100" };
                                if (sPicResize == "1")
                                {
                                    string fullPicSize = ProtocolsFuncs.GetPicURL(nPicID, "full");
                                    string[] tempPicSize = sPicSize.Split('x');
                                    if (tempPicSize != null && tempPicSize.Length > 0)
                                    {
                                        string w = tempPicSize[0];
                                        string h = tempPicSize[1];
                                        sPicStr = string.Format("http://platform-us.tvinci.com/pic_resize_tool.aspx?h={0}&w={1}&c=true&u={2}", h, w, fullPicSize);
                                    }
                                }
                                else
                                {
                                    sPicStr = "http://platform-us.tvinci.com/pic_resize_tool.aspx?h=100&w=123&c=true&u=" + sPicStr;
                                }
                                sRet.Append("<CONTENTITEM>");
                                sRet.Append("<MEDIA TYPE=\"IMAGE\" SRC=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sPicStr, true)).Append("\" HEIGHT=\"").Append(sPicSizes[1]).Append("\" WIDTH=\"").Append(sPicSizes[0]).Append("\" ALT=\"").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("\" />");
                                sRet.Append("</CONTENTITEM>");
                            }
                            sRet.Append("<CONTENTITEM>");
                            sRet.Append("<HEADLINE>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sMediaName, true)).Append("</HEADLINE>");
                            sRet.Append("<URL>").Append(TVinciShared.ProtocolsFuncs.XMLEncode(sNewLink, true)).Append("</URL>");
                            sRet.Append("</CONTENTITEM>");

                        }
                    }
                    sRet.Append("</CONTENT>");
                    sRet.Append("</rss>");
                    CachingManager.CachingManager.SetCachedData("Rss_" + nGroupID.ToString() + "_" + sChannelID + "_" + sPicSize + "_" + sStartIndex + "_" + sPageSize + "_" + sFileFormat + "_" + sFileQuality + "_" + sLang + "_" + sType + "_" + sBaseURL + "_" + sCountryID + "_" + sDeviceID + "_" + sPicResize + "_" + sWithImageInDescription, sRet.ToString(), 7200, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                    Response.Clear();
                    Response.Expires = -1;
                    Response.ContentType = "text/xml";
                    Response.Write(sRet.ToString());
                }
            }
            catch
            {
                Response.Expires = -1;
                Response.StatusCode = 500;
                Response.End();
            }
        }
        else
        {
            log.Debug("RSS 404 - " + string.Format("No group ID found - 404 returned from IP : {0}", sCallerIP));
            Response.Expires = -1;
            Response.StatusCode = 404;
            Response.End();
            return;
        }
    }

    static public string GetPicRSSXMLParts(Int32 nPicID, Int32 nGroupID, string sPicSize, string sPicResize)
    {
        if (nPicID == 0)
            nPicID = PageUtils.GetDefaultPICID(nGroupID);
        string sRet = "";
        if (sPicResize == "1")
        {
            sRet = ProtocolsFuncs.GetPicURL(nPicID, "full");
            string[] sep = { "x" };
            string[] splited = sPicSize.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            sRet = "http://platform-us.tvinci.com/pic_resize_tool.aspx?h=" + splited[0] + "&w=" + splited[1] + "&c=true&u=" + sRet;
        }
        else
            sRet = ProtocolsFuncs.GetPicURL(nPicID, sPicSize);
        return ProtocolsFuncs.XMLEncode(sRet, true);
    }

    static public string GetPicSizesXMLParts(Int32 nPicID, Int32 nGroupID)
    {
        if (nPicID == 0)
            nPicID = PageUtils.GetDefaultPICID(nGroupID);
        return " image=\"" + ProtocolsFuncs.XMLEncode(ProtocolsFuncs.GetPicURL(nPicID, "tn"), true) + "\"";
    }

    protected void GetLangData(string sLang, Int32 nGroupID, ref Int32 nLangID, ref bool bIsMain)
    {
        if (sLang == "")
            return;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select g.LANGUAGE_ID,ll.id from groups g,lu_languages ll where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(ll.NAME)))", "=", sLang.Trim().ToLower());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                Int32 nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
                if (nLangID == nMainLangID)
                    bIsMain = true;
                else
                    bIsMain = false;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    static protected bool CheackChannel(Int32 nChannelID)
    {
        try
        {
            Int32 nIsRssChannel = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("channels", "IS_RSS", nChannelID).ToString());
            if (nIsRssChannel == 0)
                return false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    static protected bool IsIpValid(ref Int32 nGroupID, Int32 nFormGroupID)
    {
        return true;
        bool bOK = false;
        string sCallerIP = PageUtils.GetCallerIP();
        log.Debug("Feed IP - " + string.Format("RSS call from IP {0}: ", sCallerIP));
        if (sCallerIP == "127.0.0.1")
        {
            nGroupID = nFormGroupID;
            return true;
        }
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select group_id from groups_ips where RSS_OPEN=1 and is_active=1 and (end_date is null or end_date>getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(IP))", "=", sCallerIP);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nFormGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount == 1)
            {
                bOK = true;
                nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return bOK;
    }
}
