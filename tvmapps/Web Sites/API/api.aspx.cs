using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using System.IO;
using System.Xml;
using System.Threading;
using System.Text;
using com.llnw.mediavault;
using KLogMonitor;
using System.Reflection;


public class StatHolder
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected DateTime m_dStartDate;
    protected double m_dDuration;
    protected Int32 m_nCounter;
    protected double m_nLongest;
    protected double m_nShortest;

    public StatHolder()
    {
        m_dStartDate = DateTime.Now;
        m_dDuration = 0;
        m_nCounter = 0;
        m_nLongest = 0;
        m_nShortest = 0;
    }

    public bool AddDuration(double dDuration, Int32 nMax)
    {
        bool bRet = true;
        if (dDuration > m_nLongest)
            m_nLongest = dDuration;
        if (dDuration < m_nShortest || m_nShortest == 0)
            m_nShortest = dDuration;

        m_dDuration += dDuration;
        m_nCounter++;
        if (m_nCounter > nMax)
        {
            bRet = false;
            m_dStartDate = DateTime.Now;
            m_dDuration = dDuration;
            m_nLongest = 0;
            m_nShortest = 0;
            m_nCounter = 1;
        }
        return bRet;
    }

    public string GetStatStr(string sProtocolName)
    {
        string sRet = "";
        if (m_nCounter > 0)
        {
            if (sProtocolName != "AllAll")
                sRet = "<b>Protocol: " + sProtocolName + "</b> - Avg call: " + m_dDuration / m_nCounter + " sec/call (" + m_nCounter.ToString() + ") , Longest call: " + m_nLongest.ToString() + " , Shortest call: " + m_nShortest.ToString() + " , Call/Sec: " + ((double)m_nCounter / (double)((DateTime.Now - m_dStartDate).TotalSeconds)).ToString();
            else
                sRet = "<b>Total: </b> - Avg call: " + m_dDuration / m_nCounter + " sec/call (" + m_nCounter.ToString() + ") , Longest call: " + m_nLongest.ToString() + " , Shortest call: " + m_nShortest.ToString() + " , Call/Sec: " + ((double)m_nCounter / (double)((DateTime.Now - m_dStartDate).TotalSeconds)).ToString();
        }
        else
        {
            if (sProtocolName != "AllAll")
                sRet = "<b>Total: </b> - No calls for now";
            else
                sRet = "<b>Total: </b> - No calls for now";
        }

        return sRet;
    }
}

public partial class api : System.Web.UI.Page
{
    #region Memmber
    //
    protected XmlDocument m_xmlDox;
    //
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    static protected Hashtable m_StatTableHolder;

    #endregion

    #region Eventes
    /// <summary>
    /// Page Load Event handler
    /// </summary>
    /// <param name="sender">object sebder</param>
    /// <param name="e">Event argument</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (m_StatTableHolder == null)
            m_StatTableHolder = new Hashtable();

        m_xmlDox = new XmlDocument();

        //get request form paramter 
        string sRequest = GetFormParameters();

        //get session ID
        string sSessionID = Session.SessionID;

        //string xmlData = EscapeDecoder(sRequest);
        string xmlData = sRequest;

        //xml document 
        XmlDocument theDoc = new XmlDocument();
        theDoc.PreserveWhitespace = false;

        #region Init API variable
        string sBaseResponse = "";
        Int32 nGroupID = 0;
        Int32 nPlayerID = 0;
        Int32 nCountryID = 0;
        string sTVinciGUID = "";
        string sLastOnTvinci = "";
        string sLastOnSite = "";
        string sType = "";
        bool bWithTimer = false;
        double d = 0;
        string sDurationStr = "";
        //bool bDown = true;
        bool bDown = false;
        #endregion

