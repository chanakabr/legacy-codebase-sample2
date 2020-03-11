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

public partial class adm_actions_types_new : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
            if (Request.QueryString["action_type_id"] != null &&
                Request.QueryString["action_type_id"].ToString() != "")
            {
                Session["action_type_id"] = int.Parse(Request.QueryString["action_type_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_actions", "group_id", int.Parse(Session["action_type_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["action_type_id"] = 0;

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
        string sRet = PageUtils.GetPreHeader() + ": Group Documented Actions";
        if (Session["action_type_id"] != null && Session["action_type_id"].ToString() != "" && Session["action_type_id"].ToString() != "0")
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
        if (Session["action_type_id"] != null && Session["action_type_id"].ToString() != "" && int.Parse(Session["action_type_id"].ToString()) != 0)
            t = Session["action_type_id"];
        string sBack = "adm_actions_types.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_actions", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("Action Name", "adm_table_header_nbg", "FormInput", "ACTION_NAME", true);
        theRecord.AddRecord(dr_d);

        DataRecordShortTextField dr_dn = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_dn.Initialize("Action Description", "adm_table_header_nbg", "FormInput", "ACTION_DESCRIPTION", false);
        theRecord.AddRecord(dr_dn);

        DataRecordLongTextField dr_edit_data = new DataRecordLongTextField("rtl", true, 60, 10);
        dr_edit_data.Initialize("Editor remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_edit_data);

        bool bVisible = PageUtils.IsTvinciUser();
        if (bVisible == true)
        {
            DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
            dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
            theRecord.AddRecord(dr_groups);
        }
        else
        {
            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);
        }

        string sTable = theRecord.GetTableHTML("adm_actions_types_new.aspx?submited=1");

        return sTable;
    }
}
