using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TvinciImporter;
using TVinciShared;

public partial class adm_epg_channels_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            bool flag = false;
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                 System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;

                 if (coll != null && coll.Count > 13 )
                 {   
                     bool isCdvtIDExists = IsCdvtIDExists(coll["14_val"], Session["epg_channel_id"].ToString());
                     if (isCdvtIDExists)
                     {
                         Session["error_msg"] = "Cdvr Id must be unique";
                         flag = true;
                     }
                     else
                     {
                         int epgChannelID = DBManipulator.DoTheWork();
                         bool result = false;
                         if (epgChannelID > 0)
                         {
                             int nGroupID = LoginManager.GetLoginGroupID();
                             result = ImporterImpl.UpdateEpgChannelIndex(new List<ulong>() { (ulong)epgChannelID }, nGroupID, eAction.Update);
                         }
                         return;
                     }
                 }
            }
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            if (Request.QueryString["epg_channel_id"] != null &&
                Request.QueryString["epg_channel_id"].ToString() != "")
                Session["epg_channel_id"] = int.Parse(Request.QueryString["epg_channel_id"].ToString());
            else if (!flag)
                Session["epg_channel_id"] = 0;

            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nOwnerGroupID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
        }
    }


    static private bool IsCdvtIDExists(string cdvrlId, string channelID)
    {
        bool result = false;
        if (string.IsNullOrEmpty(cdvrlId))
        {
            return result;
        }

        int groupID = LoginManager.GetLoginGroupID();       

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("main_connection_string");
        selectQuery += "select ID from epg_channels where status = 1 and group_id in (SELECT * FROM Tvinci..F_Get_GroupsTree ("+ groupID +"))";
        selectQuery += " And ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CDVR_ID", "=", cdvrlId);
        selectQuery += " And ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", channelID);
        

        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;

            if (count > 0)
            {
                result = true;               
            }
        }

        selectQuery.Finish();
        selectQuery = null;

        return result;
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        if (Session["epg_channel_id"] != null &&
            Session["epg_channel_id"].ToString() == "0")
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
            sTemp += "adm_epg_channels_new.aspx?epg_channel_id=" + Session["epg_channel_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
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
                    sTemp += "adm_epg_channel_translate.aspx?epg_channel_id=" + Session["epg_channel_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
        string sRet = PageUtils.GetPreHeader() + ": EPG Channels";
        if (Session["epg_channel_id"] != null && Session["epg_channel_id"].ToString() != "" && Session["epg_channel_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected string GetSafeStrVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField)
    {
        string sRet = "";
        object oVal = selectQuery.Table("query").DefaultView[0].Row[sField];
        if (oVal != DBNull.Value && oVal != null)
            sRet = oVal.ToString();
        return sRet;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["epg_channel_id"] != null && Session["epg_channel_id"].ToString() != "" && int.Parse(Session["epg_channel_id"].ToString()) != 0)
            t = Session["epg_channel_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_channels", "adm_table_pager", "adm_epg_channels.aspx?search_save=1", "", "ID", t, "adm_epg_channels.aspx?search_save=1", "epg_channel_id");

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", true);
        dr_order_num.SetDefault(1);
        theRecord.AddRecord(dr_order_num);

        DataRecordShortTextField dr_channel_id = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_channel_id.Initialize("Channel ID", "adm_table_header_nbg", "FormInput", "CHANNEL_ID", false);
        theRecord.AddRecord(dr_channel_id);

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_bio = new DataRecordLongTextField("ltr", true, 60, 5);
        dr_bio.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_bio);

        DataRecordOnePicBrowserField dr_logo_Pic = new DataRecordOnePicBrowserField();
        dr_logo_Pic.Initialize("Pic", "adm_table_header_nbg", "FormInput", "PIC_ID", false);
        theRecord.AddRecord(dr_logo_Pic);

        DataRecordLongTextField dr_edit_data = new DataRecordLongTextField("rtl", true, 60, 10);
        dr_edit_data.Initialize("Editor remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_edit_data);

        DataRecordShortIntField dr_media_id = new DataRecordShortIntField(true, 9, 9);
        dr_media_id.Initialize("Linear Media ID", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        theRecord.AddRecord(dr_media_id);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordDropDownField dr_epg_channel_type = new DataRecordDropDownField("lu_epg_channel_type", "DESCRIPTION", "id", "", null, 60, true);
        dr_epg_channel_type.SetNoSelectStr("---");
        dr_epg_channel_type.Initialize("Channel Type", "adm_table_header_nbg", "FormInput", "epg_channel_type", false);
        theRecord.AddRecord(dr_epg_channel_type);


        // linear channel settings 

        DataRecordDropDownField dr_CDVR = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_CDVR.Initialize("C-DVR", "adm_table_header_nbg", "FormInput", "ENABLE_CDVR", false);
        theRecord.AddRecord(dr_CDVR);

        DataRecordDropDownField dr_CATCH_UP = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_CATCH_UP.Initialize("CATCH_UP", "adm_table_header_nbg", "FormInput", "ENABLE_CATCH_UP", false);
        theRecord.AddRecord(dr_CATCH_UP);

        DataRecordShortIntField dr_CATCH_UP_BUFFER  = new DataRecordShortIntField(true, 9, 9);
        dr_CATCH_UP_BUFFER.Initialize("CATCH_UP_BUFFER", "adm_table_header_nbg", "FormInput", "CATCH_UP_BUFFER", false);
        theRecord.AddRecord(dr_CATCH_UP_BUFFER);

        DataRecordDropDownField dr_START_OVER = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_START_OVER.Initialize("START_OVER", "adm_table_header_nbg", "FormInput", "ENABLE_START_OVER", false);
        theRecord.AddRecord(dr_START_OVER);

        DataRecordDropDownField dr_LIVE_TRICK_PLAY = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_LIVE_TRICK_PLAY.Initialize("LIVE_TRICK_PLAY", "adm_table_header_nbg", "FormInput", "ENABLE_TRICK_PLAY", false);
        theRecord.AddRecord(dr_LIVE_TRICK_PLAY);

        DataRecordShortIntField dr_LIVE_TRICK_PLAY_BUFFER  = new DataRecordShortIntField(true, 9, 9);
        dr_LIVE_TRICK_PLAY_BUFFER.Initialize("LIVE_TRICK_PLAY_BUFFER", "adm_table_header_nbg", "FormInput", "TRICK_PLAY_BUFFER", false);
        theRecord.AddRecord(dr_LIVE_TRICK_PLAY_BUFFER);
        
        DataRecordShortTextField dr_cdvr_id = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_cdvr_id.Initialize("Cdvr ID", "adm_table_header_nbg", "FormInput", "CDVR_ID", false);
        theRecord.AddRecord(dr_cdvr_id);

        DataRecordDropDownField dr_enableRecordingPlaybackNonEntitled = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_enableRecordingPlaybackNonEntitled.Initialize("Enable Recording Playback (for non-entitled channel)", "adm_table_header_nbg", "FormInput", "enable_recording_playback_non_entitled", false);
        theRecord.AddRecord(dr_enableRecordingPlaybackNonEntitled);

        DataRecordDropDownField dr_enableRecordingPlaybackNonExisting = new DataRecordDropDownField("lu_epg_field_enable", "DESCRIPTION", "id", "", null, 60, false);
        dr_enableRecordingPlaybackNonExisting.Initialize("Enable Recording Playback (for non-existing channel)", "adm_table_header_nbg", "FormInput", "enable_recording_playback_non_existing", false);
        theRecord.AddRecord(dr_enableRecordingPlaybackNonExisting);

        string sTable = theRecord.GetTableHTML("adm_epg_channels_new.aspx?submited=1");

        return sTable;
    }
}
