using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using CachingProvider.LayeredCache;
using System.Collections.Generic;

public partial class adm_tags_new : System.Web.UI.Page
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
        //else if (PageUtils.IsTvinciUser() == false)
        //LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(9, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                LayeredCache.Instance.InvalidateKeys(new List<string>() { LayeredCacheKeys.GroupManagerGetGroupInvalidationKey(DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID())) });

                return;
            }

            if (Request.QueryString["tag_id"] != null &&
                Request.QueryString["tag_id"].ToString() != "")
            {
                Session["tag_id"] = int.Parse(Request.QueryString["tag_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("tags", "group_id", int.Parse(Session["tag_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["tag_id"] = 0;
        }
    }

    public void GetHeader()
    {
        if (Session["tag_id"] != null && Session["tag_id"].ToString() != "" && int.Parse(Session["tag_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Tags - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Tags - New");
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
        if (Session["tag_id"] != null && Session["tag_id"].ToString() != "" && int.Parse(Session["tag_id"].ToString()) != 0)
            t = Session["tag_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tags", "adm_table_pager", "adm_tags.aspx?search_save=1", "", "ID", t, "adm_tags.aspx?search_save=1", "tag_id");

        DataRecordShortTextField dr_tag = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_tag.Initialize("Tag", "adm_table_header_nbg", "FormInput", "Value", true);
        theRecord.AddRecord(dr_tag);

        string sGroups = PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID());
        DataRecordRadioField dr_tag_type = new DataRecordRadioField("media_tags_types", "name", "id", "", null);
        dr_tag_type.Initialize("Tag type", "adm_table_header_nbg", "FormInput", "TAG_TYPE_ID", true);
        string sQuery = "select name as txt,id as id from media_tags_types where status=1 and (group_id=0 or group_id " + sGroups + ")";
        dr_tag_type.SetSelectsQuery(sQuery);
        theRecord.AddRecord(dr_tag_type);
        
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
