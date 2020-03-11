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

public partial class adm_pics_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_pics.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_pics.aspx" , LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(8, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["pic_id"] != null &&
                Request.QueryString["pic_id"].ToString() != "")
            {
                Session["pic_id"] = int.Parse(Request.QueryString["pic_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("pics", "group_id", int.Parse(Session["pic_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["pic_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Pics management");
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
        if (Session["pic_id"] != null && Session["pic_id"].ToString() != "" && int.Parse(Session["pic_id"].ToString()) != 0)
            t = Session["pic_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("pics", "adm_table_pager", "adm_pics.aspx", "", "ID", t, "adm_pics.aspx", "pic_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordUploadField dr_upload = new DataRecordUploadField(60, "pics", true);
        if (Session["pic_id"] != null && Session["pic_id"].ToString() != "" && int.Parse(Session["pic_id"].ToString()) != 0)
            dr_upload.Initialize("The pic", "adm_table_header_nbg", "FormInput", "BASE_URL", false);
        else
            dr_upload.Initialize("The pic", "adm_table_header_nbg", "FormInput", "BASE_URL", true);
        PageUtils.AddCutCroptDimentions(ref dr_upload);
        theRecord.AddRecord(dr_upload);

        DataRecordShortTextField dr_pic_link = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pic_link.Initialize("Pic link", "adm_table_header_nbg", "FormInput", "PIC_LINK", false);
        theRecord.AddRecord(dr_pic_link);

        DataRecordRadioField dr_link_target = new DataRecordRadioField("lu_link_target", "description", "id", "", null);
        dr_link_target.Initialize("Pic link target", "adm_table_header_nbg", "FormInput", "PIC_LINK_TARGET", false);
        dr_link_target.SetDefault(1);
        theRecord.AddRecord(dr_link_target);

        DataRecordShortTextField dr_credit = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_credit.Initialize("Credit", "adm_table_header_nbg", "FormInput", "CREDIT", false);
        theRecord.AddRecord(dr_credit);

        DataRecordShortTextField dr_link = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_link.Initialize("Credit link", "adm_table_header_nbg", "FormInput", "CREDIT_LINK", false);
        theRecord.AddRecord(dr_link);

        DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "pics_tags", "PIC_ID", "TAG_ID", true, "ltr", 60, "tags");
        dr_tags.Initialize("Tags", "adm_table_header_nbg", "FormInput", "VALUE", true);
        dr_tags.SetCollectionLength(8);
        dr_tags.SetExtraWhere("TAG_TYPE_ID=0");
        //dr_tags.SetOrderCollectionBy("CLICK_CNT desc");
        theRecord.AddRecord(dr_tags);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_pics_new.aspx?submited=1");

        return sTable;
    }
}
