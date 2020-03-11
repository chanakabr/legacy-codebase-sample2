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

public partial class adm_tvp_editorial : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 5, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                string sIP = PageUtils.GetCallerIP();
                string sCountryCode2 = Request.Form["1_val"].ToString();
                string sLanguageFullName = Request.Form["2_val"].ToString();
                string sDeviceName = Request.Form["0_val"].ToString();
                ApiObjects.UserStatus u = (ApiObjects.UserStatus)(int.Parse(Request.Form["3_val"].ToString()));
                //string sToken = TVM.apiWS.api.SetAdminToken(sIP, sCountryCode2, sLanguageFullName, sDeviceName, u , DateTime.UtcNow.AddHours(2) , nGroupID);
                //Session["admin_token"] = sToken;
            }
            else
                Session["admin_token"] = "";
        }
        Response.Expires = -1;
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Cache clear form";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["admin_token"] == null || Session["admin_token"] == "")
        {
            if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
            {
                Session["error_msg"] = "";
                return Session["last_page_html"].ToString();
            }
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            object t = null; ;
            DBRecordWebEditor theRecord = new DBRecordWebEditor("media_locale_values", "adm_table_pager", "", "", "ID", t, "", "");

            DataRecordDropDownField dr_device = new DataRecordDropDownField("groups_devices", "DEVICE_NAME", "DEVICE_NAME", "group_id", nGroupID, 60, false);
            dr_device.Initialize("Application", "adm_table_header_nbg", "FormInput", "device_id", false);
            dr_device.SetFieldType("string");
            dr_device.SetOrderBy("DEVICE_NAME");
            theRecord.AddRecord(dr_device);

            DataRecordDropDownField dr_country = new DataRecordDropDownField("countries", "COUNTRY_NAME", "ID", "", null, 60, false);
            dr_country.SetFieldType("string");
            string sQuery = "select COUNTRY_NAME as txt,COUNTRY_CD2 as id,COUNTRY_CD2 from countries where status=1 and id>0 order by country_name";
            dr_country.SetSelectsQuery(sQuery);
            dr_country.Initialize("Country", "adm_table_header_nbg", "FormInput", "country_id", false);
            dr_country.SetOrderBy("ID");
            theRecord.AddRecord(dr_country);

            DataRecordDropDownField dr_language = new DataRecordDropDownField("lu_languages", "NAME", "id", "", null, 60, false);
            sQuery = "select NAME as txt,NAME as id from lu_languages where status=1 and id>0 order by name";
            dr_language.Initialize("Language", "adm_table_header_nbg", "FormInput", "language_id", false);
            dr_language.SetFieldType("string");
            dr_language.SetSelectsQuery(sQuery);
            dr_language.SetOrderBy("name");
            theRecord.AddRecord(dr_language);

            DataRecordDropDownField dr_user_dtatus = new DataRecordDropDownField("lu_user_states", "DESCRIPTION", "ID", "", null, 60, false);
            dr_user_dtatus.Initialize("User Status", "adm_table_header_nbg", "FormInput", "language_id", false);
            theRecord.AddRecord(dr_user_dtatus);

            string sTable = theRecord.GetTableHTML("adm_tvp_editorial.aspx?submited=1");
            return sTable;
        }
        else
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            object oBaseURL = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
            if (oBaseURL != null && oBaseURL != DBNull.Value)
                return "<div class=alert_text nowrap>Token created. The token will be valid for the next 2 hours. &nbsp;&nbsp;<a target='_blank' href='" + oBaseURL.ToString() + "?admin_token=" + Session["admin_token"].ToString() + "'>Click here to enter the site</a></div>";
            else
                return "<div class=alert_text nowrap>Token created but no base url was found...</div>";
        }
    }
}
