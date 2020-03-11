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

public partial class adm_pics_templates_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_pics_templates.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_pics_templates.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        //else if (PageUtils.IsTvinciUser() == false)
            //LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(11, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            if (Request.QueryString["media_pic_size_id"] != null &&
                Request.QueryString["media_pic_size_id"].ToString() != "")
            {
                Session["media_pic_size_id"] = int.Parse(Request.QueryString["media_pic_size_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_pics_sizes", "group_id", int.Parse(Session["media_pic_size_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["media_pic_size_id"] = 0;
        }
    }

    public void GetHeader()
    {
        if (Session["media_pic_size_id"] != null && Session["media_pic_size_id"].ToString() != "" && int.Parse(Session["media_pic_size_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Pics sizes - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Pics sizes - New");
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
        if (Session["media_pic_size_id"] != null && Session["media_pic_size_id"].ToString() != "" && int.Parse(Session["media_pic_size_id"].ToString()) != 0)
            t = Session["media_pic_size_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_pics_sizes", "adm_table_pager", "adm_pics_templates.aspx", "", "ID", t, "adm_pics_templates.aspx", "media_pic_size_id");

        DataRecordShortTextField dr_description = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", true);
        theRecord.AddRecord(dr_description);

        DataRecordShortIntField dr_width = new DataRecordShortIntField(true, 4, 4);
        dr_width.Initialize("Width", "adm_table_header_nbg", "FormInput", "Width", true);
        theRecord.AddRecord(dr_width);

        DataRecordShortIntField dr_height = new DataRecordShortIntField(true, 4, 4);
        dr_height.Initialize("Height", "adm_table_header_nbg", "FormInput", "Height", true);
        theRecord.AddRecord(dr_height);

        DataRecordRadioField dr_crop = new DataRecordRadioField("lu_crop_or_not", "description", "id", "", null);
        dr_crop.Initialize("Crop", "adm_table_header_nbg", "FormInput", "TO_CROP", true);
        dr_crop.SetDefault(0);
        theRecord.AddRecord(dr_crop);

        
        DataRecordDropDownField dr_ratios = new DataRecordDropDownField("lu_groups_ratios", "ratio", "id", "", null, 60, false);
        dr_ratios.Initialize("Ratio", "adm_table_header_nbg", "FormInput", "RATIO_ID", true);
        dr_ratios.SetSelectsQuery("select lur.ratio as 'txt', lur.id from  lu_pics_ratios lur, groups g where g.id = " + LoginManager.GetLoginGroupID() + " and lur.id = g.ratio_id" + " UNION select lur.ratio as 'txt', lur.id from lu_pics_ratios lur, group_ratios gr where gr.group_id = " + LoginManager.GetLoginGroupID() + " and gr.ratio_id = lur.id and gr.status = 1");  
        //dr_ratios.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
        theRecord.AddRecord(dr_ratios);
        //bool bVisible = PageUtils.IsTvinciUser();
        //if (bVisible == true)
        //{
        //DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
        //dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
        //dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
        //theRecord.AddRecord(dr_groups);
        //}
        //else
        //{
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
        //}

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
