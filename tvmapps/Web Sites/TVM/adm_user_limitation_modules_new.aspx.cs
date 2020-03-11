using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_user_limitation_modules_new : System.Web.UI.Page
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
            if (Request.QueryString["limit_id"] != null &&
                Request.QueryString["limit_id"].ToString() != "")
            {
                Session["limit_id"] = int.Parse(Request.QueryString["limit_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_limitation_modules", "group_id", int.Parse(Session["limit_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["limit_id"] = 0;

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
        string sRet = PageUtils.GetPreHeader() + ": User Limitation Module";
        if (Session["limit_id"] != null && Session["limit_id"].ToString() != "" && Session["limit_id"].ToString() != "0")
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
                selectQuery += "SELECT MAX_DEVICE_LIMIT FROM GROUPS WITH (NOLOCK) WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        int maxLimit = int.Parse(selectQuery.Table("query").DefaultView[0].Row["MAX_DEVICE_LIMIT"].ToString());
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
        if (Session["limit_id"] != null && Session["limit_id"].ToString() != "" && int.Parse(Session["limit_id"].ToString()) != 0)
            t = Session["limit_id"];
        string sBack = "adm_domain_limitation_modules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_device_limitation_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_limit = new DataRecordShortIntField(true, 9, 9);
        dr_limit.Initialize("Limit", "adm_table_header_nbg", "FormInput", "user_max_limit", false);
        theRecord.AddRecord(dr_limit);

        DataRecordDropDownField dr_frequency = new DataRecordDropDownField("lu_min_periods", "Description", "ID",string.Empty,string.Empty, 60, true);
        dr_frequency.Initialize("Frequency", "adm_table_header_nbg", "FormInput", "user_freq_period_id", false);
        theRecord.AddRecord(dr_frequency);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_user_limitation_modules_new.aspx?submited=1");

        return sTable;
    }
}
