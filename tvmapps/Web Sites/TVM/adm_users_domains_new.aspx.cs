using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_users_domains_new : System.Web.UI.Page
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
            if (Request.QueryString["domain_id"] != null &&
                Request.QueryString["domain_id"].ToString() != "")
            {
                Session["domain_id"] = int.Parse(Request.QueryString["domain_id"].ToString());
                /*
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_limitation_modules", "group_id", int.Parse(Session["limit_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                */
            }
            else
                Session["domain_id"] = null;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
                DBManipulator.DoTheWork("users_connection");

        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Domain ";
        if (Session["domain_id"] != null && Session["domain_id"].ToString() != "" && Session["domain_id"].ToString() != "0")
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
                selectQuery += "select max_device_limit from groups where ";
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
        if (Session["domain_id"] != null && Session["domain_id"].ToString() != "" && int.Parse(Session["domain_id"].ToString()) != 0)
            t = Session["domain_id"];
        
        string sBack = "adm_users_domains.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("domains", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("users_connection");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortTextField dr_Desc = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Desc.Initialize("Description", "adm_table_header_nbg", "FormInput", "Description", true);
        theRecord.AddRecord(dr_Desc);

        /*DataRecordShortIntField dr_limit = new DataRecordShortIntField(true, 9, 9);
        dr_limit.Initialize("Limit", "adm_table_header_nbg", "FormInput", "max_limit", false);
        theRecord.AddRecord(dr_limit);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
        */
        string sTable = theRecord.GetTableHTML("adm_users_domains_new.aspx?submited=1");

        return sTable;
    }
}
