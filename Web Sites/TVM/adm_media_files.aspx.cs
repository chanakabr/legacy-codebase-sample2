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
using KLogMonitor;
using System.Reflection;
using System.Collections.Generic;
using TvinciImporter;

public partial class adm_media_files : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (string.IsNullOrEmpty(Request.QueryString["media_file_id"]))
        {
            log.Debug("Session key - Media_File_Removed");
            Session["media_file_id"] = null;
            //Session.Remove("media_file_id");

        }
        else
        {
            log.Debug("Session key - Media file not null " + Request.QueryString["media_file_id"]);
        }
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_media.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            //if (Request.QueryString["search_save"] != null)
            //Session["search_save"] = "1";
            //else
            //Session["search_save"] = null;

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
            }
            else if (Session["media_id"] == null || Session["media_id"].ToString() == "" || Session["media_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("media", "NAME", int.Parse(Session["media_id"].ToString())).ToString() + " Content Files ");
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
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select q.ID,q.editor_remarks,q.order_num,q.is_active,q.OVERRIDE_PLAYER_TYPE_ID,q.status,q.mt_description as 'Media Type',q.mq_description as 'Media Quality',q.bt_description as 'Billing type',q.views as 'Views',q.s_description as 'State',q.ADS_ENABLED as 'Ads Enabled' from (select mf.is_active,mf.order_num,mf.OVERRIDE_PLAYER_TYPE_ID,mf.id,mf.status,lmt.description as 'mt_description',lmq.description as 'mq_description',lbt.description as 'bt_description',mf.views,lcs.description as 's_description',mf.ADS_ENABLED,mf.editor_remarks from lu_billing_type lbt,lu_content_status lcs,lu_media_quality lmq,groups_media_type lmt,media_files mf where lbt.id=mf.billing_type_id and lcs.id=mf.status and mf.MEDIA_TYPE_ID=lmt.MEDIA_TYPE_ID and mf.MEDIA_QUALITY_ID=lmq.id and " + PageUtils.GetStatusQueryPart("mf") + "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", int.Parse(Session["media_id"].ToString()));
        theTable += "AND";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("lmt.group_id", "=", nGroupID);
        theTable += ")q";
        //theTable.AddHiddenField("ID");
        theTable.AddHiddenField("is_active");
        DataTableMultiValuesColumn multi_tags = new DataTableMultiValuesColumn("Streaming Type", "val", "lpd.id", "OVERRIDE_PLAYER_TYPE_ID");
        multi_tags += "select lpd.description as val from lu_player_descriptions lpd where ";
        theTable.AddMultiValuesColumn(multi_tags);

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) &&
            LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            theTable.AddActivationField("media_files");

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_files_ppvmodules.aspx", "PayPerView modules", "");
            linkColumn1.AddQueryStringValue("media_file_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_finacial_contracts.aspx", "Financial contracts", "");
            linkColumn1.AddQueryStringValue("media_file_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }


        theTable.AddHiddenField("status");
        theTable.AddHiddenField("OVERRIDE_PLAYER_TYPE_ID");
        theTable.AddVideoField("media_files");
        theTable.AddOrderNumField("media_files", "ID", "order_num", "Order number");
        theTable.AddHiddenField("order_num");
        theTable.AddActivationField("media_files", "adm_media_files.aspx");
        theTable.AddOnOffField("Ads Enabled", "media_files~~|~~ADS_ENABLED~~|~~id~~|~~Yes~~|~~No");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.order_num,q.id desc";
        theTable.AddTechDetails("media_files");
        theTable.AddEditorRemarks("media_files");
        theTable.AddHiddenField("EDITOR_REMARKS");

        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_files_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("media_file_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media_files");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media_files");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media_files");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_media.aspx";
        Session["LastContentPage"] = "adm_media_files.aspx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();        
        int mediaFileID;
        if (int.TryParse(sID, out mediaFileID))
        {
            Int32 nMediaID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "media_id", mediaFileID).ToString());
            if (nMediaID > 0)
            {
                if (!ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, nGroupID, ApiObjects.eAction.Update))
                {
                    log.Error(string.Format("Failed updating index for mediaID: {0}, groupID: {1}", nMediaID, nGroupID));
                }
            }
        }
    }

}