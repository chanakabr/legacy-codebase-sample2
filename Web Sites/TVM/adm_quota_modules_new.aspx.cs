using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_quota_modules_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }
    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Quota Modules";
        if (Session["quota_id"] != null && Session["quota_id"].ToString().Length > 0 && Session["quota_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!LoginManager.CheckLogin())
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int newQuotaID = DBManipulator.DoTheWork();
                Session["quota_id"] = null;
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["quota_id"] != null &&
                Request.QueryString["quota_id"].ToString().Length > 0)
            {
                Session["quota_id"] = int.Parse(Request.QueryString["quota_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("quota_modules", "group_id", int.Parse(Session["quota_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && !PageUtils.IsTvinciUser())
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                Session["quota_id"] = 0;
            }
        }

    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString().Length > 0)
        {
            Session["error_msg"] = string.Empty;
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["quota_id"] != null && Session["quota_id"].ToString().Length > 0 && int.Parse(Session["quota_id"].ToString()) != 0)
            t = Session["quota_id"];
        string sBack = "adm_quota_modules.aspx?search_save=1";

        int nGroupID = LoginManager.GetLoginGroupID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("quota_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_quota_in_minutes = new DataRecordShortIntField(true, 9, 9);
        dr_quota_in_minutes.Initialize("Quota (Minutes)", "adm_table_header_nbg", "FormInput", "quota_in_minutes", false);
        theRecord.AddRecord(dr_quota_in_minutes);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(nGroupID.ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_quota_modules_new.aspx?submited=1");

        return sTable;
    }
}