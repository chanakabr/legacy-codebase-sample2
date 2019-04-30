using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_tvm_notifications_settings_new : System.Web.UI.Page
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
        m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

        if (!IsPostBack)
        {
            if (Request.QueryString["notification_setting_id"] != null &&
                  Request.QueryString["notification_setting_id"].ToString() != "")
            {   
                Session["notification_setting_id"] = int.Parse(Request.QueryString["notification_setting_id"].ToString());
            }
            else
                Session["notification_setting_id"] = 0;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("notifications_connection");
                return;
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        if (Session["notification_setting_id"] == null || Session["notification_setting_id"].ToString() == "" || Session["notification_setting_id"].ToString() == "0")
            return;

        string sRet = PageUtils.GetPreHeader() + ":New Media Notifications Settings names ";
        if (Session["notification_setting_id"] != null && Session["notification_setting_id"].ToString() != "" && Session["notification_setting_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);     
    }

    static protected Int32 GetMainLang(ref string sMainLang, ref string sCode)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.NAME,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                sCode = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nLangID;
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
        if (Session["notification_setting_id"] != null && Session["notification_setting_id"].ToString() != "" && int.Parse(Session["notification_setting_id"].ToString()) != 0)
            t = Session["notification_setting_id"];
        string sBack = "adm_tvm_notifications_settings.aspx";
        
        DBRecordWebEditor theRecord = new DBRecordWebEditor("notifications_settings", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");

        DataRecordLongTextField dr_text = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_text.Initialize("Message Text", "adm_table_header_nbg", "FormInput", "message_text", true);
        theRecord.AddRecord(dr_text);
      
        DataRecordShortIntField dr_notification_trigger_type = new DataRecordShortIntField(false, 9, 9);
        dr_notification_trigger_type.Initialize("trigger type", "adm_table_header_nbg", "FormInput", "trigger_type", false);
        dr_notification_trigger_type.SetValue("3"); // follow by tag
        theRecord.AddRecord(dr_notification_trigger_type);

        DataRecordShortTextField dr_dateDefaultFormat = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_dateDefaultFormat.Initialize("Date Default Format", "adm_table_header_nbg", "FormInput", "notification_date_format", false);
        theRecord.AddRecord(dr_dateDefaultFormat);

        DataRecordShortTextField dr_emailTemplate = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_emailTemplate.Initialize("Email Template", "adm_table_header_nbg", "FormInput", "notification_email_template", false);
        theRecord.AddRecord(dr_emailTemplate);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvm_notifications_settings_new.aspx?submited=1");

        return sTable;
    }
}