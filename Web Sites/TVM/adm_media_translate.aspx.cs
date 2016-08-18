using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using KLogMonitor;
using TVinciShared;
using ODBCWrapper;
using System.Collections.Generic;

public partial class adm_media_translate : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork();
                ProtocolsFuncs.SeperateMediaTranslateTexts(int.Parse(Session["media_id"].ToString()),
                    int.Parse(Session["lang_id"].ToString()));

                try
                {
                    Notifiers.BaseMediaNotifier t = null;
                    Notifiers.Utils.GetBaseMediaNotifierImpl(ref t, LoginManager.GetLoginGroupID());
                    if (t != null)
                        t.NotifyChange(Session["media_id"].ToString());
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + Session["media_id"].ToString() + " : " + ex.Message, ex);
                }

                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
            {
                Session["media_id"] = int.Parse(Request.QueryString["media_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
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
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
                Int32 nCO = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select count(*) as co from group_extra_languages where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nOwnerGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nCO == 0)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                m_sLangMenu = GetLangMenu(nOwnerGroupID);
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
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
            sTemp += "<li><a href=\"";
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
                    if (int.Parse(Session["lang_id"].ToString()) == nLangID)
                        sTemp += "<li><a class=\"on\" href=\"";
                    else
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
        Response.Write(PageUtils.GetPreHeader() + ": Media management translation (" + PageUtils.GetTableSingleVal("media", "name", int.Parse(Session["media_id"].ToString())).ToString());
        //Response.Write(PageUtils.GetPreHeader() + ": Media management translation");
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

    protected void AddStrFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 21; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_STR_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_STR";
                        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 255);
                        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
                        theRecord.AddRecord(dr_name);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void AddTagsFields(ref DBRecordWebEditor theRecord, int media_id, int lang_id)
    {
        List<string> tagsTypeTranslate = new List<string>();
        string sGroups = PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID());
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from media_tags_types where (status=1 and TagFamilyID IS NULL and group_id " + sGroups+")"  ;
        selectQuery += " or (group_id=0 and TagFamilyID = 1) ";
        selectQuery += "order by order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            DataTable dt = selectQuery.Table("query");
                       
            foreach (DataRow dr in dt.Rows)
            {
                string TagTypeName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                tagsTypeTranslate.Add(TagTypeName);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
               
        string name = string.Empty;
        string value = string.Empty;
       
       selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " SELECT mtt.id AS tag_type_id, mtt.NAME AS tag_type_name ,t.id AS tag_id ,tt.value as translateTagValue, t.value as tagValue ";
        selectQuery += " FROM dbo.tags t WITH(NOLOCK) INNER JOIN dbo.media_tags_types mtt WITH(NOLOCK) ON mtt.id = t.TAG_TYPE_ID ";
        selectQuery += " INNER JOIN dbo.media_tags mt WITH (NOLOCK) ON mt.tag_id = t.id ";
        selectQuery += " LEFT JOIN dbo.tags_translate tt WITH(NOLOCK) ON tt.tag_id = t.id AND ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tt.LANGUAGE_ID", "=", lang_id);
        selectQuery += " WHERE t.STATUS = 1 AND mt.STATUS = 1 AND mtt.STATUS = 1 AND ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mt.MEDIA_ID", "=", media_id);

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            DataTable dt = selectQuery.Table("query");
            
            string tagType = string.Empty;
            string tagValue = string.Empty;
            List<string> tempvalues;           
            foreach (string tags in tagsTypeTranslate)
            {
                DataRow[] tagValues = dt.Select("tag_type_name ='" + tags + "'");
                tempvalues = new List<string>();
                foreach (DataRow dr in tagValues)
                {
                    tagValue = ODBCWrapper.Utils.GetSafeStr(dr, "translateTagValue");
                    tempvalues.Add(tagValue);                    
                }
               

                DataRecordLongTextField dr_name = new DataRecordLongTextField("ltr", false, 60, 4);
                dr_name.Initialize(tags, "adm_table_header_nbg", "FormInput", "", false);
                dr_name.SetValue(string.Join("; ", tempvalues));
                theRecord.AddRecord(dr_name);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null;
        int media_id = 0;
        int lang_id = 0;
        if (Session["media_id"] != null && Session["media_id"].ToString() != "" && int.Parse(Session["media_id"].ToString()) != 0)
        {
            media_id = int.Parse(Session["media_id"].ToString());
            if (Session["lang_id"] != null && Session["lang_id"].ToString() != "" && int.Parse(Session["lang_id"].ToString()) != 0)
            {
                lang_id = int.Parse(Session["lang_id"].ToString());
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from media_translate where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", int.Parse(Session["media_id"].ToString()));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        t = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
        }
        string sRet = "adm_media_new.aspx?media_id=" + Session["media_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_translate", "adm_table_pager", sRet, "", "ID", t, sRet, "media_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        AddStrFields(ref theRecord);
        AddTagsFields(ref theRecord, media_id, lang_id);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_media_id = new DataRecordShortIntField(false, 9, 9);
        dr_media_id.Initialize("Media", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        dr_media_id.SetValue(Session["media_id"].ToString());
        theRecord.AddRecord(dr_media_id);

        DataRecordShortIntField dr_lang_id = new DataRecordShortIntField(false, 9, 9);
        dr_lang_id.Initialize("Lang", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang_id.SetValue(Session["lang_id"].ToString());
        theRecord.AddRecord(dr_lang_id);

        string sTable = theRecord.GetTableHTML("adm_media_translate.aspx?submited=1");

        return sTable;
    }
}
