using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_epg_tags_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                // DBManipulator.DoTheWork();
                int nEpgTagTypelID = 0;
                if (Session["epg_tag_id"] != null && Session["epg_tag_id"].ToString() != "" && int.Parse(Session["epg_tag_id"].ToString()) != 0)
                    nEpgTagTypelID = int.Parse(Session["epg_tag_id"].ToString());
                Dictionary<int, List<string>> lTagsDefaults = new Dictionary<int, List<string>>();
                lTagsDefaults.Add(nEpgTagTypelID, new List<string>());
                int groupID = 0;
                int isActive = 0;
                int? orderNum = null;
                int tagTypeFlag = 0;
                string TagName = string.Empty;
                bool bSuccess = GetAllDeafultValues(nEpgTagTypelID, ref lTagsDefaults, ref groupID, ref isActive, ref orderNum, ref TagName, ref tagTypeFlag);

                bool bInsertUpdate = Tvinci.Core.DAL.CatalogDAL.UpdateOrInsert_EPGTagTypeWithDeafultsValues(lTagsDefaults, nEpgTagTypelID, groupID, isActive, orderNum, TagName, tagTypeFlag);
                CachingManager.CachingManager.RemoveFromCache("SetValue_epg_tags_types" + "_");
                m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["epg_tag_id"] != null &&
                Request.QueryString["epg_tag_id"].ToString() != "")
            {
                Session["epg_tag_id"] = int.Parse(Request.QueryString["epg_tag_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("EPG_Tags_Types", "group_id", int.Parse(Session["epg_tag_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_tag_id"] = 0;
        }
    }

    private bool GetAllDeafultValues(int tagID, ref Dictionary<int, List<string>> lTagsDefaults, ref int groupID, ref int isActive, ref int? orderNum, ref string TagName, ref int tagTypeFlag)
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
                if (sType == "string" && coll[nCounter.ToString() + "_field"] != null)
                {
                    string sExtID = coll[nCounter.ToString() + "_field"].ToString();
                    if (sExtID != "")
                    {
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString();
                            if (sExtID.ToLower() == "name")
                            {
                                TagName = sVal;
                            }
                        }

                    }
                }
                else if (sType == "int" && coll[nCounter.ToString() + "_field"] != null)
                {
                    string sExtID = coll[nCounter.ToString() + "_field"].ToString();
                    if (sExtID != "")
                    {
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString();
                            if (!string.IsNullOrEmpty(sVal))
                            {
                                if (sExtID.ToLower() == "is_active")
                                {
                                    isActive = int.Parse(sVal);
                                }
                                if (sExtID.ToLower() == "group_id")
                                {
                                    groupID = int.Parse(sVal);
                                }
                                if (sExtID.ToLower() == "order_num")
                                {
                                    orderNum = int.Parse(sVal);
                                }
                                if (sExtID.ToLower() == "tag_type_flag")
                                {
                                    tagTypeFlag = int.Parse(sVal);
                                }
                            }
                        }

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
                        if (!string.IsNullOrEmpty(sVal))
                        {
                            List<string> lVal = sVal.Split(';').ToList();
                            lTagsDefaults[tagID].AddRange(lVal);
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

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": EPG tags";
        if (Session["epg_tag_id"] != null && Session["epg_tag_id"].ToString() != "" && Session["epg_tag_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
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
        if (Session["epg_tag_id"] != null && Session["epg_tag_id"].ToString() != "" && int.Parse(Session["epg_tag_id"].ToString()) != 0)
            t = Session["epg_tag_id"];
        string sBack = "adm_epg_tags.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_tags_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("tag name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_IsActive = new DataRecordShortIntField(false, 9, 9);
        dr_IsActive.Initialize("IsActive", "adm_table_header_nbg", "FormInput", "is_active", false);
        dr_IsActive.SetValue("1");
        theRecord.AddRecord(dr_IsActive);

        // tags values        
        DataRecordMultiField dr_tags = new DataRecordMultiField("epg_tags", "id", "id", "EPG_tags_types_defaults", "epg_tag_type_id", "epg_tag_id", true, "ltr", 60, "tags");
        dr_tags.Initialize("Values", "adm_table_header_nbg", "FormInput", "Value", false);
        dr_tags.SetCollectionLength(8);
        string s_epg_tag_type_id = "0";
        if (t != null)
        {
            s_epg_tag_type_id = t.ToString();
        }
        dr_tags.SetExtraWhere("epg_tag_type_id=" + s_epg_tag_type_id);

        theRecord.AddRecord(dr_tags);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", false);
        theRecord.AddRecord(dr_order_num);


        DataRecordDropDownField dr_tag_flag = new DataRecordDropDownField("lu_tag_type_flag", "DESCRIPTION", "id", "", null, 60, true);
        dr_tag_flag.SetNoSelectStr("---");
        dr_tag_flag.Initialize("Tag Type Flag", "adm_table_header_nbg", "FormInput", "tag_type_flag", false);
        theRecord.AddRecord(dr_tag_flag);

        string sTable = theRecord.GetTableHTML("adm_epg_tags_new.aspx?submited=1");

        return sTable;
    }
}