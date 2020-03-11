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

public partial class adm_tvc_meta_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_tvc_meta.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_tvc_meta.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        //else if (PageUtils.IsTvinciUser() == false)
        //LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(15, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
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
                return;
            }

            if (Request.QueryString["tvc_meta_id"] != null &&
                Request.QueryString["tvc_meta_id"].ToString() != "")
            {
                Session["tvc_meta_id"] = int.Parse(Request.QueryString["tvc_meta_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_tags_types", "group_id", int.Parse(Session["tvc_meta_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["tvc_meta_id"] = 0;
        }
    }

    public void GetHeader()
    {
        if (Session["tvc_meta_id"] != null && Session["tvc_meta_id"].ToString() != "" && int.Parse(Session["tvc_meta_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Tag types - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Tag types - New");
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
        if (Session["tvc_meta_id"] != null && Session["tvc_meta_id"].ToString() != "" && int.Parse(Session["tvc_meta_id"].ToString()) != 0)
            t = Session["tvc_meta_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvc_meta", "adm_table_pager", "adm_tvc_meta.aspx", "", "ID", t, "adm_tvc_meta.aspx", "tvc_meta_id");

        DataRecordDropDownField dr_types = new DataRecordDropDownField("media_tags_types", "name", "id", "", null, 60, false);
        dr_types.Initialize("Meta Tag Type", "adm_table_header_nbg", "FormInput", "media_tags_types_id", true);
        dr_types.SetWhereString("status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_types);

        DataRecordShortTextField dr_site_name = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_site_name.Initialize("Site Name", "adm_table_header_nbg", "FormInput", "SITE_NAME", false);
        theRecord.AddRecord(dr_site_name);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", false);
        theRecord.AddRecord(dr_order_num);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
        //}

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
