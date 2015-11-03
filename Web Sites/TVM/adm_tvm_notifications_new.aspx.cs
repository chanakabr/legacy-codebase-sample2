using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using System.Data;
using KLogMonitor;
using System.Reflection;

public partial class adm_tvm_notifications_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    static public bool IsTvinciImpl()
    {

        return true;

    }

    protected void Page_Load(object sender, EventArgs e)
    {
       
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
       
        Int32 nMenuID = 0;
        string sMainCode3 = "";
        if (!IsPostBack)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();

            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("notifications_connection");
                return;
            }
           
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            string sMainLang = "";
            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
            {
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
                Session["lang_code"] = ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "code3", int.Parse(Session["lang_id"].ToString()));
            }
            else
            {
                Session["lang_id"] = GetMainLang(ref sMainLang, ref sMainCode3);
                Session["lang_code"] = sMainCode3;
            }
           
            if (Request.QueryString["notification_id"] != null &&
                Request.QueryString["notification_id"].ToString() != "")
            {
                log.Debug("Notification - Started 5");
                Session["notification_id"] = int.Parse(Request.QueryString["notification_id"].ToString());
                
            }
            else
                Session["notification_id"] = 0;
        }
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            string sCode3 = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang, ref sCode3);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_tvm_notifications_new.aspx?notification_id=" + Session["notification_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
            else
                sTemp += "javascript:void(0);";
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sOnOff = "on";
                    if (nLangID != int.Parse(Session["lang_id"].ToString()))
                        sOnOff = "off";
                    sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
                    if (nLangID != int.Parse(Session["lang_id"].ToString()))
                    {
                        sTemp += "adm_tvm_notifications_new.aspx?ppv_module_id=" + Session["ppv_module_id"].ToString() + "&lang_id=" + nLangID.ToString();
                    }
                    else
                        sTemp += "javascript:void(0);";
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                if (nCount1 == 0)
                    sTemp = "";
            }
            selectQuery1.Finish();
            selectQuery1 = null;
           
            return sTemp;
        }
        catch
        {
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    protected void GetLangMenu()
    {
        
        Response.Write(m_sLangMenu);
    }

    protected void GetMainMenu()
    {
       
        Response.Write(m_sMenu);
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

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Notification ";
        if (Session["notification_id"] != null && Session["notification_id"].ToString() != "" && Session["notification_id"].ToString() != "0")
        {
            int notificationID = int.Parse(Session["notification_id"].ToString());
            string sMainLang = "";
            string sCode3 = "";
            Int32 nLangID = GetMainLang(ref sMainLang, ref sCode3);
            object sNotificationName = ODBCWrapper.Utils.GetTableSingleVal("notifications", "title", "id", "=", notificationID, "notifications_connection");
            if (sNotificationName != null && sNotificationName != DBNull.Value)
                sRet += "(" + sNotificationName.ToString() + ")";
            sRet += " - Edit";
        }
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
        if (Session["notification_id"] != null && Session["notification_id"].ToString() != "" && int.Parse(Session["notification_id"].ToString()) != 0)
            t = Session["notification_id"];
        string sBack = "adm_tvm_notifications.aspx";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("notifications", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");
        string sMainLang = "";
        string sMainCode = "";        

        DataRecordShortTextField dr_title = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_title.Initialize("Title", "adm_table_header_nbg", "FormInput", "title", true);
        theRecord.AddRecord(dr_title);

        DataRecordDropDownField dr_notification_type = new DataRecordDropDownField("notification_request_types", "decription", "id", "", null, 60, false);
        dr_notification_type.SetFieldType("string");
        dr_notification_type.Initialize("Notification Type", "adm_table_header_nbg", "FormInput", "notification_type", true);
        dr_notification_type.SetDefault(0);
        theRecord.AddRecord(dr_notification_type);

        DataRecordDropDownField dr_notification_trigger_type = new DataRecordDropDownField("notification_triggers_types", "decription", "id", "", null, 60, false);
        dr_notification_trigger_type.Initialize("Trigger Type", "adm_table_header_nbg", "FormInput", "trigger_type", true);
        dr_notification_trigger_type.SetFieldType("string");
        dr_notification_trigger_type.SetDefault(0);
       
        theRecord.AddRecord(dr_notification_trigger_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
        
        string sDefTag = string.Empty;
        if (t != null)
        {
            object oDefTag = ODBCWrapper.Utils.GetTableSingleVal("notifications", "sKey", int.Parse(t.ToString()), 0, "notifications_connection");
            if (oDefTag != DBNull.Value && oDefTag != null)
                sDefTag = oDefTag.ToString();
        }
      
        DataRecordDropDownField dr_tags = new DataRecordDropDownField("media_tags_types", "NAME", "id", "",null, 60, false);
        dr_tags.SetFieldType("string");        
        string sQuery = "select  name as txt,id as id from media_tags_types where status=1 and group_id in (" + GetRegularChildGroupsStr(LoginManager.GetLoginGroupID(), "MAIN_CONNECTION_STRING") + ")";
        sQuery += " order by txt"; 
        dr_tags.SetNoSelectStr("---");
        
        dr_tags.SetSelectsQuery(sQuery);       
        dr_tags.Initialize("Tag Type", "adm_table_header_nbg", "FormInput", "sKey", false);
        dr_tags.SetConnectionKey("MAIN_CONNECTION_STRING");       

        if (!string.IsNullOrEmpty(sDefTag))
            dr_tags.SetDefaultVal(sDefTag);
        theRecord.AddRecord(dr_tags);

        //SMS / PUSH / EMAIL 
        DataRecordCheckBoxField sms_checkBox = new DataRecordCheckBoxField(true);
        sms_checkBox.Initialize("SMS Notification", "adm_table_header_nbg", "FormInput", "is_sms", false);
        sms_checkBox.SetDefault(1);
        theRecord.AddRecord(sms_checkBox);

        DataRecordCheckBoxField email_checkBox = new DataRecordCheckBoxField(true);
        email_checkBox.Initialize("Email Notification", "adm_table_header_nbg", "FormInput", "is_email", false);
        email_checkBox.SetDefault(1);
        theRecord.AddRecord(email_checkBox);

        DataRecordCheckBoxField device_checkBox = new DataRecordCheckBoxField(true);
        device_checkBox.Initialize("Device Notification", "adm_table_header_nbg", "FormInput", "is_device", false);
        device_checkBox.SetDefault(1);
        theRecord.AddRecord(device_checkBox);

        string sTable = theRecord.GetTableHTML("adm_tvm_notifications_new.aspx?submited=1", false);
        return sTable;
    }

    private string GetRegularChildGroupsStr(int nGroupID, string sConnKey)
    {
        string groups = string.Empty;
        List<string> lGroups = new List<string>();
        DataTable dt = DAL.NotificationDal.GetRegularChildGroupsStr(nGroupID);
        if (dt != null && dt.DefaultView.Count > 0)
        {
            foreach (DataRow dr in dt.Rows)
            {
                lGroups.Add(ODBCWrapper.Utils.GetSafeStr(dr["group_id"]));
            }
            groups = string.Join(",", lGroups.ToArray());
        }
        return groups;
    }  
}