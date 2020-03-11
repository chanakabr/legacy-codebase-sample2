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

public partial class adm_3choise_meta_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_3choise_meta.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_3choise_meta.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        //else if (PageUtils.IsTvinciUser() == false)
            //LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(12, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            if (Request.QueryString["3choise_meta_id"] != null &&
                Request.QueryString["3choise_meta_id"].ToString() != "")
            {
                Session["3choise_meta_id"] = int.Parse(Request.QueryString["3choise_meta_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_3choise_types", "group_id", int.Parse(Session["3choise_meta_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["3choise_meta_id"] = 0;
        }
    }

    public void GetHeader()
    {
        if (Session["3choise_meta_id"] != null && Session["3choise_meta_id"].ToString() != "" && int.Parse(Session["3choise_meta_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":3 Choise Meta Types - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":3 Choise Meta Types - New");
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
        if (Session["3choise_meta_id"] != null && Session["3choise_meta_id"].ToString() != "" && int.Parse(Session["3choise_meta_id"].ToString()) != 0)
            t = Session["3choise_meta_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_3choise_types", "adm_table_pager", "adm_3choise_meta.aspx", "", "ID", t, "adm_3choise_meta.aspx", "3choise_meta_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Type Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_choise1 = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_choise1.Initialize("Choise1", "adm_table_header_nbg", "FormInput", "CHOISE1", true);
        theRecord.AddRecord(dr_choise1);

        DataRecordShortTextField dr_choise2 = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_choise2.Initialize("Choise2", "adm_table_header_nbg", "FormInput", "CHOISE2", true);
        theRecord.AddRecord(dr_choise2);

        //bool bVisible = PageUtils.IsTvinciUser();
        //if (bVisible == true)
        //{
        //DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
        //dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
        //dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
        //theRecord.AddRecord(dr_groups);
        //}
        //else
        //{
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
        //}

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
