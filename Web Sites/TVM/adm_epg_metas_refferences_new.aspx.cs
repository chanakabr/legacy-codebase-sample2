using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_metas_refferences_new : System.Web.UI.Page
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
            if (Request.QueryString["epg_ref_id"] != null &&
                Request.QueryString["epg_ref_id"].ToString() != "")
            {
                Session["epg_ref_id"] = int.Parse(Request.QueryString["epg_ref_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("device_rules", "group_id", int.Parse(Session["epg_ref_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_ref_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": EPG Metas Refferences";
        if (Session["epg_ref_id"] != null && Session["epg_ref_id"].ToString() != "" && Session["epg_ref_id"].ToString() != "0")
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
        if (Session["epg_ref_id"] != null && Session["epg_ref_id"].ToString() != "" && int.Parse(Session["epg_ref_id"].ToString()) != 0)
            t = Session["epg_ref_id"];
        string sBack = "adm_epg_metas_refferences.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("EPG_fields_mapping", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Refference", "adm_table_header_nbg", "FormInput", "external_ref", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_Order = new DataRecordShortIntField(true, 60, 128);
        dr_Order.Initialize("Order", "adm_table_header_nbg", "FormInput", "order_num", true);
        theRecord.AddRecord(dr_Order);

        string sTable = theRecord.GetTableHTML("adm_metas_refferences_new.aspx?submited=1");

        return sTable;
    }
}