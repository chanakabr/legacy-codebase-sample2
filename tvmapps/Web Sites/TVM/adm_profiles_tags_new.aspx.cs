using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_profiles_tags_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_profiles_tags.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_profiles_tags.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        //else if (PageUtils.IsTvinciUser() == false)
        //LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(9, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            if (Request.QueryString["profile_tag_id"] != null &&
                Request.QueryString["profile_tag_id"].ToString() != "")
            {
                Session["profile_tag_id"] = int.Parse(Request.QueryString["profile_tag_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("profile_tags", "group_id", int.Parse(Session["profile_tag_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        string sProfileTag = ODBCWrapper.Utils.GetTableSingleVal("profile_tags" , "description" , int.Parse(Session["profile_tag_id"].ToString())).ToString();
        string sProfileType = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select ptt.description from profile_tags_types ptt,profile_tags pt where pt.PROFILE_TAG_TYPE_ID = ptt.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pt.id", "=", int.Parse(Session["profile_tag_id"].ToString()));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sProfileType = selectQuery.Table("query").DefaultView[0].Row["description"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (Session["profile_tag_id"] != null && Session["profile_tag_id"].ToString() != "" && int.Parse(Session["profile_tag_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Profile Tags - " + sProfileTag + " (type=" + sProfileType + ")");
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
        if (Session["profile_tag_id"] != null && Session["profile_tag_id"].ToString() != "" && int.Parse(Session["profile_tag_id"].ToString()) != 0)
            t = Session["profile_tag_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("profile_tags", "adm_table_pager", "adm_profiles_tags.aspx?search_save=1", "", "ID", t, "adm_profiles_tags.aspx?search_save=1", "profile_tag_id");

        DataRecordShortTextField dr_tag = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_tag.Initialize("Profile Tag Code", "adm_table_header_nbg", "FormInput", "Value", true);
        theRecord.AddRecord(dr_tag);

        DataRecordShortTextField dr_tag_desc = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_tag_desc.Initialize("Profile Tag Description", "adm_table_header_nbg", "FormInput", "Description", true);
        theRecord.AddRecord(dr_tag_desc);
        /*
        string sGroups = " in ( " + LoginManager.GetLoginGroupID().ToString() + ")";
        DataRecordDropDownField dr_tag_type = new DataRecordDropDownField("profile_tags_types", "description", "id", "", null , 60 , false);
        dr_tag_type.Initialize("Profile Tag type", "adm_table_header_nbg", "FormInput", "PROFILE_TAG_TYPE_ID", true);
        string sQuery = "select description as txt,id as id from profile_tags_types where status=1 and (group_id=0 or group_id " + sGroups + ")";
        dr_tag_type.SetSelectsQuery(sQuery);
        theRecord.AddRecord(dr_tag_type);
        */
        DataRecordShortIntField dr_tag_type = new DataRecordShortIntField(false, 9, 9);
        dr_tag_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "PROFILE_TAG_TYPE_ID", false);
        //dr_tag_type.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_tag_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
