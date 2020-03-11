using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_bs_projects_new : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(17, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["project_id"] != null &&
                Request.QueryString["project_id"].ToString() != "")
            {
                Session["project_id"] = int.Parse(Request.QueryString["project_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("bs_projects", "group_id", int.Parse(Session["project_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["project_id"] = 0;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
                DBManipulator.DoTheWork();
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Bugs System: ";

        if (Session["project_id"] != null && Session["project_id"].ToString() != "" && Session["project_id"].ToString() != "0")
            sRet += ODBCWrapper.Utils.GetTableSingleVal("bs_projects", "Name", int.Parse(Session["project_id"].ToString()));
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
        if (Session["project_id"] != null && Session["project_id"].ToString() != "" && int.Parse(Session["project_id"].ToString()) != 0)
            t = Session["project_id"];
        string sBack = "adm_bs_projects.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("bs_projects", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_group_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_group_name.Initialize("Project Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_group_name);

        DataRecordShortTextField dr_desc = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_desc.Initialize("Short Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", true);
        theRecord.AddRecord(dr_desc);

        DataRecordMultiField dr_accounts = new DataRecordMultiField("accounts", "id", "id", "bs_projects_accounts", "project_id", "account_ID", false, "ltr", 60, "tags");
        dr_accounts.Initialize("Assigned Accounts", "adm_table_header_nbg", "FormInput", "USERNAME", false);
        string sQuery = "select USERNAME as txt,id as val from accounts where status=1 and group_id=" + LoginManager.GetLoginGroupID().ToString();
        sQuery += " order by USERNAME";
        dr_accounts.SetCollectionQuery(sQuery);
        theRecord.AddRecord(dr_accounts);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_bs_projects_new.aspx?submited=1");

        return sTable;
    }
}