        //try request API XML if bDown = false 
        if (bDown == true)
        {
            Response.StatusCode = 404;
        }
        else
        {
            #region Request API XML

            bool bZip = false;
            try
            {
                theDoc.LoadXml(xmlData);
                //string sDocStruct = TVinciShared.ProtocolsFuncs.ConvertXMLToString(ref theDoc, true);

                string sWithTimer = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "timer");

                if (sWithTimer == "1")
                    bWithTimer = true;

                //if (bWithTimer == true)
                DateTime theTimer = DateTime.Now;
                //Session["timer_" + AppDomain.GetCurrentThreadId().ToString()] = DateTime.Now;

                XmlNode theType = theDoc.SelectSingleNode("/root/request/@type");
                sType = theType.Value.ToLower().Trim();

                string sSiteGUID = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "site_guid").Trim();
                string sNOCache = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "no_cache");
                string sZiped = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "zip");
                string sDevice = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "device");
                //string sNOCache = "0";

                if (sZiped == "1")
                    bZip = true;

                bool bWithCache = false;

                if (sNOCache == "1")
                    Session["ODBC_CACH_SEC"] = "0";
                else
                    Session["ODBC_CACH_SEC"] = Session["ODBC_CACH_SEC_FIX"];

                if (Session["ODBC_CACH_SEC"].ToString() != "0")
                    bWithCache = true;

                string sHost = "";
                string sRefferer = "";
                if (HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null)
                    sHost = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"].ToLower();
                if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
                    sRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();

                bool bAdmin = false;
                if (sHost.ToLower().IndexOf("admin.tvinci.com") != -1 ||
                    sHost.ToLower().IndexOf("tvm.tvinci.com") != -1 ||
                    sHost.ToLower().IndexOf("62.128.54.164") != -1 ||
                    sHost.ToLower().IndexOf("62.128.54.165") != -1 ||
                    sHost.ToLower().IndexOf("62.128.54.166") != -1 ||
                    sHost.ToLower().IndexOf("62.128.54.167") != -1 ||
                    sHost.ToLower().IndexOf("62.128.54.168") != -1 ||
                    sHost.ToLower().IndexOf("127.0.0.1") != -1 ||
                    sRefferer.ToLower().IndexOf("tvinci.com") != -1)
                    bAdmin = true;

                string sLang = ProtocolsFuncs.GetFlashVarsValue(ref theDoc, "lang");

                Int32 nWatcherID = 0;
                bool bCreate = false;
                if (Session["created"] == null)
                {
                    bCreate = true;
                    Session["created"] = "1";
                }

                if (sType != "categories_tree" && sType != "mh_categories_tree")
                {
                    TVinciShared.ProtocolsFuncs.RemoveFlashVarsParameter(ref theDoc, "with_channels");
                    TVinciShared.ProtocolsFuncs.RemoveFlashVarsParameter(ref theDoc, "starting_menu");
                }
                ApiObjects.PicObject[] thePics = null;
                ApiObjects.MediaObject[] theMediaObjs = null;
                ApiObjects.MediaFileObject theMediaFileObj = null;
                ApiObjects.PlayListSchema thePlaylistSchemaObj = null;
                ApiObjects.InitializationObject theIniObj = null;
                ApiObjects.MediaInfoStructObject theStructObj = null;
                ApiObjects.MediaInfoObject theInfoObj = null;
                ApiObjects.MediaInfoStructObject theInfoStructObj = null;
                ApiObjects.ChannelObject theChannelObj = null;
                ApiObjects.MediaPersonalStatistics thePersonalStatistics = null;
                ApiObjects.MediaStatistics theMediaStatistics = null;
                ApiObjects.SearchDefinitionObject theSearchCriteris = null;
                ApiObjects.PageDefinition thePageDef = null;
                ApiObjects.ChannelRequestObject[] theChannelrequestObjects = null;
                ApiObjects.ChannelObject[] theChannelsObj = null;
                ApiObjects.CategoryObject[] theCategories = null;
                ApiObjects.GenericWriteResponse theWSGenericResposne = null;
                ApiObjects.RateResponseObject theRateResp = null;
                ApiObjects.TagRequestObject[] theTagReqObj = null;
                ApiObjects.TagResponseObject[] theTagRespObj = null;
                ApiObjects.OneTimeObject oOneTimeObj = null;
                ApiObjects.UserComment[] theComments = null;
                ApiObjects.UserComment theComment = null;

                bool bRet = false;

                Int32 nDeviceID = 0;
                nWatcherID = TVinciShared.ProtocolsFuncs.GetStartValues(ref theDoc, ref nGroupID, ref sTVinciGUID, ref sLastOnTvinci, ref sLastOnSite, sSiteGUID, ref nCountryID, ref nPlayerID, bCreate, ref nDeviceID, ref sLang, ref bAdmin, ref bWithCache);

                if (sDevice.Trim() != "" && nGroupID != 0)
                    nDeviceID = TVinciShared.ProtocolsFuncs.GetDeviceIdFromName(sDevice, nGroupID);
                if (nGroupID == 0)
                    sBaseResponse = TVinciShared.ProtocolsFuncs.GetErrorMessage("Site not authorized to query TVinci");
                else if (sType == "starting")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.StartingProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID);
                else if (sType == "vbox_xml")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.VboxXmlProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID);
                else if (sType == "hit")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.HitProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nCountryID, nPlayerID, ref theIniObj, 0, ref theMediaFileObj);
                else if (sType == "set_duration")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SetDurationProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, ref theIniObj, 0, 0, ref bRet);
                else if (sType == "categories_tree")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.CategoriesListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, bWithCache, ref theIniObj, ref theInfoStructObj, 0, true, ref theCategories, nCountryID, nDeviceID, false);
                else if (sType == "mh_categories_tree")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.CategoriesListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, bWithCache, ref theIniObj, ref theInfoStructObj, 0, true, ref theCategories, nCountryID, nDeviceID, true);
                else if (sType == "rss_channels_list")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.RSSChannelsProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, bWithCache, nCountryID, nDeviceID);
                else if (sType == "epg_channels_schedule")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.EPGChannelsScheduleProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref thePics, nDeviceID);
                else if (sType == "epg_channels_list")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.EPGChannelsListListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, ref thePics);
                else if (sType == "playlist_save")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SavePlayListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, sLang, ref theIniObj, null, "", false, ref theWSGenericResposne);
                else if (sType == "delete_playlist_item")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.DeletePlayListItemProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, sLang, ref theIniObj, 0, "", ref theWSGenericResposne);
                else if (sType == "add_playlist_item")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.AddPlayListItemProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, sLang, ref theIniObj, 0, 0, "", ref theWSGenericResposne);
                else if (sType == "delete_playlist")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.DeletePlayListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, sLang, bAdmin, ref theIniObj, ref theInfoStructObj, ref theChannelsObj, 0, nCountryID, nDeviceID);
                else if (sType == "media_mark")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.MediaMark(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nCountryID, nPlayerID, ref theIniObj, "", 0, ref theMediaFileObj);
                else if (sType == "send_to_friend")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SentToFriendProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, ref theIniObj, 0, "", "", "", "", "", ref theWSGenericResposne);
                else if (sType == "send_to_friend_text")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SentToFriendProtocolText(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID);
                else if (sType == "media_structure")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.MediaStructureProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, ref theStructObj);
                else if (sType == "media_info")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.MediaInfoProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache, bAdmin, ref thePics, ref theInfoObj, ref thePersonalStatistics, ref theMediaStatistics, 0);
                else if (sType == "tvc")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.TVCProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache);
                else if (sType == "channels_list")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.ChannelsListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, bWithCache, ref theIniObj, ref theInfoStructObj, 0, ref theChannelsObj, nCountryID, nDeviceID);
                else if (sType == "watcher_channels_list")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.WatcherChannelsListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, bAdmin, ref theIniObj, ref theInfoStructObj, ref theChannelsObj, sLang, nCountryID, nDeviceID);

                //localization stops here
                else if (sType == "single_media")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SingleMediaProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache, bAdmin,
                        ref theIniObj, null, ref theStructObj, ref theMediaObjs, ref thePlaylistSchemaObj, nCountryID, nDeviceID);
                else if (sType == "channels_media")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.ChannelMediaProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelrequestObjects, ref theChannelsObj, nDeviceID);
                else if (sType == "pics")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.PicsProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache, bAdmin);
                else if (sType == "search_media")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SearchMediaProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache, false, bAdmin, nCountryID, ref theIniObj, ref theSearchCriteris, ref theInfoStructObj, ref theChannelObj, nDeviceID);
                else if (sType == "now_playing")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.NowPlayingProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelObj, ref thePageDef, nDeviceID);
                else if (sType == "most_viewd")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.MostViewdProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, nDeviceID);
                else if (sType == "personal_last_watched")
                {
                    if (nGroupID == 109 || nGroupID == 110 || nGroupID == 111)
                    {
                        log.Debug("Request:" + xmlData);
                    }
                    sBaseResponse = TVinciShared.ProtocolsFuncs.PersonalLastWatchedProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref thePageDef, ref theInfoStructObj, ref theChannelObj, nDeviceID);
                    if (nGroupID == 109 || nGroupID == 110 || nGroupID == 111)
                    {
                        log.Debug("Response - " + sBaseResponse);
                    }
                }
                else if (sType == "personal_recommended")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.PersonalRecommendedProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref thePageDef, ref theInfoStructObj, ref theChannelObj, nDeviceID, bWithCache);
                else if (sType == "personal_rated")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.PersonalRatedProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelObj, ref thePageDef, 0, 0, nDeviceID);
                else if (sType == "save_comments")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SaveCommentsProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, ref theIniObj, theComment, false, "", ref theComments, sLang);
                else if (sType == "comments_list")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.CommentsListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, ref theIniObj, 0, "", ref theComments, sLang);
                else if (sType == "save_playlist")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SavePlayListProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, sLang, ref theIniObj, null, "", false, ref theWSGenericResposne);
                else if (sType == "rating")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.RatingProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, ref theIniObj, ref theRateResp, 0, 0);
                else if (sType == "search_related")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SearchRelatedProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelObj, ref thePageDef, 0, nDeviceID);
                else if (sType == "user_social_medias")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.GetUserSocialActions(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelObj, 0, 0, nDeviceID);
                else if (sType == "people_who_liked_also_liked")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.PWLALProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelObj, 0, 0, nDeviceID);
                else if (sType == "people_who_watched_also_watched")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.PWWAWProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bAdmin, nCountryID, ref theIniObj, ref theInfoStructObj, ref theChannelObj, 0, 0, nDeviceID);
                /*
                else if (sType == "tvinci_sa_comm")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.TvinciSACommercialProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, nCountryID);
                else if (sType == "tvinci_ro_text_comm")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.TvinciROTextCommercialProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, nPlayerID, nCountryID);
                */
                else if (sType == "tag_values")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.TagValuesProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, ref theIniObj, ref theTagReqObj, ref theTagRespObj, nCountryID, nDeviceID);
                /*
                else if (sType == "sms_billing_code_check")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SmsBillingCodeCheckProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID);
                */
                else if (sType == "media_onetime_link")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.MediaOneTimeLinkProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, "", 0, 0, "", "", ref oOneTimeObj);
                /*
                else if (sType == "report")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.ReportProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID);
                */
                else if (sType == "subscription_media")
                    sBaseResponse = TVinciShared.ProtocolsFuncs.SubscriptionMediaProtocol(ref theDoc, nGroupID, sTVinciGUID, sLastOnTvinci, sLastOnSite, sSiteGUID, nWatcherID, sLang, nPlayerID, bWithCache, bAdmin, nCountryID, nDeviceID);

                else
                {
                    sBaseResponse = TVinciShared.ProtocolsFuncs.GetErrorMessage("Protocol not recognized");
                }

                d = GetTimerMS(theTimer, ref sDurationStr);
            }
            catch (Exception exp)
            {
                //string sErrMes = "OOOOOPS";
                string sErrMes = exp.Message + "||" + exp.InnerException;
                if (Request.QueryString["statistics"] != null)
                {
                    Int32 nCacheCount = System.Web.HttpRuntime.Cache.Count;
                    sErrMes = "";
                    string m_sServerName = Environment.MachineName;

                    sErrMes += "<h3>Server: " + m_sServerName + "</h3>";
                    sBaseResponse = sErrMes;
                    if (m_StatTableHolder["Total"] != null)
                    {
                        if (((Hashtable)(m_StatTableHolder["Total"])).ContainsKey("All") == true)
                        {
                            sErrMes += ((StatHolder)(((Hashtable)(m_StatTableHolder["Total"]))["All"])).GetStatStr("AllAll") + "<br/>";
                        }
                    }
                    sErrMes += "Cache count: " + nCacheCount.ToString() + "<br/>";
                    if (m_StatTableHolder != null)
                    {
                        Int32 nGroupCount = m_StatTableHolder.Count;
                        IEnumerator iter1 = m_StatTableHolder.Keys.GetEnumerator();
                        Int32 nIterN = 0;
                        while (iter1.MoveNext())
                        {
                            nIterN++;
                            if (iter1.Current.ToString() == "Total")
                                continue;
                            if (nIterN > 1)
                                sErrMes += "<br/>";
                            try
                            {
                                sErrMes += "<b>Group: " + iter1.Current.ToString() + " " + ODBCWrapper.Utils.GetTableSingleVal("groups", "group_name", int.Parse(iter1.Current.ToString())).ToString() + "</b><br/>";
                            }
                            catch
                            {
                                sErrMes += "<b>Group: " + iter1.Current.ToString() + "</b><br/>";
                            }
                            Int32 nCount = ((Hashtable)(m_StatTableHolder[iter1.Current.ToString()])).Count;
                            if (((Hashtable)(m_StatTableHolder[iter1.Current.ToString()])).ContainsKey("All") == true)
                            {
                                sErrMes += ((StatHolder)(((Hashtable)(m_StatTableHolder[iter1.Current.ToString()]))["All"])).GetStatStr("All") + "<br/>";
                            }
                            IEnumerator iter = ((Hashtable)(m_StatTableHolder[iter1.Current.ToString()])).Keys.GetEnumerator();
                            while (iter.MoveNext())
                            {
                                if (iter.Current.ToString() != "All")
                                    sErrMes += ((StatHolder)(((Hashtable)(m_StatTableHolder[iter1.Current.ToString()]))[iter.Current.ToString()])).GetStatStr(iter.Current.ToString()) + "<br/>";
                            }
                        }

                    }
                    sBaseResponse = sErrMes;
                }
                else
                    sBaseResponse = TVinciShared.ProtocolsFuncs.GetErrorMessage(sErrMes);
            }
            Response.ClearHeaders();
            Response.Clear();
            if (sTVinciGUID != "")
                CookieUtils.SetCookie("tvinci_api", sTVinciGUID, 36500);
            byte[] zipResponse = null;
            //else
            //Response.Cookies.Remove("tvinci_api");
            Response.Cookies["ASP.NET_SessionId"].Value = sSessionID;
            if (Request.QueryString["statistics"] == null && Request.QueryString["cache"] == null && (bZip == false || sBaseResponse.Length < 400))
                Response.ContentType = "text/xml";
            else if (bZip == true && sBaseResponse.Length >= 400)
            {
                zipResponse = TVinciShared.ZipUtils.Compress(sBaseResponse);
                Response.ContentType = "application/x-gzip-compressed";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + "api" + ".zip");
                Response.AddHeader("Content-Length", zipResponse.Length.ToString());
            }

            Response.Expires = -1;
            try
            {
                if (sType != "")
                {
                    if (m_StatTableHolder[nGroupID.ToString()] == null)
                    {
                        m_StatTableHolder[nGroupID.ToString()] = new Hashtable();
                    }
                    if (m_StatTableHolder["Total"] == null)
                    {
                        m_StatTableHolder["Total"] = new Hashtable();
                        if (((Hashtable)(m_StatTableHolder["Total"]))["All"] == null)
                        {
                            StatHolder s = new StatHolder();
                            ((Hashtable)(m_StatTableHolder["Total"]))["All"] = s;
                        }
                    }
                    if (((Hashtable)(m_StatTableHolder[nGroupID.ToString()]))[sType] == null)
                    {
                        StatHolder s = new StatHolder();
                        ((Hashtable)(m_StatTableHolder[nGroupID.ToString()]))[sType] = s;
                    }
                    ((StatHolder)(((Hashtable)(m_StatTableHolder[nGroupID.ToString()]))[sType])).AddDuration(d, 5000);
                    if (sType != "media_mark" && sType != "hit" && sType != "starting")
                    {
                        if (((Hashtable)(m_StatTableHolder[nGroupID.ToString()]))["All"] == null)
                        {
                            StatHolder s = new StatHolder();
                            ((Hashtable)(m_StatTableHolder[nGroupID.ToString()]))["All"] = s;
                        }
                        bool bInBounds = ((StatHolder)(((Hashtable)(m_StatTableHolder[nGroupID.ToString()]))["All"])).AddDuration(d, 5000);
                        if (bInBounds == false)
                        {
                            ((Hashtable)(m_StatTableHolder[nGroupID.ToString()])).Clear();
                        }

                        bInBounds = ((StatHolder)(((Hashtable)(m_StatTableHolder["Total"]))["All"])).AddDuration(d, 5000);
                        if (bInBounds == false)
                        {
                            m_StatTableHolder.Clear();
                            //((Hashtable)(m_StatTableHolder["Total"])).Clear();
                            //StatHolder s = new StatHolder();
                            //((Hashtable)(m_StatTableHolder["Total"]))["All"] = s;
                        }
                    }
                }
            }
            catch (Exception ex)
            { 
                log.Error("",ex);
            }


            if (bWithTimer == true)
                sBaseResponse = InsertTimerToXML(sBaseResponse, d);
            if (bZip == false || sBaseResponse.Length < 400)
                Response.Write(sBaseResponse);
            else
                Response.BinaryWrite(zipResponse);
            #endregion
        }
    }


    #endregion

    #region Methods
    /// <summary>
    /// Get Form Parametes
    /// </summary>
    /// <returns>return string</returns>
    protected string GetFormParameters()
    {
        Int32 nCount = Request.TotalBytes;
        string sFormParameters = System.Text.Encoding.UTF8.GetString(Request.BinaryRead(nCount));
        return sFormParameters;
    }
    /// <summary>
    /// Escape Decoder
    /// </summary>
    /// <param name="sToDecode"></param>
    /// <returns>return string</returns>
    protected string EscapeDecoder(string sToDecode)
    {
        sToDecode = sToDecode.Replace("%20", " ");
        sToDecode = sToDecode.Replace("%3C", "<");
        sToDecode = sToDecode.Replace("%3E", ">");
        sToDecode = sToDecode.Replace("%23", "#");
        sToDecode = sToDecode.Replace("%25", "%");
        sToDecode = sToDecode.Replace("%7B", "{");
        sToDecode = sToDecode.Replace("%7D", "}");
        sToDecode = sToDecode.Replace("%7C", "|");
        sToDecode = sToDecode.Replace("%5C", "\\");
        sToDecode = sToDecode.Replace("%5E", "^");
        sToDecode = sToDecode.Replace("%7E", "~");
        sToDecode = sToDecode.Replace("%5B", "[");
        sToDecode = sToDecode.Replace("%60", "`");
        sToDecode = sToDecode.Replace("%27", "'");
        sToDecode = sToDecode.Replace("%3B", ";");
        sToDecode = sToDecode.Replace("%2F", "/");
        sToDecode = sToDecode.Replace("%3F", "?");
        sToDecode = sToDecode.Replace("%3A", ":");
        sToDecode = sToDecode.Replace("%40", "@");
        sToDecode = sToDecode.Replace("%3D", "=");
        sToDecode = sToDecode.Replace("%26", "&amp;");
        sToDecode = sToDecode.Replace("%24", "$");
        sToDecode = sToDecode.Replace("%5F", "_");
        sToDecode = sToDecode.Replace("%22", "\"");

        sToDecode = sToDecode.Replace("%u05d0", "à");
        sToDecode = sToDecode.Replace("%u05d1", "á");
        sToDecode = sToDecode.Replace("%u05d2", "â");
        sToDecode = sToDecode.Replace("%u05d3", "ã");
        sToDecode = sToDecode.Replace("%u05d4", "ä");
        sToDecode = sToDecode.Replace("%u05d5", "å");
        sToDecode = sToDecode.Replace("%u05d6", "æ");
        sToDecode = sToDecode.Replace("%u05d7", "ç");
        sToDecode = sToDecode.Replace("%u05d8", "è");
        sToDecode = sToDecode.Replace("%u05d9", "é");
        sToDecode = sToDecode.Replace("%u05db", "ë");
        sToDecode = sToDecode.Replace("%u05dc", "ì");
        sToDecode = sToDecode.Replace("%u05de", "î");
        sToDecode = sToDecode.Replace("%u05e0", "ð");
        sToDecode = sToDecode.Replace("%u05e1", "ñ");
        sToDecode = sToDecode.Replace("%u05e2", "ò");
        sToDecode = sToDecode.Replace("%u05e4", "ô");
        sToDecode = sToDecode.Replace("%u05e6", "ö");
        sToDecode = sToDecode.Replace("%u05e7", "÷");
        sToDecode = sToDecode.Replace("%u05e8", "ø");
        sToDecode = sToDecode.Replace("%u05e9", "ù");
        sToDecode = sToDecode.Replace("%u05ea", "ú");
        sToDecode = sToDecode.Replace("%u05da", "ê");
        sToDecode = sToDecode.Replace("%u05dd", "í");
        sToDecode = sToDecode.Replace("%u05df", "ï");
        sToDecode = sToDecode.Replace("%u05e3", "ó");
        sToDecode = sToDecode.Replace("%u05e5", "õ");

        return sToDecode;
    }
    /// <summary>
    /// Get Time MS
    /// </summary>
    /// <param name="startDT">DataTime start date</param>
    /// <param name="sTimeStr">ref string Timer</param>
    /// <returns>return double</returns>
    protected double GetTimerMS(DateTime startDT, ref string sTimeStr)
    {
        //DateTime startDT = (DateTime)(Session["timer_" + AppDomain.GetCurrentThreadId().ToString()]);
        TimeSpan t = DateTime.Now - startDT;
        sTimeStr = t.Hours.ToString() + ":" + t.Minutes.ToString() + ":" + t.Seconds.ToString() + ":" + t.Milliseconds.ToString();
        return t.TotalMilliseconds;
    }
    /// <summary>
    /// Insert Timer To XML
    /// </summary>
    /// <param name="sBase">Base Name</param>
    /// <param name="d">double param</param>
    /// <returns>return string</returns>
    protected string InsertTimerToXML(string sBase, double d)
    {
        try
        {
            string sMS = d.ToString();
            Int32 nLoc = sBase.IndexOf("\"", 16);
            string sToInsert = " timer=\"" + sMS + "\" ";
            sBase = sBase.Insert(nLoc + 1, sToInsert);
            return sBase;
        }
        catch (Exception ex)
        {
            return TVinciShared.ProtocolsFuncs.GetErrorMessage(ex.Message);
        }

    }
    #endregion





}
