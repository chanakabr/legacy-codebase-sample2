using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_epg_channel_defaults : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Dictionary<int, List<string>> dMetasDefaults = new Dictionary<int, List<string>>();
                Dictionary<int, List<string>> dTagsDefaults = new Dictionary<int, List<string>>();
                bool bSuccess = GetAllDeafultValues(ref dMetasDefaults, ref dTagsDefaults);
                int nEpgChannelID = 0;
                if (Session["epg_channel_id"] != null && Session["epg_channel_id"].ToString() != "" && int.Parse(Session["epg_channel_id"].ToString()) != 0)
                    nEpgChannelID = int.Parse(Session["epg_channel_id"].ToString());

                bool bInsertUpdate = Tvinci.Core.DAL.CatalogDAL.UpdateOrInsert_EPGDeafultsValues(dMetasDefaults, dTagsDefaults, nEpgChannelID);

                m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
                return;
            }



            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["epg_channel_id"] != null &&
                Request.QueryString["epg_channel_id"].ToString() != "")
            {
                Session["epg_channel_id"] = int.Parse(Request.QueryString["epg_channel_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                //  m_sLangMenu = GetLangMenu(nOwnerGroupID);
            }
            else
                Session["epg_channel_id"] = 0;
        }
    }

    private bool GetAllDeafultValues(ref Dictionary<int, List<string>> dMetasDefaults, ref Dictionary<int, List<string>> dTagsDefaults)
    {
        NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["table_name"] == null)
        {
            HttpContext.Current.Session["error_msg"] = "missing table name - cannot update";
        }
        int nCounter = 0;
        try
        {
            while (nCounter < coll.Count)
            {
                string sType = "";
                if (coll[nCounter.ToString() + "_type"] == null)
                    break;
                else
                    sType = coll[nCounter.ToString() + "_type"];
                string sVal = "";

                if (sType == "string" && coll[nCounter.ToString() + "_ext"] != null)
                {
                    string sExtID = coll[nCounter.ToString() + "_ext"].ToString();
                    if (sExtID != "")
                    {
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString();
                        }
                        int id = int.Parse(sExtID);
                        dMetasDefaults.Add(id, new List<string> { sVal });
                    }
                }
                else if (sType == "multi" && coll[nCounter.ToString() + "_extra_field_val"] != null)
                {
                    string sExtID = coll[nCounter.ToString() + "_extra_field_val"].ToString();
                    if (!string.IsNullOrEmpty(sExtID))
                    {
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString().TrimEnd(';');
                        }

                        int id = int.Parse(sExtID);
                        if (dTagsDefaults.ContainsKey(id))
                        {
                            dTagsDefaults[id].Add(sVal);
                        }
                        else
                        {
                            dTagsDefaults.Add(id, new List<string>() { sVal });
                        }
                    }
                }
                nCounter++;
            }
            return true;
        }
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
            return false;
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
            sTemp += "<li><a class=\"on\" href=\"";
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
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Media management");
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

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        Dictionary<string, string> filedList = new Dictionary<string, string>();
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["epg_channel_id"] != null && Session["epg_channel_id"].ToString() != "" && int.Parse(Session["epg_channel_id"].ToString()) != 0)
            t = Session["epg_channel_id"];
        string sBack = "adm_epg_channels.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_channels", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
        int nCount = 0;
        #region  get all metas
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select  e.id as idEPG_channel_deafults_values, e.Meta_TAG_TYPE_ID , e.default_value as EpgChannelDeafultValues , emt.id , emt.name   , emt.default_value  as GroupGroupDeafultValues ";
        selectQuery += " FROM EPG_metas_types emt left join EPG_channel_deafults_values e on emt.group_id = e.group_id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("e.EPG_CHANNEL_ID", "=", t);
        selectQuery += "  and emt.id = e.Meta_TAG_TYPE_ID and e.type = 2 ";
        selectQuery += " where  emt.status = 1 and emt.is_active = 1 ";
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("emt.group_id", "=", nOwnerGroupID);


        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 0; i < nCount; i++)
                {
                    string sMetaName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    string sMetaFiled = "default_value";

                    int nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    string sEpgMetaValue = selectQuery.Table("query").DefaultView[i].Row["EpgChannelDeafultValues"].ToString();
                    string sGroupMetaValue = selectQuery.Table("query").DefaultView[i].Row["GroupGroupDeafultValues"].ToString();

                    DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128, nID);
                    dr_name.Initialize(sMetaName, "adm_table_header_nbg", "FormInput", sMetaFiled, false);
                    string val = "";
                    if (!string.IsNullOrEmpty(sEpgMetaValue))
                    {
                        val += sEpgMetaValue; // only one value 
                    }
                    else if (!string.IsNullOrEmpty(sGroupMetaValue))
                    {
                        val += sGroupMetaValue; // only one value 
                    }
                    dr_name.SetValue(val);
                    theRecord.AddRecord(dr_name);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        #endregion


        #region  get all tags
        int channelID = 0;
        if (t != null)
        {
            channelID = (int)t;
        }
        DataTable dt = DAL.TvmDAL.GetTagsWithDefaultValues(channelID, nOwnerGroupID);
        int prevID = 0;
        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
        {
            foreach (DataRow dr in dt.Rows)
            {
                int nID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                if (prevID != nID)
                {
                    DataRow[] drs = dt.Select("id = " + nID);
                    string sTagTypeName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                    Int32 nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                    string val = string.Empty;
                    List<string> lVals = new List<string>();
                    foreach (DataRow item in drs)
                    {
                        string sTagValue = ODBCWrapper.Utils.GetSafeStr(item, "EpgChannelDeafultValues");
                        string sTagGroupValue = ODBCWrapper.Utils.GetSafeStr(item, "GroupTagDeafultValues");

                        if (!string.IsNullOrEmpty(sTagValue))
                        {
                            lVals.Add(sTagValue);
                            break; // if this is an epf channels values - stop the foreach loop after one time 
                        }
                        else if (!string.IsNullOrEmpty(sTagGroupValue)) // need to get the default value for the group
                        {
                            lVals.Add(sTagGroupValue);
                        }
                    }
                    DataRecordMultiField dr_tags = new DataRecordMultiField("epg_tags", "id", "id", "EPG_channel_deafults_values", "epg_channel_id", "Meta_TAG_TYPE_ID", true, "ltr", 60, "epg_tags");///
                    dr_tags.Initialize(sTagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);
                    dr_tags.SetCollectionLength(8);
                    val = string.Join(";", lVals);
                    dr_tags.SetValue(val);
                    dr_tags.SetExtraWhere("epg_tag_type_id=" + nTagTypeID.ToString());
                    theRecord.AddRecord(dr_tags);
                    prevID = nID;
                }
            }
        }
        #endregion


        string sTable = theRecord.GetTableHTML("adm_epg_channel_defaults.aspx?submited=1");

        return sTable;
    }
}