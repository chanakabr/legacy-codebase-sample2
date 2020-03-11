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

public partial class adm_account_permissions_inner : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_accounts.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(1, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            try
            {
                if (Session["account_permission_id"] == null || Session["account_permission_id"].ToString() == "" || Session["account_permission_id"].ToString() == "0")
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                if (Request.QueryString["main_menu_id"] != null && Request.QueryString["main_menu_id"].ToString() != "")
                    Session["account_main_menu_id"] = int.Parse(Request.QueryString["main_menu_id"].ToString());
                else
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }

                Int32 nAccountID = int.Parse(Session["account_permission_id"].ToString());
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                bool bBelongs = PageUtils.DoesAccountBelongToGroup(nAccountID, nGroupID);
                if (bBelongs == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            catch
            {
                LoginManager.LogoutFromSite("login.html");
            }
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
        theTable += "select aap.id as aap_id,am.id,am.menu_text as 'Menu name', aap.view_permit as 'View',aap.EDIT_PERMIT as 'Edit',aap.NEW_PERMIT as 'New',aap.REMOVE_PERMIT as 'Delete',aap.PUBLISH_PERMIT as 'Publish' from admin_menu am,admin_accounts_permissions aap where aap.menu_id=am.id and am.menu_text<>'' and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("aap.account_id", "=", int.Parse(Session["account_permission_id"].ToString()));
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("am.parent_menu_id", "=", int.Parse(Session["account_main_menu_id"].ToString()));
        if (nGroupID > 1)
            theTable += " and am.only_tvinci=0 ";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by MENU_ORDER_VIS";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("aap_id");
        theTable.AddOrderByColumn("Main menu", "am.menu_text");
        theTable.AddOnOffField("View", "admin_accounts_permissions~~|~~view_permit~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("Edit", "admin_accounts_permissions~~|~~edit_permit~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("New", "admin_accounts_permissions~~|~~NEW_PERMIT~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("Delete", "admin_accounts_permissions~~|~~REMOVE_PERMIT~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("Publish", "admin_accounts_permissions~~|~~PUBLISH_PERMIT~~|~~aap_id~~|~~Open~~|~~Close");

    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        string sUN = LoginManager.GetLoginName(int.Parse(Session["account_permission_id"].ToString()));
        string sMainName = PageUtils.GetTableSingleVal("admin_menu" , "MENU_TEXT" , int.Parse(Session["account_main_menu_id"].ToString())).ToString();
        Response.Write(PageUtils.GetPreHeader() + "Sub menu permissions for account: " + sUN + " on Menu: " + sMainName);
    }
}
