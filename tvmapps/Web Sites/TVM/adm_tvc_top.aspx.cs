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
using System.Threading;
using TVinciShared;

public partial class adm_tvc_top : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(15, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 7, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                object oBaseSiteAdd = ODBCWrapper.Utils.GetTableSingleVal("tvc", "SITE_BASE_ADD", "group_id", "=", nGroupID);
                string sBaseSiteAdd = "";
                if (oBaseSiteAdd != DBNull.Value && oBaseSiteAdd != null)
                {
                    sBaseSiteAdd = oBaseSiteAdd.ToString();
                    if (sBaseSiteAdd.EndsWith("/") == false)
                        sBaseSiteAdd += "/";
                }
                Notifier tt = new Notifier(sBaseSiteAdd + "technical.aspx?Action=RefreshConfiguration", "");
                ThreadStart job = new ThreadStart(tt.NotifyGet);
                Thread thread = new Thread(job);
                System.Threading.Thread.Sleep(250);
                thread.Start();
                //tt.NotifyGet();
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Top Header Initialization";
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
        object t = null;
        object tvc_id = ODBCWrapper.Utils.GetTableSingleVal("tvc", "id", "group_id", "=", LoginManager.GetLoginGroupID());
        if (tvc_id != DBNull.Value && tvc_id != null)
            t = tvc_id;
        //object t = LoginManager.GetLoginGroupID();
        string sBack = "adm_tvc_top.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvc", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_iframe_url = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_iframe_url.Initialize("Header Iframe URL", "adm_table_header_nbg", "FormInput", "IFRAME_URL", false);
        theRecord.AddRecord(dr_iframe_url);

        DataRecordShortIntField dr_iframe_width = new DataRecordShortIntField(true, 3, 3);
        dr_iframe_width.Initialize("Header Iframe width", "adm_table_header_nbg", "FormInput", "IFRAME_WIDTH", false);
        theRecord.AddRecord(dr_iframe_width);

        DataRecordShortIntField dr_iframe_height = new DataRecordShortIntField(true, 3, 3);
        dr_iframe_height.Initialize("Header Iframe height", "adm_table_header_nbg", "FormInput", "IFRAME_HEIGHT", false);
        theRecord.AddRecord(dr_iframe_height);

        DataRecordOnePicBrowserField dr_company_logo = new DataRecordOnePicBrowserField();
        dr_company_logo.Initialize("Company Logo(Left)", "adm_table_header_nbg", "FormInput", "COMPANY_LOGO_ID", false);
        theRecord.AddRecord(dr_company_logo);

        DataRecordShortTextField dr_comp_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_comp_link.Initialize("Company Link(Left)", "adm_table_header_nbg", "FormInput", "COMP_LINK", false);
        theRecord.AddRecord(dr_comp_link);

        DataRecordOnePicBrowserField dr_sponser_pic = new DataRecordOnePicBrowserField();
        dr_sponser_pic.Initialize("Sponser Logo(Right)", "adm_table_header_nbg", "FormInput", "SPONSER_LOGO_ID", false);
        theRecord.AddRecord(dr_sponser_pic);

        DataRecordShortTextField dr_sponser_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_sponser_link.Initialize("Sponser Link(Right)", "adm_table_header_nbg", "FormInput", "SPONSER_LINK", false);
        theRecord.AddRecord(dr_sponser_link);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvc_init.aspx?submited=1");

        return sTable;
    }
}
