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

public partial class adm_tags_translation_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_tags.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_tags.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(9, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork();
                ProtocolsFuncs.SeperateMediaTranslateTagsByTags(int.Parse(Session["tag_id"].ToString()), int.Parse(Session["lang_id"].ToString()));
                return;
            }
            Int32 nOwnerGroupID = 0;
            if (Request.QueryString["tag_id"] != null &&
                Request.QueryString["tag_id"].ToString() != "")
            {
                Session["tag_id"] = int.Parse(Request.QueryString["tag_id"].ToString());
                nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("tags", "group_id", int.Parse(Session["tag_id"].ToString())).ToString());
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

            if (Request.QueryString["lang_id"] != null &&
                Request.QueryString["lang_id"].ToString() != "")
            {
                Session["lang_id"] = int.Parse(Request.QueryString["lang_id"].ToString());
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from tags_translate where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tag_id", "=", int.Parse(Session["tag_id"].ToString()));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        Session["tag_translation_id"] = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    else
                        Session["tag_translation_id"] = 0;
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":Tags - Translation (" + PageUtils.GetTableSingleVal("tags" , "value" , int.Parse(Session["tag_id"].ToString())) + " to " + PageUtils.GetTableSingleVal("lu_languages" , "name" , int.Parse(Session["lang_id"].ToString())) + " )");
    }

    public void GetHeader1()
    {
        Response.Write("Source Languager: " + PageUtils.GetTableSingleVal("tags", "value", int.Parse(Session["tag_id"].ToString())));
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
        if (Session["tag_translation_id"] != null && Session["tag_translation_id"].ToString() != "" && int.Parse(Session["tag_translation_id"].ToString()) != 0)
            t = Session["tag_translation_id"];
        string sRet = "adm_tags_translate.aspx?search_save=1&lang_id=" + Session["lang_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tags_translate", "adm_table_pager", sRet, "", "ID", t, sRet, "tag_id");

        DataRecordShortTextField dr_tag = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_tag.Initialize("Translation", "adm_table_header_nbg", "FormInput", "Value", true);
        theRecord.AddRecord(dr_tag);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_tag_val = new DataRecordShortIntField(false, 9, 9);
        dr_tag_val.Initialize("Tag", "adm_table_header_nbg", "FormInput", "TAG_ID", false);
        dr_tag_val.SetValue(Session["tag_id"].ToString());
        theRecord.AddRecord(dr_tag_val);

        DataRecordShortIntField dr_lang = new DataRecordShortIntField(false, 9, 9);
        dr_lang.Initialize("Lang", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang.SetValue(Session["lang_id"].ToString());
        theRecord.AddRecord(dr_lang);

        string sTable = theRecord.GetTableHTML("adm_tags_translation_new.aspx?submited=1");
        return sTable;
    }
}
