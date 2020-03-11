using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_group_fictivic_metas_new : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["group_fictivic_meta_id"] != null &&
                Request.QueryString["group_fictivic_meta_id"].ToString() != "")
            {
                Session["group_fictivic_meta_id"] = int.Parse(Request.QueryString["group_fictivic_meta_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_fictivic_metas", "group_id", int.Parse(Session["group_fictivic_meta_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["group_fictivic_meta_id"] = 0;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
                DBManipulator.DoTheWork();
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Group Fictivic Metas";
        if (Session["group_fictivic_meta_id"] != null && Session["group_fictivic_meta_id"].ToString() != "" && Session["group_fictivic_meta_id"].ToString() != "0")
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
        if (Session["group_fictivic_meta_id"] != null && Session["group_fictivic_meta_id"].ToString() != "" && int.Parse(Session["group_fictivic_meta_id"].ToString()) != 0)
            t = Session["group_fictivic_meta_id"];
        string sBack = "adm_group_fictivic_metas.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_fictivic_metas", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("Original Meta Name", "adm_table_header_nbg", "FormInput", "ORIGIN_META_NAME", true);
        theRecord.AddRecord(dr_d);

        DataRecordShortTextField dr_RELATED_TYPE = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_RELATED_TYPE.Initialize("Related Type(Name)", "adm_table_header_nbg", "FormInput", "RELATED_TYPE", true);
        theRecord.AddRecord(dr_RELATED_TYPE);
        
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_group_fictivic_metas_new.aspx?submited=1");

        return sTable;
    }
}
