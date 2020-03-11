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

public partial class adm_account_permissions : System.Web.UI.Page
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
                if (Request.QueryString["per_parent_id"] != null && Request.QueryString["per_parent_id"].ToString() != "")
                    Session["per_parent_id"] = int.Parse(Request.QueryString["per_parent_id"].ToString());
                else
                    Session["per_parent_id"] = 0;

                if (Request.QueryString["account_id"] != null && Request.QueryString["account_id"].ToString() != "")
                    Session["account_permission_id"] = int.Parse(Request.QueryString["account_id"].ToString());
                else if (Session["account_permission_id"] == null)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            catch
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
        theTable += "select aap.id as aap_id,am.id,am.menu_text as 'Main menu', aap.view_permit as 'Open/Close',aap.EDIT_PERMIT as 'Edit',aap.NEW_PERMIT as 'New',aap.REMOVE_PERMIT as 'Delete',aap.PUBLISH_PERMIT as 'Publish' from admin_menu am,admin_accounts_permissions aap where ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("am.parent_menu_id" , "=" , Session["per_parent_id"]);
        theTable += " and aap.menu_id=am.id and am.menu_text<>'' and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("account_id" , "=" , int.Parse(Session["account_permission_id"].ToString()));
        theTable += " and aap.MENU_ID in (select distinct menu_id from admin_accounts_permissions where PUBLISH_PERMIT=1 and view_permit=1 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", LoginManager.GetLoginID());
        theTable += ") and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", int.Parse(Session["account_permission_id"].ToString()));
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
        theTable.AddOnOffField("Open/Close", "admin_accounts_permissions~~|~~view_permit~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("Edit", "admin_accounts_permissions~~|~~edit_permit~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("New", "admin_accounts_permissions~~|~~NEW_PERMIT~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("Delete", "admin_accounts_permissions~~|~~REMOVE_PERMIT~~|~~aap_id~~|~~Open~~|~~Close");
        theTable.AddOnOffField("Publish", "admin_accounts_permissions~~|~~PUBLISH_PERMIT~~|~~aap_id~~|~~Open~~|~~Close");
        if (LoginManager.IsActionPermittedOnPage("adm_accounts.aspx" , LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_account_permissions.aspx", "Sub menu's", "");
            linkColumn1.AddQueryStringValue("per_parent_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
    }

    public void GetBackLink()
    {
        Int32 nPar = 0;
        if (Session["per_parent_id"] != null)
            nPar = int.Parse(Session["per_parent_id"].ToString());
        object o = ODBCWrapper.Utils.GetTableSingleVal("admin_menu", "PARENT_MENU_ID", nPar);
        if (o != null && o != DBNull.Value)
            Response.Write("<td onclick='window.document.location.href=\"adm_account_permissions.aspx?per_parent_id=" + o.ToString() + "\";'><a href=\"#\" class=\"btn_back\"></a></td>");
        else
            Response.Write("<td onclick='window.document.location.href=\"adm_accounts.aspx\";'><a href=\"#\" class=\"btn_back\"></a></td>");
            
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
        Response.Write(PageUtils.GetPreHeader() + "Main menu permissions for account: " + sUN);
    }
}
