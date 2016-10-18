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
using TvinciImporter;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using System.Collections.Generic;
using System.Threading;

public partial class adm_media : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;


    // Handle error messages from package propagations (Eutelsat) 
    protected void Page_PreRender(object sender, EventArgs e)
    {
        if (HttpContext.Current.Session["error_msg_sub"] != null || Session["error_msg_sub"] != null)
        {
            hfError.Value = (HttpContext.Current.Session["error_msg_sub"] != null)
                ? Session["error_msg_sub"].ToString()
                : HttpContext.Current.Session["error_msg_sub"].ToString();

            HttpContext.Current.Session["error_msg_sub"] = null;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        log.Debug("MediaPage - MediaPage");
        if (LoginManager.CheckLogin() == false)
        {
            log.Debug("False Login - Login Is False");
            Response.Redirect("login.html");
        }
        if (LoginManager.IsPagePermitted() == false)
        {
            log.Debug("Page Not Permitted - Page Not Permitted");
            LoginManager.LogoutFromSite("login.html");
        }
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            log.Debug("AMS - AMS");
            return;
        }
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
            {
                Session["search_save"] = "1";
            }
            else
                Session["search_save"] = null;
        }

        Session["aa"] = "adm_media.aspx";
    }
   
    protected void NotifyMediaActivationChange(Int32 nMediaID, Int32 nStatus)
    {
        string sURL = "";
        string sXML = "";
        Int32 nLanguageID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select g.LANGUAGE_ID,g.media_notify_url from groups g,media m where g.id=m.group_id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oNURL = selectQuery.Table("query").DefaultView[0].Row["media_notify_url"];
                if (oNURL != DBNull.Value && oNURL != null)
                    sURL = oNURL.ToString();
                nLanguageID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (sURL == "")
            return;
        //sXML
        sXML = "<notification type=\"";
        if (nStatus == 0)
            sXML += "unpublish";
        if (nStatus == 1)
            sXML += "publish";
        sXML += "\"><media_info media_id=\"" + nMediaID.ToString() + "\">";
        System.Xml.XmlNode tNode = null;
        ApiObjects.MediaInfoObject theInfo = null;
        ApiObjects.MediaStatistics theMediaStatistics = null;
        ApiObjects.MediaPersonalStatistics thePersonalStatistics = null;
        sXML += TVinciShared.ProtocolsFuncs.GetMediaInfoInner(nMediaID, nLanguageID, true, 0, true, ref tNode, false, false, false, ref theInfo,
            ref thePersonalStatistics, ref theMediaStatistics);
        sXML += "</media_info>";
        sXML += "</notification>";

        Notifier t = new Notifier(sURL, sXML);
        ThreadStart job = new ThreadStart(t.Notify);
        Thread thread = new Thread(job);
        thread.Start();
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

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        return "";
    }

    static protected bool IsANumber(string sToCheck)
    {
        try
        {
            double d = double.Parse(sToCheck);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected void InsertStrMetaToTable(ref DBTableWebEditor theTable, Int32 nGroupID, bool bWithQ)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select * from groups with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int j = 1; j < 21; j++)
                {
                    string sFieldVal = "META" + j.ToString() + "_STR";
                    string sField = "IS_META" + j.ToString() + "_STR_ON_TABLE";
                    string sFieldName = "META" + j.ToString() + "_STR_NAME";

                    Int32 nOnOff = int.Parse(selectQuery.Table("query").DefaultView[0].Row[sField].ToString());
                    string sFieldNameVal = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                    if (nOnOff == 1)
                    {
                        theTable += ",";
                        string sFN = "";
                        if (bWithQ == true)
                            sFN = "q.";
                        else
                            sFN = "m.";
                        sFN += sFieldVal;
                        if (bWithQ == true)
                            sFN += " as '" + sFieldNameVal + "'";
                        theTable += sFN;
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }
    protected void InsertMetasToTable(ref DBTableWebEditor theTable, Int32 nGroupID)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select * from media_tags_types where status=1 and IS_ON_TABLE=1 and group_id " + PageUtils.GetParentsGroupsStr(nGroupID);
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        selectQuery += " order by order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int j = 0; j < nCount; j++)
            {
                Int32 nTagType = int.Parse(selectQuery.Table("query").DefaultView[j].Row["ID"].ToString());
                string sFieldNameVal = selectQuery.Table("query").DefaultView[j].Row["NAME"].ToString();

                DataTableMultiValuesColumn multi_tag = new DataTableMultiValuesColumn(sFieldNameVal, "val", "mt.media_id", "ID");
                multi_tag += "select t.value as val from tags t,media_tags mt where mt.tag_id=t.id and mt.status=1 and t.status=1 and t.TAG_TYPE_ID=" + nTagType.ToString() + " and ";
                theTable.AddMultiValuesColumn(multi_tag);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

    }

    protected void InsertDoubleMetaToTable(ref DBTableWebEditor theTable, Int32 nGroupID, bool bWithQ)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select * from groups with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int j = 1; j < 11; j++)
                {
                    string sFieldVal = "META" + j.ToString() + "_DOUBLE";
                    string sField = "IS_META" + j.ToString() + "_DOUBLE_ON_TABLE";
                    string sFieldName = "META" + j.ToString() + "_DOUBLE_NAME";

                    Int32 nOnOff = int.Parse(selectQuery.Table("query").DefaultView[0].Row[sField].ToString());
                    string sFieldNameVal = selectQuery.Table("query").DefaultView[0].Row[sFieldName].ToString();
                    if (nOnOff == 1)
                    {
                        theTable += ",";
                        string sFN = "";
                        if (bWithQ == true)
                            sFN = "q.";
                        else
                            sFN = "m.";
                        sFN += sFieldVal;
                        if (bWithQ == true)
                            sFN += " as '" + sFieldNameVal + "'";
                        theTable += sFN;
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void AddCommentFields(ref DBTableWebEditor theTable, Int32 nGroupID)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from comment_types where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        selectQuery += " order by order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nCommentID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                {
                    DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_comments.aspx", sName, "");
                    linkColumn1.AddQueryStringValue("media_id", "field=id");
                    linkColumn1.AddQueryStringValue("comment_type_id", nCommentID.ToString());
                    theTable.AddLinkColumn(linkColumn1);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select q.editor_remarks,q.is_active,q.status as 'status1',q.s_id as 'MID',p.base_url as 'Pic',q.NAME as 'Name', ISNULL(q.catalog_start_date,q.start_date) as 'Catalog Start Date'  ,q.start_date as 'Start Date',q.end_date as 'Catalog End Date',q.description as 'Description',q.PLAYER_CONTROL_ADS as 'Ads Controller'";
        InsertStrMetaToTable(ref theTable, nGroupID, true);
        InsertDoubleMetaToTable(ref theTable, nGroupID, true);
        theTable += ",q.CO_GUID as 'Outer GUID',q.EPG_IDENTIFIER as 'EPG GUID',q.s_id as 'id',q.s_desc as 'Status', pic_id from (select distinct m.is_active,m.is_active as 'q_ia'";
        InsertStrMetaToTable(ref theTable, nGroupID, false);
        InsertDoubleMetaToTable(ref theTable, nGroupID, false);
        theTable += ",m.editor_remarks,m.MEDIA_PIC_ID as 'pic_id',m.PLAYER_CONTROL_ADS,m.CO_GUID,m.EPG_IDENTIFIER,m.status,m.NAME as 'NAME',m.DESCRIPTION as 'Description',m.id as 's_id',lcs.description as 's_desc',CONVERT(VARCHAR(10),m.CATALOG_START_DATE, 104) as 'Catalog_Start_Date',CONVERT(VARCHAR(10),m.START_DATE, 104) as 'Start_Date',CONVERT(VARCHAR(10),m.End_DATE, 104) as 'End_Date',CONVERT(VARCHAR(10),m.Final_End_DATE, 104) as 'Final_End_Date'  from media m with (nolock),lu_content_status lcs with (nolock)";
        if (Session["search_tag"] != null && Session["search_tag"].ToString() != "")
            theTable += ",tags t,media_tags mt ";
        if (Session["search_only_unapproved_comments"] != null && Session["search_only_unapproved_comments"].ToString() != "")
            theTable += ",media_comments mc ";


        theTable += "where m.status<>2 and lcs.id=m.status ";
        if (Session["search_tag"] != null && Session["search_tag"].ToString() != "")
        {
            string sL = "LTRIM(RTRIM(LOWER(t.value))) like (N'%" + Session["search_tag"].ToString().ToLower().Trim() + "%')";
            theTable += " and t.status=1 and mt.status=1 and t.id=mt.tag_id and mt.media_ID=m.id and " + sL;
        }
        if (Session["search_only_unapproved_comments"] != null && Session["search_only_unapproved_comments"].ToString() != "")
        {
            theTable += " and mc.MEDIA_ID=m.id and mc.status=1 and mc.IS_ACTIVE=0 and mc.COMMENT_TYPE_ID=0 ";
        }
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free"].ToString() + "%')";
            theTable += " and (";
            theTable += "m.NAME " + sLike + " OR m.DESCRIPTION " + sLike + " OR m.META1_STR " + sLike + " OR m.META2_STR " + sLike + " OR m.META3_STR " + sLike + " OR m.META4_STR " + sLike + " OR m.META5_STR " + sLike + " OR m.META6_STR " + sLike + " OR m.META7_STR " + sLike + " OR m.META8_STR " + sLike + " OR m.META9_STR " + sLike + " OR m.META10_STR " + sLike;
            theTable += " OR m.META11_STR " + sLike + " OR m.META12_STR " + sLike + " OR m.META13_STR " + sLike + " OR m.META14_STR " + sLike + " OR m.META15_STR " + sLike + " OR m.META16_STR " + sLike + " OR m.META17_STR " + sLike + " OR m.META18_STR " + sLike + " OR m.META19_STR " + sLike + " OR m.META20_STR " + sLike;
            theTable += " OR m.EPG_IDENTIFIER " + sLike + " OR m.CO_GUID " + sLike;
            if (IsANumber(Session["search_free"].ToString()) == true)
            {
                double d = double.Parse(Session["search_free"].ToString());
                Int32 n = int.Parse(Session["search_free"].ToString());
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.ID", "=", n);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META1_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META2_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META3_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META4_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META5_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META6_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META7_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META8_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META9_DOUBLE", "=", d);
                theTable += " OR ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("m.META10_DOUBLE", "=", d);
            }
            theTable += ")";
        }
        if (Session["search_on_off"] != null && Session["search_on_off"].ToString() != "" && Session["search_on_off"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("m.is_active", "=", int.Parse(Session["search_on_off"].ToString()));
        }
        if (Session["search_tag_type"] != null && Session["search_tag_type"].ToString() != "" && Session["search_tag_type"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_TYPE_ID", "=", int.Parse(Session["search_tag_type"].ToString()));
        }

        theTable += "and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        theTable += " )q LEFT JOIN pics p ON p.id=q.pic_id and " + PageUtils.GetStatusQueryPart("p");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.s_id desc";
        InsertMetasToTable(ref theTable, nGroupID);
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status1");
        theTable.AddOrderByColumn("Name", "q.NAME");
        theTable.AddOrderByColumn("MID", "q.s_id");
        theTable.AddOrderByColumn("Status", "q.s_desc");
        theTable.AddOrderByColumn("Media ID", "q.s_id");
        theTable.AddImageField("Pic");
        theTable.AddTechDetails("media");
        theTable.AddEditorRemarks("media");
        theTable.AddHiddenField("EDITOR_REMARKS");
        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("pic_Id");
        theTable.AddOnOffField("Ads Controller", "media~~|~~PLAYER_CONTROL_ADS~~|~~id~~|~~Player~~|~~Owner");
        //string sNotifyURL = "";
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) &&
            LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            theTable.AddActivationField("media", "adm_media.aspx");
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_files.aspx", "Files", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from media_files where status=1 and is_active=1 and media_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) &&
            LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            theTable.AddActivationField("media", "adm_media.aspx");
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_locales.aspx", "Locale", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from media_locale_values where status=1 and is_active=1 and media_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_comments.aspx", "Comments", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            //linkColumn1.AddQueryCounterValue("select count(*) as val from media_comments where status=1 and is_active=0 and media_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        AddCommentFields(ref theTable, nGroupID);
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_cube.aspx", "Statistics", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS1=1;STATUS1=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS1=3;STATUS1=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS1=3;STATUS1=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }
    }
    
    public string GetPageContent(string sOrderBy, string sPageNum, string search_tag, string search_free, string search_on_off,
        string search_only_unapproved_comments, string search_tag_type)
    {
        if (search_tag != "")
            Session["search_tag"] = search_tag.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_tag"] = "";

        if (search_only_unapproved_comments != "-1" && search_only_unapproved_comments != "")
            Session["search_only_unapproved_comments"] = search_only_unapproved_comments;
        else if (Session["search_only_unapproved_comments"] == null || search_only_unapproved_comments == "-1")
            Session["search_only_unapproved_comments"] = "";

        if (search_tag_type != "-1")
            Session["search_tag_type"] = search_tag_type;
        else if (Session["search_save"] == null)
            Session["search_tag_type"] = "";

        if (search_on_off != "-1")
            Session["search_on_off"] = search_on_off;
        else if (Session["search_save"] == null)
            Session["search_on_off"] = "";

        if (search_free != "")
            Session["search_free"] = search_free.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 20);
        
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        eAction eAction;
        
        int nAction = int.Parse(sStatus);
        int nId = int.Parse(sID);
        List<int> idsToUpdate = new List<int>();
        if (nId != 0)
        {
            idsToUpdate.Add(nId);
        }

        if (nAction == 0)
        {
            eAction = eAction.Delete;
        }
        else // status sent is 1
        {
            eAction = eAction.Update;

            ImporterImpl.UpdateNotificationsRequests(nGroupID, nId);
        }

        if (!ImporterImpl.UpdateIndex(idsToUpdate, nGroupID, eAction))
        {
            log.Error(string.Format("Failed updating index for mediaIDs: {0}, groupID: {1}", idsToUpdate, nGroupID));
        }

        NotifyMediaActivationChange(int.Parse(sID), int.Parse(sStatus));
        Notifiers.BaseMediaNotifier t = null;
        Notifiers.Utils.GetBaseMediaNotifierImpl(ref t, LoginManager.GetLoginGroupID());
        if (t != null)
            t.NotifyChange(sID);
    }
}
