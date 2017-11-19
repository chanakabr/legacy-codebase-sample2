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

public partial class adm_media_concurrency_tag_type_values : System.Web.UI.Page
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
        Response.Write(PageUtils.GetPreHeader() + ": Group rule");
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

        string sTagTypeName = string.Empty;
        Int32 nTagTypeID = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select mtt.id, mtt.name from media_concurrency_rules mc, media_tags_types mtt where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mc.id", "=", t.ToString());
        selectQuery += "and mc.tag_type_id=mtt.id";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                sTagTypeName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                nTagTypeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());

                DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "media_concurrency_rules_values", "rule_id", "TAG_ID", true, "ltr", 60, "tags");
                dr_tags.SetCollectionLength(20);
                dr_tags.SetExtraWhere("TAG_TYPE_ID=" + nTagTypeID);
                dr_tags.SetCollectionQuery("SELECT TOP 20 value as txt, ID as val from tags where tag_type_id=" + nTagTypeID);
                dr_tags.Initialize(sTagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);

                theRecord.AddRecord(dr_tags);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_groups_rules_values.aspx?submited=1");

        return sTable;
    }
}