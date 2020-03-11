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

public partial class adm_number_meta : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_number_meta.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_number_meta.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");


        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(12, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":Media Number Meta Names");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void InsertField(ref DBRecordWebEditor theRecord, Int32 nIndex)
    {
        string sMetaName = "Number Meta" + nIndex.ToString() + " Name";
        string sMetaFieldName = "META" + nIndex.ToString() + "_DOUBLE_NAME";
        string sCBField = "IS_META" + nIndex.ToString() + "_DOUBLE_RELATED";
        string sCBOTField = "IS_META" + nIndex.ToString() + "_DOUBLE_ON_TABLE";

        DataRecordShortTextField dr_meta = new DataRecordShortTextField("ltr", true, 20, 128);
        dr_meta.Initialize(sMetaName, "adm_table_header_nbg", "FormInput", sMetaFieldName, false);

        DataRecordCheckBoxField dr_use = new DataRecordCheckBoxField(true);
        dr_use.Initialize("", "adm_table_header_nbg", "FormInput", sCBField, false);

        DataRecordCutWithStrField dr_cut = new DataRecordCutWithStrField(ref dr_use, ref dr_meta, "Check to make the field a related factor");
        dr_cut.Initialize(sMetaName, "adm_table_header_nbg", "FormInput", sMetaFieldName, false);
        theRecord.AddRecord(dr_cut);

        DataRecordCheckBoxField dr_on_table = new DataRecordCheckBoxField(true);
        dr_on_table.Initialize("Is Meta" + nIndex.ToString() + " On Media Table", "adm_table_header_nbg", "FormInput", sCBOTField, false);
        theRecord.AddRecord(dr_on_table);

    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object t = LoginManager.GetLoginGroupID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", "adm_number_meta.aspx", "", "ID", t, "adm_number_meta.aspx", "media_tag_type_id");

        for (int i = 1; i < 11; i++)
        {
            InsertField(ref theRecord, i);
        }

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
