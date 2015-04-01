using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_generic_lookup_table_new : System.Web.UI.Page
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
                DBManipulator.DoTheWork();
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["lookup_id"] != null && Request.QueryString["lookup_id"].ToString() != "")
            {
                Session["lookup_id"] = int.Parse(Request.QueryString["lookup_id"].ToString());
               
            }
            else
                Session["lookup_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Lookup Type";
        if (Session["lookup_id"] != null && Session["lookup_id"].ToString() != "" && Session["lookup_id"].ToString() != "0")
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
        if (Session["lookup_id"] != null && Session["lookup_id"].ToString() != "" && int.Parse(Session["lookup_id"].ToString()) != 0)
            t = Session["lookup_id"];
        string sBack = "adm_generic_lookup_table.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("lu_lookup", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Lookup name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        string sTable = theRecord.GetTableHTML("adm_generic_lookup_table_new.aspx?submited=1");

        return sTable;
    }
}