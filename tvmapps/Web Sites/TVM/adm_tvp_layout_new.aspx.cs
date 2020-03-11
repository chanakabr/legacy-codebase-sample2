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

public partial class adm_tvp_layout_new : System.Web.UI.Page
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
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["tvp_layout_id"] != null &&
               Request.QueryString["tvp_layout_id"].ToString() != "")
            {
                Session["tvp_layout_id"] = int.Parse(Request.QueryString["tvp_layout_id"].ToString());
            }
            else
                Session["tvp_layout_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Layout configuration ";
        if (Session["tvp_layout_id"] != null && Session["tvp_layout_id"].ToString() != "" && Session["tvp_layout_id"].ToString() != "0")
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["tvp_layout_id"] != null && Session["tvp_layout_id"].ToString() != "" && int.Parse(Session["tvp_layout_id"].ToString()) != 0)
            t = Session["tvp_layout_id"];

        string sBack = "adm_tvp_layout.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_layout", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortIntField dr_num_of_main = new DataRecordShortIntField(true, 9, 9);
        dr_num_of_main.Initialize("Number of mains", "adm_table_header_nbg", "FormInput", "NUM_OF_MAIN", true);
        theRecord.AddRecord(dr_num_of_main);

        DataRecordShortIntField dr_num_of_sides = new DataRecordShortIntField(true, 9, 9);
        dr_num_of_sides.Initialize("Number of sides", "adm_table_header_nbg", "FormInput", "NUM_OF_SIDES", true);
        theRecord.AddRecord(dr_num_of_sides);


        DataRecordBoolField dr_has_top = new DataRecordBoolField(true);
        dr_has_top.Initialize("Has Top", "adm_table_header_nbg", "FormInput", "HAS_TOP", false);
        theRecord.AddRecord(dr_has_top);

        DataRecordBoolField dr_has_bottom = new DataRecordBoolField(true);
        dr_has_bottom.Initialize("Has Bottom", "adm_table_header_nbg", "FormInput", "HAS_BOTTOM", false);
        theRecord.AddRecord(dr_has_bottom);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvp_layout_new.aspx?submited=1");

        return sTable;
    }
}
