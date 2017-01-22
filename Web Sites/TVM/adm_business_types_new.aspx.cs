using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_business_types_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 5, true);
            if (Request.QueryString["groups_media_type_id"] != null &&
                Request.QueryString["groups_media_type_id"].ToString() != "")
            {
                Session["groups_media_type_id"] = int.Parse(Request.QueryString["groups_media_type_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_media_type", "group_id", int.Parse(Session["groups_media_type_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["groups_media_type_id"] = 0;

            if (Request.QueryString["media_type_id"] != null &&
                Request.QueryString["media_type_id"].ToString() != "")
                Session["media_type_id"] = int.Parse(Request.QueryString["media_type_id"].ToString());
            else
            {
                LoginManager.LogoutFromSite("login.html");
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
        string sRet = PageUtils.GetPreHeader() + ": Business Media Types";
        if (Session["media_type_id"] != null)
        {
            string sMediaType = PageUtils.GetTableSingleVal("lu_media_types", "description", int.Parse(Session["media_type_id"].ToString())).ToString();
            sRet += " - " + sMediaType;
            if (Session["groups_media_type_id"] != null && Session["groups_media_type_id"].ToString() != "" && Session["groups_media_type_id"].ToString() != "0")
                sRet += " - Edit";
            else
                sRet += " - New";
        }
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
        if (Session["groups_media_type_id"] != null && Session["groups_media_type_id"].ToString() != "" && int.Parse(Session["groups_media_type_id"].ToString()) != 0)
            t = Session["groups_media_type_id"];

        string sBack = "adm_business_types.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_media_type", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("Friendly Name", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_d);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_mt = new DataRecordShortIntField(false, 9, 9);
        dr_mt.Initialize("Media type id", "adm_table_header_nbg", "FormInput", "MEDIA_TYPE_ID", false);
        dr_mt.SetValue(Session["media_type_id"].ToString());
        theRecord.AddRecord(dr_mt);

        DataRecordCheckBoxField dr_isTrailer = new DataRecordCheckBoxField(true);
        dr_isTrailer.Initialize("Trailer", "adm_table_header_nbg", "FormInput", "IS_TRAILER", false);
        theRecord.AddRecord(dr_isTrailer);

        DataRecordDropDownField dr_streamerType = new DataRecordDropDownField("", "NAME", "id", "", null, 60, true);
        dr_streamerType.SetSelectsDT(GetStreamerTypeDT());
        dr_streamerType.Initialize("Streamer type", "adm_table_header_nbg", "FormInput", "STREAMER_TYPE", false);
        dr_streamerType.SetNoSelectStr("None");
        theRecord.AddRecord(dr_streamerType);

        string sTable = theRecord.GetTableHTML("adm_business_types_new.aspx?submited=1");

        return sTable;
    }

    private System.Data.DataTable GetStreamerTypeDT()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("txt", typeof(string));
        foreach (ApiObjects.StreamerType r in Enum.GetValues(typeof(ApiObjects.StreamerType)))
        {
            dt.Rows.Add((int)r, r);
        }
        return dt;
    }
}
