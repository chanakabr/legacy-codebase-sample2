using System;
using System.Web;
using TVinciShared;

public partial class adm_engagements_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
           
    protected void Page_Load(object sender, EventArgs e)
    {
        Int32 nMenuID = 0;

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
                DBManipulator.DoTheWork("notifications_connection");
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 7, true);

            if (Request.QueryString["engagement_id"] != null && Request.QueryString["engagement_id"].ToString() != "")
            {
                Session["engagement_id"] = int.Parse(Request.QueryString["engagement_id"].ToString());
            }
            else
            {
                Session["engagement_id"] = 0;
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
        string sRet = PageUtils.GetPreHeader() + ": Engagement";
        if (Session["engagement_id"] != null && Session["engagement_id"].ToString() != "" && Session["engagement_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
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
        
        object groupId = LoginManager.GetLoginGroupID();        
        object engagementId = null;
       
        if (Session["engagement_id"] != null && Session["engagement_id"].ToString() != "" && int.Parse(Session["engagement_id"].ToString()) != 0)
            engagementId = Session["engagement_id"];      
            
        string sBack = "adm_engagements.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("engagements", "adm_table_pager", sBack, "", "ID", engagementId, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");      

        DataRecordDropDownField dropDownField = new DataRecordDropDownField("engagement_adapter", "name", "id", "group_id", groupId, 60, true);
        dropDownField.Initialize("Engagement adapter", "adm_table_header_nbg", "FormInput", "ADAPTER_ID", false);
        dropDownField.SetWhereString("status=1 and is_active=1");
        theRecord.AddRecord(dropDownField);


        //DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 256);
        //dr_name.setFiledName("Name");
        //dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "name", true);
        //theRecord.AddRecord(dr_name);

        //DataRecordLongTextField dr_Message = new DataRecordLongTextField("ltr", true, 60, 4);
        //dr_Message.setFiledName("Message");
        //dr_Message.Initialize("Message", "adm_table_header_nbg", "FormInput", "Message", true);
        //theRecord.AddRecord(dr_Message);

        //DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        //dr_start_date.setFiledName("StartDateTime");
        //dr_start_date.Initialize("Begin send date & time", "adm_table_header_nbg", "FormInput", "start_time", true);
        //dr_start_date.SetDefault(DateTime.Now);
        

        string sTable = theRecord.GetTableHTML("adm_engagements_new.aspx?submited=1");

        return sTable;
    }   
}