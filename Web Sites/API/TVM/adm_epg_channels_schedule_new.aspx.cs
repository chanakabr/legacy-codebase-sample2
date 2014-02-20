using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_channels_schedule_new : System.Web.UI.Page
{
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
            Int32 nOwnerGroupID1 = LoginManager.GetLoginGroupID();
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int progID = DBManipulator.DoTheWork();
                int collNum = 8;
                int metaCount = GetMetaCount();
                ODBCWrapper.UpdateQuery metaUpdateQuery = new ODBCWrapper.UpdateQuery("epg_program_metas");
                metaUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                metaUpdateQuery += " where ";
                metaUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("program_id", "=", progID);
                metaUpdateQuery.Execute();
                metaUpdateQuery.Finish();
                metaUpdateQuery = null;
                for (int i = collNum; i < collNum + metaCount; i++)
                {
                    string sName = Request.Form[i.ToString() + "_val"];
                    string sExtID = Request.Form[i.ToString() + "_ext"];
                    if (!string.IsNullOrEmpty(sExtID))
                    {
                        if (!string.IsNullOrEmpty(sName))
                        {
                            Logger.Logger.Log("EPG Metas", "for " + i.ToString() + ": " + sName + " " + sExtID, "EPGSChecule");
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("epg_program_metas");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", sName);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_meta_id", "=", int.Parse(sExtID));
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("program_id", "=", progID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", 1);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }
                    }
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
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_channels_schedule_id"] = "0";

            m_sLangMenu = GetLangMenu(nOwnerGroupID1);
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

    protected void AddTagsFields(ref DBRecordWebEditor theRecord)
    {
        string sGroups = PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID());
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from EPG_tags_types where status=1 and group_id " + sGroups;

        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());

        selectQuery += "order by order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sTagTypeName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                Int32 nTagTypeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                DataRecordMultiField dr_tags = new DataRecordMultiField("epg_tags", "id", "id", "EPG_program_tags", "program_id", "epg_tag_id", true, "ltr", 60, "tags");
                dr_tags.Initialize(sTagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);
                dr_tags.SetCollectionLength(8);
                dr_tags.SetExtraWhere("epg_tag_type_id=" + nTagTypeID.ToString());
                theRecord.AddRecord(dr_tags);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        
    }

    protected int GetMetaCount()
    {
        int retVal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select count(*) as 'count' from epg_metas_types where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["count"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return retVal;
    }
    protected void AddStrFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id, name from epg_metas_types where status=1 and type = 2 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
       
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 0; i < nCount; i++)
                {
                    int id = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    object oName = selectQuery.Table("query").DefaultView[i].Row["name"];
                   
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();

                        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128, id);
                        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", string.Empty, false);
                        ODBCWrapper.DataSetSelectQuery valueSelectQUery = new ODBCWrapper.DataSetSelectQuery();
                        if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "" && int.Parse(Session["epg_channels_schedule_id"].ToString()) != 0)
                        {
                            valueSelectQUery += " select value from epg_program_metas where status = 1 ";

                            valueSelectQUery += " and ";
                            valueSelectQUery += ODBCWrapper.Parameter.NEW_PARAM("epg_meta_id", "=", id);
                            valueSelectQUery += " and ";
                            valueSelectQUery += ODBCWrapper.Parameter.NEW_PARAM("program_id", "=", int.Parse(Session["epg_channels_schedule_id"].ToString()));
                            if (valueSelectQUery.Execute("query", true) != null)
                            {
                                int valueCount = valueSelectQUery.Table("query").DefaultView.Count;
                                if (valueCount > 0)
                                {
                                    object oValue = valueSelectQUery.Table("query").DefaultView[0].Row["value"];

                                    if (oValue != DBNull.Value && oValue != null && oName.ToString() != "")
                                     {
                                         dr_name.SetValue(oValue.ToString());
                                     }
                                }
                            }
                            valueSelectQUery.Finish();
                            valueSelectQUery = null;
                        }
                        theRecord.AddRecord(dr_name);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
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

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date/Time", "adm_table_header_nbg", "FormInput", "START_DATE", true);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date/Time", "adm_table_header_nbg", "FormInput", "END_DATE", true);
        theRecord.AddRecord(dr_end_date);

        DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField();
        dr_pic.Initialize("Thumb", "adm_table_header_nbg", "FormInput", "PIC_ID", false);
        theRecord.AddRecord(dr_pic);

        DataRecordLongTextField dr_bio = new DataRecordLongTextField("ltr", true, 60, 5);
        dr_bio.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_bio);

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("EPG Identifier", "adm_table_header_nbg", "FormInput", "EPG_IDENTIFIER", true);
        theRecord.AddRecord(dr_d);
        //DataRecordOneVideoBrowserField dr_media = new DataRecordOneVideoBrowserField("media", "media_tags", "media_id");
        //dr_media.Initialize("The Media", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        //theRecord.AddRecord(dr_media);

        DataRecordShortTextField dr_t = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_t.Initialize("EPG Tag", "adm_table_header_nbg", "FormInput", "EPG_Tag", false);
        theRecord.AddRecord(dr_t);

        DataRecordOneVideoBrowserField dr_media = new DataRecordOneVideoBrowserField("media", "media_tags", "media_id");
        dr_media.Initialize("Related Media", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        theRecord.AddRecord(dr_media);


        AddStrFields(ref theRecord);
        AddTagsFields(ref theRecord);
        

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_epg_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_epg_channel_id.Initialize("Epg Channel ID", "adm_table_header_nbg", "FormInput", "epg_channel_id", false);
        dr_epg_channel_id.SetValue(Session["epg_channel_id"].ToString());
        theRecord.AddRecord(dr_epg_channel_id);

        string sTable = theRecord.GetTableHTML("adm_epg_channels_schedule_new.aspx?submited=1");
        return sTable;
    }
}
