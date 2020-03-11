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

public partial class adm_tvc_ads : System.Web.UI.Page
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
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 6, true);
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
        string sRet = PageUtils.GetPreHeader() + ": Ads Initialization";
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
        string sBack = "adm_tvc_ads.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvc", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordLongTextField dr_top_ad_script = new DataRecordLongTextField("ltr", true, 90, 8);
        dr_top_ad_script.Initialize("Top Ad Script", "adm_table_header_nbg", "FormInput", "TOP_AD_SCRIPT", false);
        theRecord.AddRecord(dr_top_ad_script);

        DataRecordLongTextField dr_bottom_ad_script = new DataRecordLongTextField("ltr", true, 90, 8);
        dr_bottom_ad_script.Initialize("Bottom Ad Script", "adm_table_header_nbg", "FormInput", "BOTTOM_AD_SCRIPT", false);
        theRecord.AddRecord(dr_bottom_ad_script);

        DataRecordShortTextField dr_adv_header = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_adv_header.Initialize("Right Ad Header", "adm_table_header_nbg", "FormInput", "ADVERTISMENT_HEADER", false);
        theRecord.AddRecord(dr_adv_header);

        DataRecordLongTextField dr_right_ad_script = new DataRecordLongTextField("ltr", true, 90, 8);
        dr_right_ad_script.Initialize("Right Ad Script", "adm_table_header_nbg", "FormInput", "RIGHT_AD_SCRIPT", false);
        theRecord.AddRecord(dr_right_ad_script);

        DataRecordOnePicBrowserField dr_commercial_pic = new DataRecordOnePicBrowserField();
        dr_commercial_pic.Initialize("Right Ad Image", "adm_table_header_nbg", "FormInput", "COMMERCIAL_PIC_ID", false);
        theRecord.AddRecord(dr_commercial_pic);

        DataRecordShortTextField dr_commercial_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_commercial_link.Initialize("Right Ad Link", "adm_table_header_nbg", "FormInput", "COMMERCIAL_LINK", false);
        theRecord.AddRecord(dr_commercial_link);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvc_init.aspx?submited=1");

        return sTable;
    }
}
