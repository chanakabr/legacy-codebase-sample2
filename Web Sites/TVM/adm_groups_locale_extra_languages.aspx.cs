using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using KLogMonitor;
using TVinciShared;
using System.Data;
using TvinciImporter;
using ApiObjects;
using DAL;

public partial class adm_groups_locale_extra_languages : System.Web.UI.Page
{

    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_groups_locale.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(1, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);

            int groupLocaleId = 0;
            if (Request.QueryString["group_locale_id"] != null && !string.IsNullOrEmpty(Request.QueryString["group_locale_id"].ToString())
                && int.TryParse(Request.QueryString["group_locale_id"].ToString(), out groupLocaleId) && groupLocaleId > 0)
            {
                Session["group_locale_id"] = groupLocaleId;
            }
            else
            {
                Session["group_locale_id"] = 0;
            }  

            Session["localeCurrentLanguages"] = null;
        }
    }    

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Group Locale Extra Languages");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Extra Languages");
        dualList.Add("SecondListTitle", "Available Languages");

        object[] resultData = null;

        int group_locale_id = 0;
        if (Session["group_locale_id"] != null)
        {
            group_locale_id = int.Parse(Session["group_locale_id"].ToString());
        }

        List<object> localeLanguagesList = new List<object>();
        DataTable groupAvailableLanguages = GetGroupAvailableLanguages(group_locale_id);
        HashSet<int> localeCurrentLanguages = group_locale_id > 0 ? GetLocaleCurrentLanguages(group_locale_id) : new HashSet<int>();
        for (int i = 0; i < groupAvailableLanguages.Rows.Count; i++)
        {
            DataRow dr = groupAvailableLanguages.Rows[i];
            int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            string sTitle = ODBCWrapper.Utils.GetSafeStr(dr, "TXT");
            var data = new
            {
                ID = id,
                Title = sTitle,
                Description = sTitle,
                InList = localeCurrentLanguages.Contains(id)
            };

            localeLanguagesList.Add(data);
        }

        Session["localeCurrentLanguages"] = localeCurrentLanguages;

        resultData = new object[localeLanguagesList.Count];
        resultData = localeLanguagesList.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_groups_locale_extra_languages.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    private HashSet<int> GetLocaleCurrentLanguages(int groupLocaleId)
    {
        HashSet<int> localeCurrentLanguages = new HashSet<int>();
        DataTable dt = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        selectQuery += "select distinct language_id from dbo.groups_locale_extra_languages where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_locale_configuration_id", "=", groupLocaleId);
        dt = selectQuery.Execute("query", true);
        if (dt != null && dt.Rows != null)
        {
            foreach (DataRow dr in dt.Rows)
            {
                int languageId = ODBCWrapper.Utils.GetIntSafeVal(dr, "language_id", 0);
                if (languageId > 0 && !localeCurrentLanguages.Contains(languageId))
                {
                    localeCurrentLanguages.Add(languageId);
                }
            }
        }

        selectQuery.Finish();
        selectQuery = null;
        return localeCurrentLanguages;
    }

    private DataTable GetGroupAvailableLanguages(int groupLocaleId)
    {
        DataTable dt = new DataTable();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        selectQuery += @"select distinct l.id, l.name as txt from lu_languages l
                            where id in (select language_id
                                         from dbo.groups g
                                         where g.id=" + LoginManager.GetLoginGroupID() +
                                         @"union all
                                         select language_id
                                         from dbo.group_extra_languages gel
                                         where gel.group_id=" + LoginManager.GetLoginGroupID() +
                                         @"and gel.[status]=1)
                            and id not in (select language_id
                                           from dbo.groups_locale_configuration
                                           where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", groupLocaleId);
        selectQuery += ")";
        dt = selectQuery.Execute("query", true);
        selectQuery.Finish();
        selectQuery = null;

        return dt;
    }

    public string changeItemStatus(string itemId, string sAction)
    {
        int group_locale_id = 0;
        if (Session["group_locale_id"] != null)
        {
            group_locale_id = int.Parse(Session["group_locale_id"].ToString());
            HashSet<int> localeCurrentLanguages = new HashSet<int>();
            if (Session["localeCurrentLanguages"] != null)
            {
                localeCurrentLanguages = Session["localeCurrentLanguages"] as HashSet<int>;
            }

            int groupLocaleExtraLanguageId = 0;
            if (int.TryParse(itemId, out groupLocaleExtraLanguageId))
            {
                int status = localeCurrentLanguages.Contains(groupLocaleExtraLanguageId) ? 2 : 1;
                if (status == 2)
                {
                    localeCurrentLanguages.Remove(groupLocaleExtraLanguageId);
                }
                else
                {
                    localeCurrentLanguages.Add(groupLocaleExtraLanguageId);
                }

                if (!TvmDAL.InsertOrUpdateGroupLocaleExtraLanguage(group_locale_id, groupLocaleExtraLanguageId, status, LoginManager.GetLoginID()))
                {
                    log.ErrorFormat("failed InsertOrUpdateGroupLocaleExtraLanguage for groupLocaleId: {0}, groupLocaleExtraLanguageId: {1}, status: {2}", group_locale_id, groupLocaleExtraLanguageId, status);
                }
            }

            Session["localeCurrentLanguages"] = localeCurrentLanguages;
        }

        return "";
    }        
    
}