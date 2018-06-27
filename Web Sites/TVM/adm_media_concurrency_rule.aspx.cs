using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Reflection;

public partial class adm_media_concurrency_rule : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
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

        theTable += "select m.id as id, m.NAME , m.status , m.is_active , m.media_concurrency_limit as 'Media Concurrency Limit',  m.tag_type_id as 'TagTypeID'  , mtt.name as 'Tag Type' " +
                    " from media_concurrency_rules m  left join media_tags_types mtt on	m.tag_type_id = mtt.id " +
        " where m.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");

        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("TagTypeID");
        theTable.AddActivationField("media_concurrency_rules"); // On/Off property
        
        // add column for tagValues
        DataTableLinkColumn linkColumnTagValue = new DataTableLinkColumn("adm_media_concurrency_tag_type_values.aspx", "TagsValues", "");
        linkColumnTagValue.AddQueryStringValue("TagTypeID", "field=TagTypeID");        
        linkColumnTagValue.AddQueryStringValue("rule_id", "field=id");
        linkColumnTagValue.AddQueryCounterValue("select count(*) as val from media_concurrency_rules_values where status=1 and is_active=1 and RULE_ID=", "field=ID");
        theTable.AddLinkColumn(linkColumnTagValue);

        // add column for usgaeModules
        DataTableLinkColumn linkColumnUUsgaeModulese = new DataTableLinkColumn("adm_media_concurrency_business_modules.aspx", "BusinessModels", "");
        linkColumnUUsgaeModulese.AddQueryStringValue("rule_id", "field=ID");
        linkColumnUUsgaeModulese.AddQueryCounterValue("select count(*) as val from media_concurrency_bm where status=1 and is_active=1 and RULE_ID=", "field=ID");
        theTable.AddLinkColumn(linkColumnUUsgaeModulese);  



        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_concurrency_rule_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("rule_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        #region Reomve + Publish buttons
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media_concurrency_rules");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "5");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media_concurrency_rules");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "5");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "media_concurrency_rules");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "5");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }
        #endregion
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Device Managment:  Rules: MediaConcurrency");
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        int groupId = LoginManager.GetLoginGroupID();
        // invalidation keys
        string invalidationKey = LayeredCacheKeys.GetGroupMediaConcurrencyRulesKey(groupId);
        if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
        {
            log.ErrorFormat("Failed to set invalidation key for media concurrency rules. key = {0}", invalidationKey);
        }
    }
}