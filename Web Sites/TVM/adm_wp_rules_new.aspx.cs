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

public partial class adm_wp_rules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["watch_permission_id"] != null &&
                Request.QueryString["watch_permission_id"].ToString() != "")
            {
                Session["watch_permission_id"] = int.Parse(Request.QueryString["watch_permission_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("watch_permissions_types", "group_id", int.Parse(Session["watch_permission_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["watch_permission_id"] = 0;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();

                Int32 groupId = LoginManager.GetLoginGroupID();
                int parentGroupId = Convert.ToInt32(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", groupId));

                if (parentGroupId != 0)
                {
                    groupId = parentGroupId;
                }

                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                groupManager.UpdateGroup(groupId);
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Watch permissions rules";
        if (Session["watch_permission_id"] != null && Session["watch_permission_id"].ToString() != "" && Session["watch_permission_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
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
        if (Session["watch_permission_id"] != null && Session["watch_permission_id"].ToString() != "" && int.Parse(Session["watch_permission_id"].ToString()) != 0)
            t = Session["watch_permission_id"];
        string sBack = "adm_wp_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("watch_permissions_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Rule name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_domain);

        DataRecordRadioField dr_but_or_only = new DataRecordRadioField("lu_only_or_but", "description", "id", "", null);
        dr_but_or_only.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "ONLY_OR_BUT", true);
        dr_but_or_only.SetDefault(0);
        theRecord.AddRecord(dr_but_or_only);

        DataRecordMultiField dr_groups_sel = new DataRecordMultiField("groups", "id", "id", "watch_permissions_types_groups", "watch_permissions_type_ID", "GROUP_ID", false, "ltr", 60, "tags");
        dr_groups_sel.SetCollectionLength(100);
        dr_groups_sel.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_NAME", false);
        dr_groups_sel.SetOrderCollectionBy("newid()");
        dr_groups_sel.SetExtraWhere("id " + PageUtils.GetAllGroupTreeStr());
        string sQuery = "select top 100 GROUP_NAME as txt,id as val from groups where status<>2 and id ";
        sQuery += PageUtils.GetAllGroupTreeStr();
        sQuery += "order by GROUP_NAME";
        dr_groups_sel.SetCollectionQuery(sQuery);
        theRecord.AddRecord(dr_groups_sel);

        bool bVisible = PageUtils.IsTvinciUser();
        if (bVisible == true)
        {
            DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
            dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
            theRecord.AddRecord(dr_groups);
        }
        else
        {
            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);
        }

        string sTable = theRecord.GetTableHTML("adm_wp_rules_new.aspx?submited=1");

        return sTable;
    }
}
