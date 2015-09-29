using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_oss_adapter_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;    

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_oss_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_oss_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int pgid = 0;
                // Validate uniqe external id
                if (Session["oss_adapter_id"] != null && Session["oss_adapter_id"].ToString() != "" && int.Parse(Session["oss_adapter_id"].ToString()) != 0)
                {
                    int.TryParse(Session["oss_adapter_id"].ToString(), out pgid);
                }

                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"])) 
                {
                    if (IsExternalIDExists(coll["1_val"], pgid))
                    {
                        Session["error_msg"] = "External Id must be unique";
                        flag = true;
                    }
                    else
                    {
                        Int32 nID = DBManipulator.DoTheWork();

                        // set adapter configuration
                        apiWS.API api = new apiWS.API();

                        string sIP = "1.1.1.1";
                        string sWSUserName = "";
                        string sWSPass = "";
                        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "SetOSSAdapterConfiguration", "api", sIP, ref sWSUserName, ref sWSPass);
                        string sWSURL = GetWSURL("api_ws");
                        if (sWSURL != "")
                            api.Url = sWSURL;
                        try
                        {
                            apiWS.Status status = api.SetOSSAdapterConfiguration(sWSUserName, sWSPass, nID);
                            Logger.Logger.Log("SetOSSAdapterConfiguration", string.Format("oss adapter id:{0}, status:{1}", nID, status.Code), "SetOSSAdapterConfiguration");
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("Exception", string.Format("oss adapter id :{0}, ex msg:{1}, ex st: {2} ", nID, ex.Message, ex.StackTrace), "SetOSSAdapterConfiguration");
                        }

                        return;
                    }
                }
    
           
            }
            
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["oss_adapter_id"] != null && Request.QueryString["oss_adapter_id"].ToString() != "")
            {
                Session["oss_adapter_id"] = int.Parse(Request.QueryString["oss_adapter_id"].ToString());
            }
            else if (!flag)
                Session["oss_adapter_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": OSS Adapter");
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
        if (Session["oss_adapter_id"] != null && Session["oss_adapter_id"].ToString() != "" && int.Parse(Session["oss_adapter_id"].ToString()) != 0)
            t = Session["oss_adapter_id"];
        string sBack = "adm_oss_adapter.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("oss_adapter", "adm_table_pager", sBack, "", "ID", t, sBack, "");        
        
        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        theRecord.AddRecord(dr_external_identifier);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "adapter_url", false);
        theRecord.AddRecord(dr_adapter_url);
        
        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", false, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(dr_shared_secret);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);
        
        string sTable = theRecord.GetTableHTML("adm_oss_adapter_new.aspx?submited=1");

        return sTable;
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    static private bool IsExternalIDExists(string extId, int pgid)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool res = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();        
        selectQuery += "select ID from oss_adapter where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_identifier", "=", extId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", pgid);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                res = true;
                int pgeid = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                string pgname = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", 0);
                Logger.Logger.Log("IsExternalIDExists", string.Format("id:{0}, name:{1}", pgeid, pgname), "oss_adapter");
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return res;
    }
}