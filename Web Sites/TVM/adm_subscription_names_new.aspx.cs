using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_subscription_names_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 6);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("pricing_connection");

                try
                {
                    Notifiers.BaseSubscriptionNotifier t = null;
                    Notifiers.Utils.GetBaseSubscriptionsNotifierImpl(ref t, LoginManager.GetLoginGroupID(), "pricing_connection");
                    if (t != null)
                        t.NotifyChange(Session["subscription_id"].ToString());
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + Session["subscription_id"].ToString() + " : " + ex.Message, ex);
                }

                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["subscription_name_id"] != null &&
                Request.QueryString["subscription_name_id"].ToString() != "")
            {
                Session["subscription_name_id"] = int.Parse(Request.QueryString["subscription_name_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscription_names", "group_id", int.Parse(Session["subscription_name_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["subscription_name_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        if (Session["subscription_id"] == null ||
            Session["subscription_id"].ToString() == "")
            return;

        string sRet = PageUtils.GetPreHeader() + ": Subscription names (" + Session["subscription_id"].ToString() + ")";
        if (Session["subscription_name_id"] != null && Session["subscription_name_id"].ToString() != "" && Session["subscription_name_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public void GetBackLink()
    {
        string sBack = "adm_subscription_names.aspx?search_save=1&subscription_id=" + Session["subscription_id"].ToString();
        Response.Write(sBack);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["subscription_name_id"] != null && Session["subscription_name_id"].ToString() != "" && int.Parse(Session["subscription_name_id"].ToString()) != 0)
            t = Session["subscription_name_id"];
        string sBack = "adm_subscription_names.aspx?search_save=1&subscription_id=" + Session["subscription_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("subscription_names", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Description", "adm_table_header_nbg", "FormInput", "description", true);
        theRecord.AddRecord(dr_domain);

        DataRecordDropDownField dr_language = new DataRecordDropDownField("lu_languages", "name", "CODE3", "", "", 60, false);
        dr_language.SetFieldType("string");
        dr_language.SetConnectionKey("CONNECTION_STRING");
        dr_language.Initialize("Language", "adm_table_header_nbg", "FormInput", "language_code3", false);
        theRecord.AddRecord(dr_language);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_pcid = new DataRecordShortIntField(false, 9, 9);
        dr_pcid.Initialize("Price code id", "adm_table_header_nbg", "FormInput", "subscription_id", false);
        dr_pcid.SetValue(Session["subscription_id"].ToString());
        theRecord.AddRecord(dr_pcid);

        string sTable = theRecord.GetTableHTML("adm_subscription_names_new.aspx?submited=1");

        return sTable;
    }
}
