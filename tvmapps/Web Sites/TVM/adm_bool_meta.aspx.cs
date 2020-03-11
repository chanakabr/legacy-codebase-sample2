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

public partial class adm_bool_meta : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_bool_meta.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_bool_meta.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");


        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(12, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":Media Bool Meta Names");
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
        string sMetaName = "Boolean Meta" + nIndex.ToString() + " Name";
        string sMetaFieldName = "META" + nIndex.ToString() + "_BOOL_NAME";
        string sCBField = "IS_META" + nIndex.ToString() + "_BOOL_RELATED";

        DataRecordShortTextField dr_meta = new DataRecordShortTextField("ltr", true, 20, 128);
        dr_meta.Initialize(sMetaName, "adm_table_header_nbg", "FormInput", sMetaFieldName, false);

        DataRecordCheckBoxField dr_use = new DataRecordCheckBoxField(true);
        dr_use.Initialize("", "adm_table_header_nbg", "FormInput", sCBField, false);

        DataRecordCutWithStrField dr_cut = new DataRecordCutWithStrField(ref dr_use, ref dr_meta, "Check to make the field a related factor");
        dr_cut.Initialize(sMetaName, "adm_table_header_nbg", "FormInput", sMetaFieldName, false);
        theRecord.AddRecord(dr_cut);

    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = LoginManager.GetLoginGroupID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", "adm_bool_meta.aspx", "", "ID", t, "adm_bool_meta.aspx", "media_tag_type_id");

        for (int i = 1; i < 11; i++)
        {
            InsertField(ref theRecord, i);
        }

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
