using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_media_concurrency_rule_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media_concurrency_rule.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_media_concurrency_rule.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {

                DBManipulator.DoTheWork();

                int groupId = LoginManager.GetLoginGroupID();
                // invalidation keys
                string invalidationKey = LayeredCacheKeys.GetGroupMediaConcurrencyRulesKey(groupId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for media concurrency rules. key = {0}", invalidationKey);
                }
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["rule_id"] != null &&
                Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
            }
            else
                Session["rule_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Device Managment:  Rules: MediaConcurrency");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && int.Parse(Session["rule_id"].ToString()) != 0)
            t = Session["rule_id"];
        string sBack = "adm_media_concurrency_rule.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_concurrency_rules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 1024);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "name", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_media_concurrency_limit = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_media_concurrency_limit.Initialize("Media Concurrency Limit", "adm_table_header_nbg", "FormInput", "media_concurrency_limit", false);
        theRecord.AddRecord(dr_media_concurrency_limit);

        

        DataRecordDropDownField dr_tag_type = new DataRecordDropDownField("media_tags_types", "NAME", "id", "", null, 60, true);
        string sQuery = " select (mtt.name + ' - ' + g.GROUP_NAME) as txt, mtt.id as id from media_tags_types  mtt inner	join groups g on		mtt.group_id = g.id "+
                        " where	mtt.status =1 and	g.parent_group_id = " + LoginManager.GetLoginGroupID().ToString();
        dr_tag_type.SetSelectsQuery(sQuery);
        dr_tag_type.Initialize("Tag Type", "adm_table_header_nbg", "FormInput", "tag_type_id", false);
        theRecord.AddRecord(dr_tag_type);


        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordDropDownField restrictionPolicy = new DataRecordDropDownField("", "NAME", "ID", "", null, 60, true);
        DataTable restrictionPoliciesTable = new DataTable("restriction_policies");
        restrictionPoliciesTable.Columns.Add("id", typeof(int));
        restrictionPoliciesTable.Columns.Add("txt", typeof(string));

        foreach (ApiObjects.ConcurrencyRestrictionPolicy r in Enum.GetValues(typeof(ApiObjects.ConcurrencyRestrictionPolicy)))
        {
            restrictionPoliciesTable.Rows.Add((int)r, r);
        }

        restrictionPolicy.SetSelectsDT(restrictionPoliciesTable);
        restrictionPolicy.Initialize("Restriction Policy", "adm_table_header_nbg", "FormInput", "restriction_policy", false);
        theRecord.AddRecord(restrictionPolicy);

        string sTable = theRecord.GetTableHTML("adm_media_concurrency_rule_new.aspx?submited=1");

        return sTable;
    }
    
}