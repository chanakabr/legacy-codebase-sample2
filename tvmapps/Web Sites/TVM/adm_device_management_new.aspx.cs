using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_device_management_new : System.Web.UI.Page
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
            if (Request.QueryString["group_device_family_id"] != null &&
                Request.QueryString["group_device_family_id"].ToString() != "")
            {
                Session["group_device_family_id"] = int.Parse(Request.QueryString["group_device_family_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_families", "group_id", int.Parse(Session["group_device_family_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["group_device_family_id"] = 0;

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
        string sRet = PageUtils.GetPreHeader() + ": Device Family";
        if (Session["group_device_family_id"] != null && Session["group_device_family_id"].ToString() != "" && Session["group_device_family_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected bool ValidateLimit()
    {
        bool retVal = true;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["1_field"] != null && coll["1_val"] != null)
        {
            string limitStr = coll["1_val"].ToString();
            if (!string.IsNullOrEmpty(limitStr))
            {
                int limitInt = int.Parse(limitStr.Trim());
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select max_device_limit from groups with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        int maxLimit = int.Parse(selectQuery.Table("query").DefaultView[0].Row["max_device_limit"].ToString());
                        if (maxLimit < limitInt)
                        {
                            Session["error_msg"] = "Limit exceeds Account Max Device Limit";
                            retVal = false;
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }

        }
        return retVal;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["group_device_family_id"] != null && Session["group_device_family_id"].ToString() != "" && int.Parse(Session["group_device_family_id"].ToString()) != 0)
            t = Session["group_device_family_id"];
        string sBack = "adm_device_management.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_device_families", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordDropDownField dr_device_family = new DataRecordDropDownField("lu_DeviceFamily", "Name", "id", "", "", 60, false);
        dr_device_family.Initialize("Device Family", "adm_table_header_nbg", "FormInput", "device_family_id", false);
        theRecord.AddRecord(dr_device_family);

        DataRecordShortIntField dr_limit = new DataRecordShortIntField(true, 9, 9);
        dr_limit.Initialize("Limit", "adm_table_header_nbg", "FormInput", "max_limit", false);
        theRecord.AddRecord(dr_limit);

        DataRecordShortIntField dr_concurrent_limit = new DataRecordShortIntField(true, 9, 9);
        dr_concurrent_limit.Initialize("Concurrent Limit", "adm_table_header_nbg", "FormInput", "max_concurrent_limit", false);
        theRecord.AddRecord(dr_concurrent_limit);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_limit_module_id = new DataRecordShortIntField(false, 9, 9);
        dr_limit_module_id.Initialize("Limit ID", "adm_table_header_nbg", "FormInput", "limit_module_id", false);
        dr_limit_module_id.SetValue(Session["limit_module_id"].ToString());
        theRecord.AddRecord(dr_limit_module_id);

        string sTable = theRecord.GetTableHTML("adm_device_management_new.aspx?submited=1");

        return sTable;
    }
}
