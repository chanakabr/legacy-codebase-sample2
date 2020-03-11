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
using System.Text;
using System.Text.RegularExpressions;

public partial class adm_media_comments_new : System.Web.UI.Page
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
        Int32 nOwnerGroupID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
            {
                Session["media_id"] = int.Parse(Request.QueryString["media_id"].ToString());
                nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["comment_type_id"] != null &&
                Request.QueryString["comment_type_id"].ToString() != "")
            {
                Session["comment_type_id"] = int.Parse(Request.QueryString["comment_type_id"].ToString());
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["media_comment_id"] != null &&
                Request.QueryString["media_comment_id"].ToString() != "")
            {
                Session["media_comment_id"] = int.Parse(Request.QueryString["media_comment_id"].ToString());
            }
            else
            {
                Session["media_comment_id"] = 0;
            }

            nOwnerGroupID = LoginManager.GetLoginGroupID();
        }
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        //Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("media", "NAME", int.Parse(Session["media_id"].ToString())).ToString() + " Comment from type: " + ODBCWrapper.Utils.GetTableSingleVal("comment_types" , "NAME" , int.Parse(Session["comment_type_id"].ToString())).ToString());
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("media", "NAME", int.Parse(Session["media_id"].ToString())).ToString() + " Comment from type: ");
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
        if (Session["media_comment_id"] != null && Session["media_comment_id"].ToString() != "" && int.Parse(Session["media_comment_id"].ToString()) != 0)
            t = Session["media_comment_id"];
        string sBack = "adm_media_comments.aspx?search_save=1&media_id=" + Session["media_id"].ToString() + "&comment_type_id=" + Session["comment_type_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_comments", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_WRITER = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_WRITER.Initialize("Writer name", "adm_table_header_nbg", "FormInput", "WRITER", true);
        theRecord.AddRecord(dr_WRITER);

        DataRecordShortTextField dr_HEADER = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_HEADER.Initialize("Header", "adm_table_header_nbg", "FormInput", "HEADER", true);
        theRecord.AddRecord(dr_HEADER);

        DataRecordLongTextField dr_sub_HEADER = new DataRecordLongTextField("ltr", true, 60, 2);
        dr_sub_HEADER.Initialize("Sub Header", "adm_table_header_nbg", "FormInput", "SUB_HEADER", true);
        theRecord.AddRecord(dr_sub_HEADER);

        DataRecordLongTextField dr_CONTENT_TEXT = new DataRecordLongTextField("ltr", true, 60, 5);
        dr_CONTENT_TEXT.Initialize("Content", "adm_table_header_nbg", "FormInput", "CONTENT_TEXT", true);
        theRecord.AddRecord(dr_CONTENT_TEXT);

        DataRecordLongTextField dr_FILTRED_CONTENT_TEXT = new DataRecordLongTextField("ltr", false, 60, 5);
        dr_FILTRED_CONTENT_TEXT.Initialize("Filterd", "adm_table_header_nbg", "FormInput", "CONTENT_TEXT", false);
        dr_FILTRED_CONTENT_TEXT.SetValue(GetFiltredContent());
        theRecord.AddRecord(dr_FILTRED_CONTENT_TEXT);

        DataRecordDropDownField dr_lang = new DataRecordDropDownField("lu_languages", "NAME", "id", "", null , 60 , true);
        dr_lang.SetNoSelectStr("Valid for all");
        string sQuery = "select ll.name as txt,ll.id as id from lu_languages ll,groups g where g.id=" + LoginManager.GetLoginGroupID().ToString() + " and (ll.id=g.LANGUAGE_ID or ll.id in (select language_id from group_extra_languages where group_id=" + LoginManager.GetLoginGroupID().ToString() + " and is_active=1 and status=1))";
        dr_lang.SetSelectsQuery(sQuery);
        dr_lang.Initialize("Valid for language", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang.SetDefault(0);
        theRecord.AddRecord(dr_lang);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_COMMENT_TYPE_ID = new DataRecordShortIntField(false, 9, 9);
        dr_COMMENT_TYPE_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "COMMENT_TYPE_ID", false);
        dr_COMMENT_TYPE_ID.SetValue(Session["comment_type_id"].ToString());
        theRecord.AddRecord(dr_COMMENT_TYPE_ID);

        DataRecordShortIntField dr_MEDIA_ID = new DataRecordShortIntField(false, 9, 9);
        dr_MEDIA_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        dr_MEDIA_ID.SetValue(Session["media_id"].ToString());
        theRecord.AddRecord(dr_MEDIA_ID);

        string sTable = theRecord.GetTableHTML("adm_media_comments_new.aspx?submited=1");

        return sTable;
    }

    private string GetFiltredContent()
    {
        Int32 nCommentID = int.Parse(Session["media_comment_id"].ToString());

        string sContent = ODBCWrapper.Utils.GetTableSingleVal("media_comments", "content_text", nCommentID).ToString();


        Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
        Int32 nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nOwnerGroupID).ToString());
        if (nParentGroupID != 1)
        {
            nOwnerGroupID = nParentGroupID;
        }

        string pattern = ODBCWrapper.Utils.GetTableSingleVal("group_language_filters", "Expression", "group_id", "=", nOwnerGroupID).ToString();
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        string output = regex.Replace(sContent, "****");

        return output;
    }
}
