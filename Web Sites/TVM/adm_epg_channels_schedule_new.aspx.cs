using ApiObjects;
using EpgBL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TvinciImporter;
using TVinciShared;

public partial class adm_epg_channels_schedule_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int nGroupID = LoginManager.GetLoginGroupID();

                int epgID = DBManipulator.DoTheWork();//insert the EPG to DB first

                //if record was saved , update media record with Pic Id 
                if (epgID > 0)
                {
                    int picId = UpdateEpgChannelSchedulePics(epgID, nGroupID);

                    //retreive all tags and Metas IDs from DB
                    Dictionary<int, string> tagsDic = getMetaTag(false);
                    Dictionary<int, string> metasDic = getMetaTag(true);

                    int nParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupID);
                    TvinciEpgBL epgBLTvinci = new TvinciEpgBL(nParentGroupID);  //assuming this is a Kaltura user - the TVM does not support editing of yes Epg

                    EpgCB epg = epgBLTvinci.GetEpgCB((ulong)epgID);
                    CouchBaseManipulator.DoTheWork(ref epg, metasDic, tagsDic); //update the data of the Epg from the page

                    if (picId > 0)
                    {
                        epg.PicID = picId;
                    }

                    ulong nID = 0;
                    if (epg.EpgID == 0)
                    {
                        epg.EpgID = (ulong)epgID;
                        epgBLTvinci.InsertEpg(epg, out nID);
                    }
                    else
                    {
                        epg.EpgID = (ulong)epgID;
                        epgBLTvinci.UpdateEpg(epg);
                    }

                    bool result = false;

                    result = ImporterImpl.UpdateEpg(new List<ulong>() { epg.EpgID }, nGroupID, eAction.Update);
                }
                return;
            }

            if (Session["epg_channel_id"] == null || Session["epg_channel_id"].ToString() == "" || Session["epg_channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["epg_channels_schedule_id"] != null && Request.QueryString["epg_channels_schedule_id"].ToString() != "")
            {
                Session["epg_channels_schedule_id"] = int.Parse(Request.QueryString["epg_channels_schedule_id"].ToString());
                Int32 nOwnerGroupIDChannel = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupIDChannel && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_channels_schedule_id"] = "0";

            m_sLangMenu = GetLangMenu(nOwnerGroupID);
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        if (Session["epg_channels_schedule_id"] != null &&
            Session["epg_channels_schedule_id"].ToString() == "0")
            return "";
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            Int32 nMainLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select l.name,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_epg_channels_schedule_new.aspx?epg_channels_schedule_id=" + Session["epg_channels_schedule_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sTemp += "<li><a href=\"";
                    sTemp += "adm_epg_channels_schedule_translate.aspx?epg_channels_schedule_id=" + Session["epg_channels_schedule_id"].ToString() + "&lang_id=" + nLangID.ToString();
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                if (nCount1 == 0)
                    sTemp = "";
            }
            selectQuery1.Finish();
            selectQuery1 = null;

            return sTemp;
        }
        catch
        {
            //HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    public void GetHeader()
    {
        if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "" && int.Parse(Session["epg_channels_schedule_id"].ToString()) != 0)
        {
            //Int32 nEPGChannelID = int.Parse(PageUtils.GetTableSingleVal("epg_channels_schedule", "EPG_CHANNEL_ID", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString());
            Response.Write(PageUtils.GetPreHeader() + "EPG Channels schedule : " + PageUtils.GetTableSingleVal("epg_channels_schedule", "NAME", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString() + " - Edit");
        }
        else
            Response.Write(PageUtils.GetPreHeader() + "EPG Channels schedule - New");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    //get a Dictionary of the meta\tag ID and its type 
    protected Dictionary<int, string> getMetaTag(bool isMeta)
    {
        Dictionary<int, string> result = new Dictionary<int, string>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select ID, name from";
        if (isMeta)
        {
            selectQuery += "epg_metas_types";
        }
        else
        {
            selectQuery += "EPG_tags_types";
        }
        selectQuery += "where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());

        if (!isMeta)
            selectQuery += "order by order_num";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                object oName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    result.Add(int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()), oName.ToString());
            }
        }

        selectQuery.Finish();
        selectQuery = null;
        return result;
    }

    //add the display of all the metas
    protected void AddMetasFields(ref DBRecordWebEditor theRecord, EpgCB epg)
    {
        Dictionary<int, string> lMetas = getMetaTag(true);
       
        foreach (int id in lMetas.Keys)
        {
            string sName = lMetas[id];
            DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128, id);
            dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", string.Empty, "", false);            
            if (epg.Metas.Keys.Contains(sName))
            {
                string val = "";
                val += epg.Metas[sName][0]; //asumming each meta has only one value
                dr_name.SetValue(val);
            }
            theRecord.AddRecord(dr_name);
        }
    }

    //add the display of all the tags
    protected void AddTagsFields(ref DBRecordWebEditor theRecord, EpgCB epg)
    {
        Dictionary<int, string> lTags = getMetaTag(false);
        foreach (int tagID in lTags.Keys)
        {
            string sName = lTags[tagID];
            DataRecordMultiField dr_tags = new DataRecordMultiField("epg_tags", "id", "id", "EPG_program_tags", "program_id", "epg_tag_id", true, "ltr", 60, "tags");///
            dr_tags.Initialize(sName, "adm_table_header_nbg", "FormInput", "VALUE", "", false);
            dr_tags.SetCollectionLength(8);
            dr_tags.SetExtraWhere("epg_tag_type_id=" + tagID.ToString());           
            if (epg.Tags.Keys.Contains(sName))
            {
                string val = "";
                foreach (string tagVal in epg.Tags[sName])
                    val += tagVal + ";";
                dr_tags.SetValue(val);
            }
            theRecord.AddRecord(dr_tags);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "" && int.Parse(Session["epg_channels_schedule_id"].ToString()) != 0)
            t = Session["epg_channels_schedule_id"];
        string sBack = "adm_epg_channels_schedule.aspx?epg_channel_id=" + Session["epg_channel_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_channels_schedule", "adm_table_pager", sBack, "", "ID", t, sBack, "epg_channel_id");

        if (t == null)//this is the epg ID
            t = 0;

        //Retrieving the EpgCB or generating one if needed        
        int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
        TvinciEpgBL epgBL = new TvinciEpgBL(nParentGroupID);  //assuming this is a Kaltura user - the TVM does not support editing of yes Epg      
        EpgCB epg = epgBL.GetEpgCB(ulong.Parse(t.ToString()));
        if (epg == null)
            epg = new EpgCB();

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", epg.Name, true);
        theRecord.AddRecord(dr_name);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date/Time", "adm_table_header_nbg", "FormInput", "START_DATE", epg.StartDate.ToString("dd/MM/yyyy HH:mm:ss"), true);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date/Time", "adm_table_header_nbg", "FormInput", "END_DATE", epg.EndDate.ToString("dd/MM/yyyy HH:mm:ss"), true);
        theRecord.AddRecord(dr_end_date);

        if (!string.IsNullOrEmpty(epg.EpgIdentifier))
        {
            bool isDownloadPicWithImageServer = false;
            string imageUrl = string.Empty;
            int picId = 0;

            if (ImageUtils.IsDownloadPicWithImageServer())
            {
                isDownloadPicWithImageServer = true;
                int groupId = LoginManager.GetLoginGroupID();
                imageUrl = GetEpgChannelsSchedulePicImageUrl(out picId);
                epg.PicID = picId;
            }

            DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField(string.Empty, epg.EpgIdentifier, epg.ChannelID, isDownloadPicWithImageServer, imageUrl, picId);
            dr_pic.Initialize("Thumb", "adm_table_header_nbg", "FormInput", "PIC_ID", false);
            dr_pic.SetValue(epg.PicID.ToString());
            theRecord.AddRecord(dr_pic);
        }

        DataRecordLongTextField dr_bio = new DataRecordLongTextField("ltr", true, 60, 5);
        dr_bio.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", epg.Description, false);
        theRecord.AddRecord(dr_bio);

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("EPG Identifier", "adm_table_header_nbg", "FormInput", "EPG_IDENTIFIER", epg.EpgIdentifier, true);
        theRecord.AddRecord(dr_d);

        DataRecordOneVideoBrowserField dr_media = new DataRecordOneVideoBrowserField("media", "media_tags", "media_id");
        dr_media.Initialize("Related Media", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        dr_media.SetValue(epg.ExtraData.MediaID.ToString());
        theRecord.AddRecord(dr_media);

        // linear channel settings 

        DataRecordDropDownField dr_CDVR = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_CDVR.Initialize("C-DVR", "adm_table_header_nbg", "FormInput", "ENABLE_CDVR", false);
        dr_CDVR.SetValue(epg.EnableCDVR.ToString());
        theRecord.AddRecord(dr_CDVR);

        DataRecordDropDownField dr_CATCH_UP = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_CATCH_UP.Initialize("Catch-up", "adm_table_header_nbg", "FormInput", "ENABLE_CATCH_UP", false);
        dr_CATCH_UP.SetValue(epg.EnableCatchUp.ToString());
        theRecord.AddRecord(dr_CATCH_UP);

        DataRecordDropDownField dr_START_OVER = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_START_OVER.Initialize("Start Over", "adm_table_header_nbg", "FormInput", "ENABLE_START_OVER", false);
        dr_START_OVER.SetValue(epg.EnableStartOver.ToString());
        theRecord.AddRecord(dr_START_OVER);

        DataRecordDropDownField dr_LIVE_TRICK_PLAY = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_LIVE_TRICK_PLAY.Initialize("Live Trick Play", "adm_table_header_nbg", "FormInput", "ENABLE_TRICK_PLAY", false);
        dr_LIVE_TRICK_PLAY.SetValue(epg.EnableTrickPlay.ToString());
        theRecord.AddRecord(dr_LIVE_TRICK_PLAY);

        bool isTstvSettings = false;
        System.Data.DataRow dr = DAL.ApiDAL.GetTimeShiftedTvPartnerSettings(nParentGroupID);
        if (dr != null)
        {
            isTstvSettings = true;
        }

        //Recordings (EPG) Data model
        DataRecordLongTextField dr_CRID = new DataRecordLongTextField("ltr", true, 60, 10);
        dr_CRID.Initialize("CRID", "adm_table_header_nbg", "FormInput", "CRID", epg.Crid, isTstvSettings);        
        theRecord.AddRecord(dr_CRID);       

        AddMetasFields(ref theRecord, epg);
        AddTagsFields(ref theRecord, epg);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_epg_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_epg_channel_id.Initialize("Epg Channel ID", "adm_table_header_nbg", "FormInput", "epg_channel_id", false);
        dr_epg_channel_id.SetValue(Session["epg_channel_id"].ToString());
        theRecord.AddRecord(dr_epg_channel_id);

        string sTableNew = theRecord.GetTableHTMLCB("adm_epg_channels_schedule_new.aspx?submited=1", false, epg);

        return sTableNew;
    }

    private string GetEpgChannelsSchedulePicImageUrl(out int picId)
    {
        string imageUrl = string.Empty;
        picId = 0;

        object epgChannelsScheduleId = Session["epg_channels_schedule_id"];
        object channelId = Session["epg_channel_id"];

        return PageUtils.GetEpgChannelsSchedulePicImageUrlByScheduleId(epgChannelsScheduleId.ToString(), channelId.ToString(), out picId);        
    }


    private int UpdateEpgChannelSchedulePics(int epgChannelsScheduleId, int groupId)
    {
        // update epg_multi_pictures
        // update epg_channels_schedule only if ratio Is the group default ratio
        var epgIdentifierSession = Session["epgIdentifierForScheduleNew"];
        int picId = 0;
        int ratioId = 0;

        if (string.IsNullOrWhiteSpace(epgIdentifierSession.ToString()))
        {
            log.Error("UpdateEpgChannelSchedulePics epgIdentifier is null");
            return 0;
        }

        string epgIdentifier = epgIdentifierSession.ToString();

        //Get picId
        string epgPic = string.Format("Epg_Channel_Schedule_{0}_Pic_Id", epgIdentifier);

        if (Session[epgPic] == null || string.IsNullOrEmpty(Session[epgPic].ToString()) ||
            !int.TryParse(Session[epgPic].ToString(), out picId))
        {
            log.ErrorFormat("UpdateEpgChannelSchedulePics Epg_Channel_Schedule_{0}_Pic_Id is null", epgIdentifier);
            return 0;
        }

        //Get ratio
        string picRatio = string.Format("Epg_Pic_ID_{0}_Pic_Ratio", picId.ToString());
        if (Session[picRatio] == null || string.IsNullOrEmpty(Session[picRatio].ToString()) ||
           !int.TryParse(Session[picRatio].ToString(), out ratioId))
        {
            log.ErrorFormat("UpdateEpgChannelSchedulePics Epg_Pic_ID_{0}_Pic_Ratio is null", picId);
            return 0;
        }

        //Get channelId
        if (Session["epg_channel_id"] == null || string.IsNullOrEmpty(Session["epg_channel_id"].ToString()))
        {
            log.Error("UpdateEpgChannelSchedulePics epg_channel_id");
            return 0;
        }

        string channelId = Session["epg_channel_id"].ToString();

        //clear Sessions
        Session[string.Format("Epg_Pic_ID_{0}_Pic_Ratio", picId.ToString())] = null;
        Session[string.Format("Epg_Channel_Schedule_{0}_Pic_Id", epgChannelsScheduleId)] = null;
        Session["epg_channel_id"] = null;

        // update epg_multi_pictures
        bool isInsert = UpdateEpgMultiPictures(groupId, epgIdentifier, channelId, ratioId, picId);

        // update epg_channels_schedule only if ratio Is the group default ratio
        if (isInsert && ratioId == ImageUtils.GetGroupDefaultEpgRatio(groupId))
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
            updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("pic_id", "=", picId);
            updateQuery += "WHERE";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", epgChannelsScheduleId);
            updateQuery.Execute();
            updateQuery.Finish();
        }
        else
        {
            picId = 0;
        }

        Session["epgIdentifierForScheduleNew"] = null;
        return picId;
    }

    private static bool UpdateEpgMultiPictures(int groupId, string epgIdentifier, string channelId, int ratioId, int picId)
    {
        bool insertResult = false;

        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_multi_pictures");
        updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
        updateQuery += "WHERE";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_Identifier", "=", epgIdentifier);
        updateQuery += "AND";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioId);
        updateQuery += "AND";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", channelId);
        updateQuery += "AND";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("epg_multi_pictures");
        insertQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_Identifier", "=", epgIdentifier);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", channelId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("pic_id", "=", picId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", DateTime.UtcNow);
        insertResult = insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        return insertResult;
    }

}
