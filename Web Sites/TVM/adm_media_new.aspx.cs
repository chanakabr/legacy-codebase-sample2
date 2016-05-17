using KLogMonitor;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_media_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork();
                ProtocolsFuncs.SeperateMediaMainTexts(nID);
                ProtocolsFuncs.SeperateMediaMainTags(nID);

                if (nID > 0) //if record was save , update record in Lucene , create Notification Requests
                {
                    // Update record in Catalog (see the flow inside Update Index
                    int nGroupId = LoginManager.GetLoginGroupID();
                    if (!ImporterImpl.UpdateIndex(new List<int>() { nID }, nGroupId, ApiObjects.eAction.Update))
                    {
                        log.Error(string.Format("Failed updating index for mediaID: {0}, groupID: {1}", nID, nGroupId));
                    }

                    // update notification                     
                    object isActiveAsset = ODBCWrapper.Utils.GetTableSingleVal("media", "is_active", nID);
                    if (isActiveAsset != null && isActiveAsset != DBNull.Value && isActiveAsset.ToString().Equals("1"))
                        ImporterImpl.UpdateNotificationsRequests(LoginManager.GetLoginGroupID(), nID);
                }

                try
                {
                    Notifiers.BaseMediaNotifier t = null;
                    Notifiers.Utils.GetBaseMediaNotifierImpl(ref t, LoginManager.GetLoginGroupID());

                    string errorMessage = "";

                    if (t != null)
                    {
                        t.NotifyChange(nID.ToString(), ref errorMessage);
                    }

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        HttpContext.Current.Session["error_msg_sub"] = "Error in Package ID " + nID + ":\r\n" + errorMessage;
                    }


                    //if (t != null)
                    //    t.NotifyChange(nID.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + nID.ToString() + " : " + ex.Message, ex);
                }

                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
            {
                Session["media_id"] = int.Parse(Request.QueryString["media_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                m_sLangMenu = GetLangMenu(nOwnerGroupID);
            }
            else
                Session["media_id"] = 0;
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
            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_media_new.aspx?media_id=" + Session["media_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
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
                    sTemp += "<li><a href=\"";
                    sTemp += "adm_media_translate.aspx?media_id=" + Session["media_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
        Response.Write(PageUtils.GetPreHeader() + ": Media management");
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

    protected void AddTagsFields(ref DBRecordWebEditor theRecord)
    {
        string sGroups = PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID());
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from media_tags_types where status=1 and TagFamilyID IS NULL and group_id " + sGroups;

        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());

        selectQuery += "order by order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sTagTypeName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                Int32 nTagTypeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "media_tags", "media_id", "TAG_ID", true, "ltr", 60, "tags");
                dr_tags.Initialize(sTagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);
                dr_tags.SetCollectionLength(8);
                dr_tags.SetExtraWhere("TAG_TYPE_ID=" + nTagTypeID.ToString());
                theRecord.AddRecord(dr_tags);
            }
        }
        selectQuery.Finish();
        selectQuery = new DataSetSelectQuery();
        selectQuery += "select * from media_tags_types where status=1 and group_id=0 and TagFamilyID = 1";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sTagTypeName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                Int32 nTagTypeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "media_tags", "media_id", "TAG_ID", true, "ltr", 60, "tags");
                dr_tags.Initialize(sTagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);
                dr_tags.SetCollectionLength(8);
                dr_tags.SetExtraWhere("TAG_TYPE_ID=" + nTagTypeID.ToString());
                theRecord.AddRecord(dr_tags);
            }
        }
        selectQuery = null;
        {
            DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "media_tags", "media_id", "TAG_ID", true, "ltr", 60, "tags");
            dr_tags.Initialize("Free", "adm_table_header_nbg", "FormInput", "VALUE", false);
            dr_tags.SetCollectionLength(8);
            dr_tags.SetExtraWhere("TAG_TYPE_ID=0");
            theRecord.AddRecord(dr_tags);
        }
    }

    protected void AddStrFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 21; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_STR_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_STR";
                        //DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 255);
                        DataRecordLongTextField dr_name = new DataRecordLongTextField("ltr", true, 60, 3);
                        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
                        theRecord.AddRecord(dr_name);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void AddIntFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 11; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_DOUBLE_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_DOUBLE";
                        DataRecordShortDoubleField dr_name = new DataRecordShortDoubleField(true, 12, 12);
                        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
                        theRecord.AddRecord(dr_name);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void AddBoolFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 11; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_BOOL_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_BOOL";
                        //DataRecordCheckBoxField dr_name = new DataRecordCheckBoxField(true);
                        DataRecordBoolField dr_name = new DataRecordBoolField(true);
                        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
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
        object mediaId = null; ;
        if (Session["media_id"] != null && Session["media_id"].ToString() != "" && int.Parse(Session["media_id"].ToString()) != 0)
            mediaId = Session["media_id"];
        string sBack = "adm_media.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media", "adm_table_pager", sBack, "", "ID", mediaId, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordDateTimeField dr_catalog_start_date = new DataRecordDateTimeField(true);
        dr_catalog_start_date.Initialize("Catalog Start Date", "adm_table_header_nbg", "FormInput", "CATALOG_START_DATE", false);
        dr_catalog_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_catalog_start_date);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("Catalog End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        theRecord.AddRecord(dr_end_date);

        DataRecordDateTimeField dr_final_end_date = new DataRecordDateTimeField(true);
        dr_final_end_date.Initialize("Final End Date", "adm_table_header_nbg", "FormInput", "FINAL_END_DATE", false);
        theRecord.AddRecord(dr_final_end_date);

        DataRecordShortTextField dr_co_guid = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_co_guid.Initialize("Outer Guid(connection to outer feed)", "adm_table_header_nbg", "FormInput", "CO_GUID", false);
        theRecord.AddRecord(dr_co_guid);

        DataRecordShortTextField dr_entry_id = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_entry_id.Initialize("Entry Identifier", "adm_table_header_nbg", "FormInput", "ENTRY_ID", false);
        theRecord.AddRecord(dr_entry_id);

        DataRecordShortTextField dr_epg_guid = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_epg_guid.Initialize("EPG Guid(connection to the EPG)", "adm_table_header_nbg", "FormInput", "EPG_IDENTIFIER", false);
        theRecord.AddRecord(dr_epg_guid);

        // if media is not new - upload pic is allowed
        if (mediaId != null && !string.IsNullOrEmpty(mediaId.ToString()))
        {
            bool isDownloadPicWithImageServer = false;
            string imageUrl = string.Empty;
            int picId = 0;

            if (ImageUtils.IsDownloadPicWithImageServer())
            {
                isDownloadPicWithImageServer = true;
                int groupId = LoginManager.GetLoginGroupID();
                imageUrl = GetPicImageUrlByRatio(mediaId, groupId, out picId);
            }

            DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField("media", isDownloadPicWithImageServer, imageUrl, picId);
            dr_pic.Initialize("Thumb", "adm_table_header_nbg", "FormInput", "MEDIA_PIC_ID", false);
            theRecord.AddRecord(dr_pic);
        }

        DataRecordRadioField dr_type = new DataRecordRadioField("media_types", "NAME", "id", "", null);
        string sQuery = "select name as txt,id as id from media_types where status=1 and group_id " + PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID()) + " order by ORDER_NUM";
        dr_type.SetSelectsQuery(sQuery);
        dr_type.Initialize("Media type", "adm_table_header_nbg", "FormInput", "MEDIA_TYPE_ID", true);
        dr_type.SetDefault(1);
        theRecord.AddRecord(dr_type);

        string sDefWP = "";
        object oDefWP = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_WATCH_PERMISSION_TYPE_ID", LoginManager.GetLoginGroupID());
        if (oDefWP != DBNull.Value && oDefWP != null)
            sDefWP = oDefWP.ToString();
        string sDefBR = "";
        object oDefBR = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_BLOCK_TEMPLATE_ID", LoginManager.GetLoginGroupID());
        if (oDefBR != DBNull.Value && oDefBR != null)
            sDefBR = oDefBR.ToString();
        string sDefPR = "";
        object oDefPR = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_PLAYERS_RULES", LoginManager.GetLoginGroupID());
        if (oDefPR != DBNull.Value && oDefPR != null)
            sDefPR = oDefPR.ToString();

        DataRecordDropDownField dr_watch_permissions = new DataRecordDropDownField("watch_permissions_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from watch_permissions_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_watch_permissions.SetSelectsQuery(sQuery);
        dr_watch_permissions.Initialize("Watch Permission Rule", "adm_table_header_nbg", "FormInput", "WATCH_PERMISSION_TYPE_ID", false);
        dr_watch_permissions.SetDefaultVal(sDefWP);
        theRecord.AddRecord(dr_watch_permissions);

        DataRecordDropDownField dr_block_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from geo_block_types where GEO_RULE_TYPE=1 and status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_block_rules.SetSelectsQuery(sQuery);
        dr_block_rules.Initialize("Geo block Rule", "adm_table_header_nbg", "FormInput", "BLOCK_TEMPLATE_ID", false);
        dr_block_rules.SetDefaultVal(sDefBR);
        theRecord.AddRecord(dr_block_rules);

        DataRecordDropDownField dr_players_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from players_groups_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_players_rules.SetSelectsQuery(sQuery);
        dr_players_rules.Initialize("Players Rule", "adm_table_header_nbg", "FormInput", "PLAYERS_RULES", false);
        dr_players_rules.SetDefaultVal(sDefPR);
        theRecord.AddRecord(dr_players_rules);

        DataRecordDropDownField dr_device_rules = new DataRecordDropDownField("device_rules", "NAME", "ID", "GROUP_ID", LoginManager.GetLoginGroupID(), 60, true);
        sQuery = "select name as txt,id as id from device_rules where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_device_rules.SetSelectsQuery(sQuery);
        dr_device_rules.Initialize("Device rule", "adm_table_header_nbg", "FormInput", "device_rule_id", false);
        theRecord.AddRecord(dr_device_rules);



        AddStrFields(ref theRecord);
        AddIntFields(ref theRecord);
        AddBoolFields(ref theRecord);
        AddTagsFields(ref theRecord);
        //DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "pics_tags", "media_id", "TAG_ID", true, "ltr", 60, "tags");
        //dr_tags.Initialize("Tags", "adm_table_header_nbg", "FormInput", "VALUE", true);
        //dr_tags.SetCollectionLength(8);
        //dr_tags.SetExtraWhere("TAG_TYPE_ID=0");
        //theRecord.AddRecord(dr_tags);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_media_new.aspx?submited=1");

        return sTable;
    }

    private string GetPicImageUrlByRatio(object mediaId, int groupId, out int picId)
    {
        string imageUrl = string.Empty;
        string baseUrl = string.Empty;
        int ratioId = 0;
        int version = 0;
        picId = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.RATIO_ID, p.BASE_URL, p.ID, p.version from pics p left join media m on m.MEDIA_PIC_ID = p.ID where p.STATUS in (0, 1) and m.id = " + mediaId.ToString();

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
        {
            baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
            ratioId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["RATIO_ID"]);
            picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
            version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
            int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

            imageUrl = PageUtils.BuildVodUrl(parentGroupID, baseUrl, ratioId, version);
        }

        return imageUrl;
    }
}
