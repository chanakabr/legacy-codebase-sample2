using ApiObjects;
using EpgBL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_channels_schedule_translate : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }
        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["epg_channels_schedule_id"] != null &&
                Request.QueryString["epg_channels_schedule_id"].ToString() != "")
            {
                Session["epg_channels_schedule_id"] = int.Parse(Request.QueryString["epg_channels_schedule_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels_schedule", "group_id", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["lang_id"] != null &&
                Request.QueryString["lang_id"].ToString() != "")
            {
                Session["lang_id"] = int.Parse(Request.QueryString["lang_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels_schedule", "group_id", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString());
                Int32 nCO = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select count(*) as co from group_extra_languages where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nOwnerGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nCO == 0)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                m_sLangMenu = GetLangMenu(nOwnerGroupID);
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
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
            sTemp += "<li><a href=\"";
            sTemp += "adm_epg_channels_schedule_new.aspx?epg_channels_schedule_id=" + Session["epg_channels_schedule_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels_schedule", "group_id", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nOwnerGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    if (int.Parse(Session["lang_id"].ToString()) == nLangID)
                        sTemp += "<li><a class=\"on\" href=\"";
                    else
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
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": EPG Channels schedule translation (" + PageUtils.GetTableSingleVal("epg_channels_schedule", "name", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString());
        //Response.Write(PageUtils.GetPreHeader() + ": Media management translation");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        EpgCB epg = new EpgCB();
        int epgID = 0;
        object t = null;
        string language = string.Empty;
        if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "" && int.Parse(Session["epg_channels_schedule_id"].ToString()) != 0)
        {
            epgID = int.Parse(Session["epg_channels_schedule_id"].ToString());
           
            if (Session["lang_id"] != null && Session["lang_id"].ToString() != "" && int.Parse(Session["lang_id"].ToString()) != 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select CODE3 from lu_languages where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        language = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
        }
        string sRet = "adm_epg_channels_schedule.aspx?search_save=1&epg_channel_id=" + Session["epg_channel_id"].ToString();
        //string sRet = "adm_epg_channels.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_channels_schedule_translate", "adm_table_pager", sRet, "", "ID", t, sRet, "epg_channels_schedule_id");     

        if (t == null)
            t = 0;

        //Retrieving the EpgCB or generating one if needed        
        int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
        TvinciEpgBL epgBL = new TvinciEpgBL(nParentGroupID);  //assuming this is a Kaltura user - the TVM does not support editing of yes Epg      

        List<string> languages = new List<string>() { language };

        List<EpgCB> epgs = epgBL.GetEpgCB((ulong.Parse(epgID.ToString())), languages);
        if (epgs != null && epgs.Count > 0)
        {
            epg = epgs.Where(x => x.Language == language).FirstOrDefault();
            if (epg == null)
            {
                epg = new EpgCB();
            }
        }

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

    private string GetEpgChannelsSchedulePicImageUrl(out int picId)
    {
        string imageUrl = string.Empty;
        string baseUrl = string.Empty;
        int version = 0;
        picId = 0;
        int groupId = LoginManager.GetLoginGroupID();

        object epgChannelsScheduleId = Session["epg_channels_schedule_id"];


        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.BASE_URL, p.ID, p.version from epg_pics p left join epg_channels_schedule ec on ec.PIC_ID = p.ID where p.STATUS in (0, 1) and ec.id = " + epgChannelsScheduleId.ToString();

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
        {

            baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
            picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
            version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
            int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

            imageUrl = PageUtils.BuildEpgUrl(parentGroupID, baseUrl, version);
        }

        return imageUrl;
    }


}
