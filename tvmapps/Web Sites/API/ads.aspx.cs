using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Xml;
using TVinciShared;
using KLogMonitor;
using System.Reflection;

public partial class ads : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string sUserAgent = Request.ServerVariables["HTTP_USER_AGENT"];
            string sURL = "";
            string sPlayerUN = "";
            string sDevice = "";
            if (Request.QueryString["u"] != null)
                sPlayerUN = Request.QueryString["u"].ToString();
            if (Request.QueryString["dv"] != null)
                sDevice = Request.QueryString["dv"].ToString();

            string sPlayerPass = "";
            if (Request.QueryString["p"] != null)
                sPlayerPass = Request.QueryString["p"].ToString();
            string sMediaFormat = "";
            if (Request.QueryString["f"] != null)
                sMediaFormat = Request.QueryString["f"].ToString();
            string sMediaQuality = "";
            if (Request.QueryString["q"] != null)
                sMediaQuality = Request.QueryString["q"].ToString();

            string sResponseType = "redirect";
            if (Request.QueryString["t"] != null)
                sResponseType = Request.QueryString["t"].ToString();

            Int32 nMediaID = 0;
            if (Request.QueryString["media_id"] != null && Request.QueryString["media_id"].ToString() != "")
                nMediaID = int.Parse(Request.QueryString["media_id"].ToString());

            Int32 nPlayerID = 0;
            Int32 nGroupID = PageUtils.GetGroupByUNPass(sPlayerUN, sPlayerPass, ref nPlayerID);
            Int32 nCountryID = PageUtils.GetIPCountry2();
            Int32 nDeviceID = 0;
            if (sDevice.Trim() != "" && nGroupID != 0)
                nDeviceID = TVinciShared.ProtocolsFuncs.GetDeviceIdFromName(sDevice, nGroupID);
            string sRelTagsIDs = GetRelevantTagsIDs(nMediaID, nGroupID);
            if (sResponseType == "redirect")
            {
                sURL = GetAd(nGroupID, sRelTagsIDs, sMediaQuality, sMediaFormat);
                if (sURL != "")
                {
                    Response.StatusCode = 302;
                    Response.AddHeader("Location", sURL);
                }
                else
                    Response.StatusCode = 404;
                Response.End();
            }
            if (sResponseType == "xml")
            {
                string sXML = GetAdXML(nGroupID, sRelTagsIDs, sMediaQuality, sMediaFormat, sPlayerUN, sPlayerPass, nCountryID, nDeviceID);
                if (sXML != "")
                {
                    Response.Clear();
                    Response.ContentType = "text/xml";
                    Response.Expires = -1;
                    Response.Write(sXML);
                }
                else
                    Response.StatusCode = 404;
                Response.End();
            }

        }
        catch (Exception ex)
        {
            log.Error("exception", ex);
        }
    }

    static protected Int32 GetMainLang(Int32 nGroupID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select g.LANGUAGE_ID from groups g where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    static protected Int32 GetPlayerID(string sPlayerUN, string sPlayerPass)
    {
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from groups_passwords where status=1 and is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("username", "=", sPlayerUN);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("password", "=", sPlayerPass);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nID;
    }

    protected static string GetAdXML(Int32 nGroupID, string sRelTagIDs, string sFileQuality, string sFileFormat,
        string sPlayerUN, string sPlayerPass, Int32 nCountryID, Int32 nDeviceID)
    {
        StringBuilder sRet = new StringBuilder();

        Int32 nMediaFileID = 0;
        Int32 nMediaID = 0;

        Int32 nQualityID = ProtocolsFuncs.GetFileQualityID(sFileQuality);
        Int32 nFormatID = ProtocolsFuncs.GetFileTypeID(sFileFormat, nGroupID);
        Int32 nPlayerID = GetPlayerID(sPlayerUN, sPlayerPass);
        if (sRelTagIDs != "")
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.* from media_files mf (nolock),media m (nolock),media_tags mt (nolock) where mf.status=1 and mf.is_active=1 and m.status=1 and m.is_active=1 and mt.status=1 and (mf.MAX_VIEWS=0 or mf.MAX_VIEWS>mf.views) and m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and mf.media_id=m.id and mt.media_id=m.id and ";
            selectQuery.SetCachedSec(0);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            selectQuery += " and mt.tag_id " + sRelTagIDs;
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_TYPE_ID", "=", nFormatID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_QUALITY_ID", "=", nQualityID);
            selectQuery += " order by newid()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_id"].ToString());
                    Int32 nMaxSession = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_SESSION_VIEWS"].ToString());
                    if (nMaxSession == 0 || HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] == null ||
                        int.Parse(HttpContext.Current.Session["ad_" + nMediaFileID.ToString()].ToString()) < nMaxSession)
                    {
                        string sXML = "<root><request type=\"\"><media id=\"" + nMediaID.ToString() + "\"/></request><flashvars player_un=\"\" player_pass=\"\" file_format=\"" + sFileFormat + "\" file_quality=\"" + sFileQuality + "\" no_cache=\"1\" ";
                        if (HttpContext.Current.Session["tvinci_api"] != null && HttpContext.Current.Session["tvinci_api"].ToString() != "")
                            sXML += " tvinci_guid=\"" + HttpContext.Current.Session["tvinci_api"].ToString() + "\"";
                        sXML += "/></root>";

                        XmlDocument theDoc = new XmlDocument();
                        theDoc.LoadXml(sXML);

                        Int32 nLang = GetMainLang(nGroupID);
                        Int32 nWatcherID = 0;
                        if (HttpContext.Current.Session["tvinci_watcher"] != null &&
                            HttpContext.Current.Session["tvinci_watcher"].ToString() != "")
                            nWatcherID = int.Parse(HttpContext.Current.Session["tvinci_watcher"].ToString());

                        XmlNode elem = null;
                        sRet.Append("<response type=\"ad_system\">");
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLang, true, nWatcherID, false, false, nPlayerID, ref elem, true, true, false, nCountryID, nDeviceID));
                        sRet.Append("</response>");
                        break;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (sRelTagIDs == "" || nMediaID == 0)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.* from media_files mf (nolock),media m (nolock) where mf.status=1 and mf.is_active=1 and m.status=1 and m.is_active=1 and (mf.MAX_VIEWS>mf.views or mf.MAX_VIEWS=0) and m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and mf.media_id=m.id and ";
            selectQuery.SetCachedSec(0);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_TYPE_ID", "=", nFormatID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_QUALITY_ID", "=", nQualityID);
            selectQuery += " order by newid()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["media_id"].ToString());
                    Int32 nMaxSession = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_SESSION_VIEWS"].ToString());
                    if (nMaxSession == 0 || HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] == null ||
                        int.Parse(HttpContext.Current.Session["ad_" + nMediaFileID.ToString()].ToString()) < nMaxSession)
                    {
                        string sXML = "<root><request type=\"single_media\"><media id=\"" + nMediaID.ToString() + "\"/></request><flashvars player_un=\"\" player_pass=\"\" file_format=\"" + sFileFormat + "\" file_quality=\"" + sFileQuality + "\" no_cache=\"1\" ";
                        if (HttpContext.Current.Session["tvinci_api"] != null && HttpContext.Current.Session["tvinci_api"].ToString() != "")
                            sXML += " tvinci_guid=\"" + HttpContext.Current.Session["tvinci_api"].ToString() + "\"";
                        sXML += "/></root>";

                        XmlDocument theDoc = new XmlDocument();
                        theDoc.LoadXml(sXML);

                        Int32 nLang = GetMainLang(nGroupID);
                        Int32 nWatcherID = 0;
                        if (HttpContext.Current.Session["tvinci_watcher"] != null &&
                            HttpContext.Current.Session["tvinci_watcher"].ToString() != "")
                            nWatcherID = int.Parse(HttpContext.Current.Session["tvinci_watcher"].ToString());

                        XmlNode elem = null;
                        sRet.Append("<response type=\"ad_system\">");
                        sRet.Append(ProtocolsFuncs.GetMediaTag(ref theDoc, nMediaID, "media", nGroupID, nLang, true, nWatcherID, false, false, nPlayerID, ref elem, true, true, false, nCountryID, nDeviceID));
                        sRet.Append("</response>");
                        break;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (nMediaID != 0)
        {
            if (HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] == null)
                HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] = 1;
            else
            {
                Int32 nC = int.Parse(HttpContext.Current.Session["ad_" + nMediaFileID.ToString()].ToString());
                HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] = nC + 1;
            }
        }
        return sRet.ToString();
    }

    protected static string GetAd(Int32 nGroupID, string sRelTagIDs, string sFileQuality, string sFileFormat)
    {
        string sURL = "";
        Int32 nMediaFileID = 0;
        Int32 nQualityID = ProtocolsFuncs.GetFileQualityID(sFileQuality);
        Int32 nFormatID = ProtocolsFuncs.GetFileTypeID(sFileFormat, nGroupID);
        if (sRelTagIDs != "")
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.* from media_files mf (nolock),media m (nolock),media_tags mt (nolock) where mf.status=1 and mf.is_active=1 and m.status=1 and m.is_active=1 and mt.status=1 and (mf.MAX_VIEWS=0 or mf.MAX_VIEWS>mf.views) and m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and mf.media_id=m.id and mt.media_id=m.id and ";
            selectQuery.SetCachedSec(0);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            selectQuery += " and mt.tag_id " + sRelTagIDs;
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_TYPE_ID", "=", nFormatID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_QUALITY_ID", "=", nQualityID);
            selectQuery += " order by newid()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    Int32 nMaxSession = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_SESSION_VIEWS"].ToString());
                    if (nMaxSession == 0 || HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] == null ||
                        int.Parse(HttpContext.Current.Session["ad_" + nMediaFileID.ToString()].ToString()) < nMaxSession)
                    {
                        string sCDNImpl = "";
                        Int32 nCDNID = 0;
                        string sCDNNotidyURL = "";
                        DataRecordMediaViewerField d = new DataRecordMediaViewerField("", nMediaFileID);
                        d.GetCDNData(ref sCDNImpl, ref nCDNID, ref sCDNNotidyURL);
                        sURL = d.GetFLVSrc(nGroupID);
                        break;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (sRelTagIDs == "" || sURL == "")
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mf.* from media_files mf (nolock),media m (nolock) where mf.status=1 and mf.is_active=1 and m.status=1 and m.is_active=1 and (mf.MAX_VIEWS>mf.views or mf.MAX_VIEWS=0) and m.start_date<getdate() and (m.end_date is null or m.end_date>getdate()) and mf.media_id=m.id and ";
            selectQuery.SetCachedSec(0);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_TYPE_ID", "=", nFormatID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.MEDIA_QUALITY_ID", "=", nQualityID);
            selectQuery += " order by newid()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    nMediaFileID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    Int32 nMaxSession = int.Parse(selectQuery.Table("query").DefaultView[i].Row["MAX_SESSION_VIEWS"].ToString());
                    if (nMaxSession == 0 || HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] == null ||
                        int.Parse(HttpContext.Current.Session["ad_" + nMediaFileID.ToString()].ToString()) < nMaxSession)
                    {
                        string sCDNImpl = "";
                        Int32 nCDNID = 0;
                        string sCDNNotidyURL = "";
                        DataRecordMediaViewerField d = new DataRecordMediaViewerField("", nMediaFileID);
                        d.GetCDNData(ref sCDNImpl, ref nCDNID, ref sCDNNotidyURL);
                        sURL = d.GetFLVSrc(nGroupID);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (sURL != "")
        {
            if (HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] == null)
                HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] = 1;
            else
            {
                Int32 nC = int.Parse(HttpContext.Current.Session["ad_" + nMediaFileID.ToString()].ToString());
                HttpContext.Current.Session["ad_" + nMediaFileID.ToString()] = nC + 1;
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "update media_files set views=views+1 where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaFileID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;
            }
        }
        return sURL;
    }

    protected static string GetRelevantTagsIDs(Int32 nMediaID, Int32 nGroupID)
    {
        StringBuilder sRet = new StringBuilder();
        string sGroups = PageUtils.GetParentsGroupsStr(nGroupID);
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ID from tags (nolock) where value in (select t.value from media m (nolock),media_tags_types mtt (nolock),media_tags mt (nolock),tags t WITH (nolock) where  mt.status=1 and mt.tag_id=t.id and t.status=1 and m.id=mt.media_id and mtt.id=t.TAG_TYPE_ID and mtt.is_related=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
        selectQuery += "and t.group_id " + sGroups;
        selectQuery += ")";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                sRet.Append("in (");
            for (int i = 0; i < nCount; i++)
            {
                if (i > 0)
                    sRet.Append(",");
                sRet.Append(selectQuery.Table("query").DefaultView[i].Row["ID"]);
            }
            if (nCount > 0)
                sRet.Append(")");
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet.ToString();
    }
}
