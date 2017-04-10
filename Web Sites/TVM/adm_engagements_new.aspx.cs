using KLogMonitor;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using TvinciImporter;

public partial class adm_engagements_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
           
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_engagements.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_engagements.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {

                ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                int id = 0;
                if (Session["message_announcement_id"] != null && Session["message_announcement_id"].ToString() != "")
                {
                    id = int.Parse(Session["message_announcement_id"].ToString());
                }
                int groupId = LoginManager.GetLoginGroupID();
                string name = string.Empty;
                string message = string.Empty;
                string timezone = string.Empty;
                int recipients = 0;
                DateTime date = new DateTime();
                bool Enabled = false;

                PageFiled(ref Enabled, ref recipients, ref name, ref message, ref date, ref timezone);

                if (id == 0)
                {
                    result = ImporterImpl.AddMessageAnnouncement(groupId, Enabled, name, message, recipients, date, timezone, ref id);//Notification                           
                    Session["message_announcement_id"] = id;

                }
                else
                {
                    result = ImporterImpl.UpdateMessageAnnouncement(groupId, id, Enabled, name, message, recipients, date, timezone);
                }
                if (result == null)
                {
                    Session["error_msg_s"] = "Error";
                    Session["error_msg"] = "Error";
                }
                else if (result.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    Session["error_msg"] = result.Message;
                    Session["error_msg_s"] = result.Message;
                }

                EndOfAction();

                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["message_announcement_id"] != null && Request.QueryString["message_announcement_id"].ToString() != "")
            {
                Session["message_announcement_id"] = int.Parse(Request.QueryString["message_announcement_id"].ToString());               
            }
            else if (Session["message_announcement_id"] == null || Session["message_announcement_id"].ToString() == "")
            {
                Session["message_announcement_id"] = 0;
            }

            if (Session["error_msg_s"] != null && Session["error_msg_s"].ToString() != "")
            {
                lblError.Visible = true;
                lblError.Text = Session["error_msg_s"].ToString();
                Session["error_msg_s"] = null;                
            }
            else
            {
                lblError.Visible = false;
                lblError.Text = "";
            }

           
        }
    }

    private void EndOfAction()
    {

        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
        {
            if (coll["failure_back_page"] != null)
            {                
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["failure_back_page"].ToString() + "';</script>");
            }
            else
            {
                HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
        }
        else
        {
            if (HttpContext.Current.Request.QueryString["back_n_next"] != null)
            {
                HttpContext.Current.Session["last_page_html"] = null;
                string s = HttpContext.Current.Session["back_n_next"].ToString();
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + s.ToString() + "';</script>");
                HttpContext.Current.Session["back_n_next"] = null;
            }
            else
            {
                if (coll["success_back_page"] != null)
                    HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
                else
                    HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
            CachingManager.CachingManager.RemoveFromCache("SetValue_" + coll["table_name"].ToString() + "_");
        }        
    }
    

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            Int32 nMainLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select l.name,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_media_new.aspx?media_id=" + Session["media_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nOwnerGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sTemp += "<li><a href=\"";
                    sTemp += "adm_media_translate.aspx?media_id=" + Session["media_id"].ToString() + "&lang_id=" + nLangID.ToString();
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

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": System Announcements");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
       
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object announcementId = null;
       
        if (Session["message_announcement_id"] != null && Session["message_announcement_id"].ToString() != "" && int.Parse(Session["message_announcement_id"].ToString()) != 0)
            announcementId = Session["message_announcement_id"];      
            
        string sBack = "adm_engagements.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("message_announcements", "adm_table_pager", sBack, "", "ID", announcementId, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");      

        DataRecordShortIntField dr_enabled = new DataRecordShortIntField(false, 9, 9);
        dr_enabled.Initialize("Enabled", "adm_table_header_nbg", "FormInput", "is_active", false);
        dr_enabled.setFiledName("Enabled");
        dr_enabled.SetDefault(1);
        theRecord.AddRecord(dr_enabled);

        DataRecordDropDownField dr_message_recipient = new DataRecordDropDownField("", "name", "id", "", null, 60,false);
        dr_message_recipient.setFiledName("recipients");
        dr_message_recipient.SetSelectsDT(GetReceipentType());
        dr_message_recipient.Initialize("Recipients", "adm_table_header_nbg", "FormInput", "recipients", true);
        theRecord.AddRecord(dr_message_recipient);

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 256);
        dr_name.setFiledName("Name");
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "name", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_Message = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Message.setFiledName("Message");
        dr_Message.Initialize("Message", "adm_table_header_nbg", "FormInput", "Message", true);
        theRecord.AddRecord(dr_Message);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.setFiledName("StartDateTime");
        dr_start_date.Initialize("Begin send date & time", "adm_table_header_nbg", "FormInput", "start_time", true);
        dr_start_date.SetDefault(DateTime.Now);
        // get timezone by id 
        string tempTimeZone = "UTC";
        if (announcementId != null)
        {
            tempTimeZone = GetTimeZone(announcementId);
        }
        dr_start_date.setTimeZone(tempTimeZone);
        
        theRecord.AddRecord(dr_start_date);
       
        System.Data.DataTable tz = GetTimeZone();
        DataRecordDropDownField dr_time_zone = new DataRecordDropDownField("", "NAME", "id", "", null, 60, true);
        dr_time_zone.setFiledName("TimeZone");
        dr_time_zone.SetFieldType("string");
        dr_time_zone.Initialize("Time Zone", "adm_table_header_nbg", "FormInput", "timezone", true);
        dr_time_zone.SetSelectsDT(tz);
        dr_time_zone.SetDefaultVal("UTC");
        theRecord.AddRecord(dr_time_zone);

        string sTable = theRecord.GetTableHTML("adm_engagements_new.aspx?submited=1");

        return sTable;
    }

    private System.Data.DataTable GetReceipentType()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("txt", typeof(string));
        int i = 0;
        foreach (ApiObjects.eAnnouncementRecipientsType r in Enum.GetValues(typeof(ApiObjects.eAnnouncementRecipientsType)))
        {
            if ((int)r != (int)ApiObjects.eAnnouncementRecipientsType.Other)
            {
                dt.Rows.Add((int)r, r);
            }
        }
        return dt;
    }

    private System.Data.DataTable GetTimeZone()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(string));
        dt.Columns.Add("txt", typeof(string));
        int i = 0;
        foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
        {
            dt.Rows.Add(z.Id, z.Id);
        }
        return dt;
    }

    private string GetTimeZone(object id)
    {
        string timeZone = string.Empty;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("notifications_connection");
        int nID = 0;
        int.TryParse(id.ToString(), out nID);
        object otimezone = ODBCWrapper.Utils.GetTableSingleVal("message_announcements", "timezone", nID, 0, "notifications_connection");
        if (otimezone != null)
        {
            timeZone = otimezone.ToString();
        }
        return timeZone;
    }

    private static DateTime getDateTime(string sVal, int nCounter, ref System.Collections.Specialized.NameValueCollection coll, ref bool bValid)
    {
        if (sVal != "")
        {
            string sValMin = coll[nCounter.ToString() + "_valMin"].ToString();
            string sValHour = coll[nCounter.ToString() + "_valHour"].ToString();
            bValid = validateParam("int", sValHour, 0, 23);
            if (bValid == true)
                bValid = validateParam("int", sValMin, 0, 59);
            if (bValid == true)
                bValid = validateParam("date", sVal, 0, 59);
            DateTime tTime = DateUtils.GetDateFromStr(sVal);
            if (sValHour == "")
                sValHour = "0";
            if (sValMin == "")
                sValMin = "0";
            tTime = tTime.AddHours(int.Parse(sValHour.ToString()));
            tTime = tTime.AddMinutes(int.Parse(sValMin.ToString()));
            bValid = true;
            return tTime;
        }
        else
        {
            bValid = false;
            return DateTime.MinValue;
        }
    }
    static protected bool CheckForbiddenChars(string sStr)
    {
        return true;
    }
    static protected bool validateParam(string sType, string sVal, double nMin, double nMax)
    {
        try
        {
            bool bOK = true;
            if (sType == "string")
                return CheckForbiddenChars(sVal);
            if (sType == "int" && sVal != "")
            {
                Int32 nVal = int.Parse(sVal);
                if (nVal < nMin && nMin != -1)
                    bOK = false;
                if (nVal > nMax && nMax != -1)
                    bOK = false;
            }
            if (sType == "double" && sVal != "")
            {
                double nVal = double.Parse(sVal);
                if (nVal < nMin && nMin != -1)
                    bOK = false;
                if (nVal > nMax && nMax != -1)
                    bOK = false;
            }
            if (sType == "date")
            {
                DateTime tTime = DateUtils.GetDateFromStr(sVal);
            }
            return bOK;
        }
        catch
        {
            return false;
        }
    }
    private void PageFiled(ref bool Enabled, ref int recipients, ref string name, ref string message, ref DateTime date, ref string timezone)
    {
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["table_name"] == null)
        {
            HttpContext.Current.Session["error_msg"] = "missing table name - cannot update";
        }
        else
        {
            int nCount = coll.Count;
            int nCounter = 0;
            bool bValid = true;

            try
            {
                while (nCounter < nCount)
                {
                    try
                    {
                        if (coll[nCounter.ToString() + "_fieldName"] != null)
                        {
                            string sFieldName = coll[nCounter.ToString() + "_fieldName"].ToString();
                            string sVal = "";
                            if (coll[nCounter.ToString() + "_val"] != null)
                            {
                                sVal = coll[nCounter.ToString() + "_val"].ToString();
                            }
                            #region case
                            switch (sFieldName)
                            {
                                case "Enabled":
                                    if (sVal == "1")
                                        Enabled = true;
                                    else
                                        Enabled = false;
                                    break;
                                case "recipients":
                                    if (!string.IsNullOrEmpty(sVal))
                                    {
                                        recipients = int.Parse(sVal);
                                    }
                                    break;
                                case "Name":
                                    name = sVal.Replace("\r\n", "<br\\>");
                                    break;
                                case "Message":
                                    message = sVal.Replace("\r\n", "&lt;br\\&gt;");
                                    break;
                                case "StartDateTime":
                                    string sValMin = coll[nCounter.ToString() + "_valMin"].ToString();
                                    string sValHour = coll[nCounter.ToString() + "_valHour"].ToString();
                                    bValid = validateParam("int", sValHour, 0, 23);
                                    if (bValid == true)
                                        bValid = validateParam("int", sValMin, 0, 59);
                                    if (bValid == true)
                                        bValid = validateParam("date", sVal, 0, 59);
                                    DateTime tTime = DateUtils.GetDateFromStr(sVal);
                                    if (sValHour == "")
                                        sValHour = "0";
                                    if (sValMin == "")
                                        sValMin = "0";
                                    tTime = tTime.AddHours(int.Parse(sValHour.ToString()));
                                    tTime = tTime.AddMinutes(int.Parse(sValMin.ToString()));
                                    date = tTime;
                                    //getDateTime(sVal, nCounter, ref coll, ref bValid);
                                    break;
                                case "TimeZone":
                                    try
                                    {
                                        timezone = sVal;
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                default:
                                    break;

                            }
                            #endregion
                        }

                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                    nCounter++;
                }
                //convert datetime to UTC
                ////date = ODBCWrapper.Utils.ConvertToUtc(date, timezone);
                
            }
            catch (Exception)
            {
            }
        }
    }

}