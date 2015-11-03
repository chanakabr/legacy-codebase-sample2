using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using DAL;
using System.Data;
using KLogMonitor;
using System.Reflection;

public partial class adm_domain_limitation_modules_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!LoginManager.CheckLogin())
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int newLimitID = DBManipulator.DoTheWork();
                if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0 && Session["device_families"] != null &&
                    Session["device_families"] is List<UMObj>)
                {
                    List<UMObj> updatedDeviceFamilyObjs = Session["device_families"] as List<UMObj>;
                    List<int> updatedDeviceFamilyIDs = updatedDeviceFamilyObjs.Select(item => Int32.Parse(item.m_id)).ToList<int>();
                    int limitID = Int32.Parse(Session["limit_id"].ToString());
                    if (limitID == 0)
                    {
                        limitID = newLimitID;
                    }
                    int groupID = LoginManager.GetLoginGroupID();
                    if (limitID > 0 && updatedDeviceFamilyIDs != null && groupID > 0)
                    {
                        ODBCWrapper.DataSetSelectQuery selectQuery = null;
                        try
                        {
                            List<int> currentDeviceFamilyIDs = null;
                            selectQuery = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery += "select device_family_id from groups_device_families with (nolock) where is_active=1 and [status]=1";
                            selectQuery += " and ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
                            selectQuery += " and ";
                            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("limit_module_id", "=", limitID);
                            if (selectQuery.Execute("query", true) != null)
                            {
                                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                currentDeviceFamilyIDs = new List<int>(nCount);
                                for (int i = 0; i < nCount; i++)
                                {
                                    currentDeviceFamilyIDs.Add(Int32.Parse(selectQuery.Table("query").DefaultView[i].Row["device_family_id"].ToString()));

                                } // end for

                                UpdateDeviceFamilies(groupID, limitID, updatedDeviceFamilyIDs, currentDeviceFamilyIDs);

                                // delete from cache this DLM object    
                                DomainsWS.module p = new DomainsWS.module();

                                string sIP = "1.1.1.1";
                                string sWSUserName = "";
                                string sWSPass = "";
                                TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
                                string sWSURL = GetWSURL("domains_ws");
                                if (sWSURL != "")
                                    p.Url = sWSURL;
                                try
                                {
                                    DomainsWS.Status resp = p.RemoveDLM(sWSUserName, sWSPass, limitID);
                                    log.Debug("RemoveDLM - " + string.Format("Dlm:{0}, res:{1}", limitID, resp.Code));
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", limitID, ex.Message, ex.StackTrace), ex);
                                }
                            }
                        }
                        finally
                        {
                            if (selectQuery != null)
                            {
                                selectQuery.Finish();
                                selectQuery = null;
                            }
                        }
                    }
                }
                Session["limit_id"] = null;
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["limit_id"] != null &&
                Request.QueryString["limit_id"].ToString().Length > 0)
            {
                Session["limit_id"] = int.Parse(Request.QueryString["limit_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_limitation_modules", "group_id", int.Parse(Session["limit_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && !PageUtils.IsTvinciUser())
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                Session["limit_id"] = 0;
            }



        }
    }

    private void UpdateDeviceFamilies(int groupID, int limitID, List<int> updatedDeviceFamilyIDs, List<int> currentDeviceFamilyIDs)
    {
        for (int i = 0; i < updatedDeviceFamilyIDs.Count; i++)
        {
            if (!currentDeviceFamilyIDs.Contains(updatedDeviceFamilyIDs[i]))
            {
                TvmDAL.Insert_DeviceFamilyToGroup(groupID, updatedDeviceFamilyIDs[i], limitID);
            }
        }

        for (int j = 0; j < currentDeviceFamilyIDs.Count; j++)
        {
            if (!updatedDeviceFamilyIDs.Contains(currentDeviceFamilyIDs[j]))
            {
                TvmDAL.Update_DeviceFamilyStatus(groupID, currentDeviceFamilyIDs[j], limitID, true);
            }
        }
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Device Lmitation Module";
        if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0 && Session["limit_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected bool ValidateLimit()
    {
        bool retVal = true;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["1_field"] != null && coll["1_val"] != null)
        {
            string limitStr = coll["1_val"].ToString();
            if (!string.IsNullOrEmpty(limitStr))
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    int limitInt = int.Parse(limitStr.Trim());
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select max_device_limit from groups with (nolock) where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            int maxLimit = int.Parse(selectQuery.Table("query").DefaultView[0].Row["max_device_limit"].ToString());
                            if (maxLimit < limitInt)
                            {
                                Session["error_msg"] = "Limit exceeds Account Max Device Limit";
                                retVal = false;
                            }
                        }
                    }
                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                        selectQuery = null;
                    }
                }
            } // end if(coll[]..)

        }
        return retVal;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString().Length > 0)
        {
            Session["error_msg"] = string.Empty;
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0 && int.Parse(Session["limit_id"].ToString()) != 0)
            t = Session["limit_id"];
        string sBack = "adm_domain_limitation_modules.aspx?search_save=1";

        int nGroupID = LoginManager.GetLoginGroupID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_device_limitation_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_limit = new DataRecordShortIntField(true, 9, 9);
        dr_limit.Initialize("Device Limit", "adm_table_header_nbg", "FormInput", "max_limit", false);
        theRecord.AddRecord(dr_limit);

        DataRecordDropDownField dr_frequency = new DataRecordDropDownField("lu_min_periods", "Description", "ID", string.Empty, string.Empty, 60, true);
        dr_frequency.Initialize("Device Change Frequency", "adm_table_header_nbg", "FormInput", "freq_period_id", false);
        theRecord.AddRecord(dr_frequency);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(nGroupID.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_concurrent_limit = new DataRecordShortIntField(true, 9, 9);
        dr_concurrent_limit.Initialize("Concurrent Limit", "adm_table_header_nbg", "FormInput", "concurrent_max_limit", false);
        theRecord.AddRecord(dr_concurrent_limit);

        DataRecordDropDownField dr_env_type = new DataRecordDropDownField("lu_domain_environment", "Description", "ID", string.Empty, string.Empty, 60, true);
        string sQuery = "select Description as txt,ID from lu_domain_environment with (nolock) ";
        dr_env_type.SetSelectsQuery(sQuery);
        dr_env_type.Initialize("Environment type", "adm_table_header_nbg", "FormInput", "environment_type", false);

        dr_env_type.SetDefaultVal(getDomainEnvironment(nGroupID, t));
        theRecord.AddRecord(dr_env_type);

        DataRecordShortIntField dr_hn_limit = new DataRecordShortIntField(true, 9, 9);
        dr_hn_limit.Initialize("Home Network Limit", "adm_table_header_nbg", "FormInput", "Home_network_quantity", false);
        theRecord.AddRecord(dr_hn_limit);

        DataRecordDropDownField dr_hn_frequency = new DataRecordDropDownField("lu_min_periods", "Description", "ID", string.Empty, string.Empty, 60, true);
        dr_hn_frequency.Initialize("Home Network Frequency", "adm_table_header_nbg", "FormInput", "Home_network_frequency", false);
        theRecord.AddRecord(dr_hn_frequency);

        DataRecordShortIntField dr_user_limit = new DataRecordShortIntField(true, 9, 9);
        dr_user_limit.Initialize("User Limit", "adm_table_header_nbg", "FormInput", "user_max_limit", false);
        theRecord.AddRecord(dr_user_limit);

        DataRecordDropDownField dr_user_frequency = new DataRecordDropDownField("lu_min_periods", "Description", "ID", string.Empty, string.Empty, 60, true);
        dr_user_frequency.Initialize("User Change Frequency", "adm_table_header_nbg", "FormInput", "user_freq_period_id", false);
        theRecord.AddRecord(dr_user_frequency);

        string sTable = theRecord.GetTableHTML("adm_domain_limitation_modules_new.aspx?submited=1");

        return sTable;
    }

    private string getDomainEnvironment(int nGroupID, object t)
    {
        string sDomainEnv = string.Empty;
        if (t != null)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            int nRowID = int.Parse(t.ToString());
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();

                selectQuery += "select glimit.ID, dm.description from groups g (nolock)";
                selectQuery += "inner Join groups_device_limitation_modules glimit (nolock) on";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("glimit.ID", "=", nRowID);
                selectQuery += "Inner Join lu_domain_environment dm (nolock) on dm.ID = glimit.environment_type where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.ID", "=", nGroupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sDomainEnv = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "description", 0);
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                    selectQuery = null;
                }
            }

        }
        return sDomainEnv;
    }

    public string initDualObj()
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = null;
        List<UMObj> allFamilies = null;
        List<UMObj> limitFamilies = null;
        List<UMObj> complementLimitFamilies = null;
        try
        {
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
            int limitID = 0;
            string sRet = string.Empty;
            sRet += "Device Families";
            sRet += "~~|~~";
            sRet += "Available Device Families";
            sRet += "~~|~~";
            sRet += "<root>";
            if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0)
            {
                limitID = Int32.Parse(Session["limit_id"].ToString());
                BuildLimitationDeviceFamilies(limitID, nLogedInGroupID, ref allFamilies, ref limitFamilies, ref complementLimitFamilies);
            }

            if (allFamilies != null && allFamilies.Count > 0)
            {
                for (int i = 0; i < allFamilies.Count; i++)
                {
                    bool bIsOK = true;
                    string sID = allFamilies[i].m_id;
                    for (int j = 0; j < limitFamilies.Count; j++)
                    {
                        if (limitFamilies[j].m_id.Equals(sID))
                        {
                            bIsOK = false;
                            break;
                        }
                    }

                    if (bIsOK)
                    {
                        sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(allFamilies[i].m_title, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(string.Empty, true) + "\" inList=\"false\" />";
                    }
                } // end bigger for

                if (Session["limit_id"] != null && Session["device_families"] != null)
                {
                    int limitationID = int.Parse(Session["limit_id"].ToString());
                    List<UMObj> umObjList = Session["device_families"] as List<UMObj>;
                    foreach (UMObj obj in umObjList)
                    {
                        sRet += "<item id=\"" + obj.m_id + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(obj.m_title, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(obj.m_description, true) + "\" inList=\"true\" />";
                    }
                }
            }

            sRet += "</root>";
            return sRet;

        }
        finally
        {
            if (selectQuery != null)
            {
                selectQuery.Finish();
                selectQuery = null;
            }
        }
    }

    protected List<int> BuildLimitationDeviceFamilies(int limitID, int groupID)
    {
        List<int> res = new List<int>();
        List<UMObj> lst = new List<UMObj>();
        ODBCWrapper.DataSetSelectQuery selectQuery = null;
        try
        {
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CONNECTION_STRING");
            selectQuery += "select gdf.device_family_id, ludf.name from groups_device_families gdf with (nolock) ";
            selectQuery += "inner join lu_DeviceFamily ludf with (nolock) on gdf.device_family_id=ludf.id ";
            selectQuery += " where gdf.is_active=1 and gdf.[status]=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gdf.group_id", "=", groupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gdf.limit_module_id", "=", limitID);
            selectQuery += " order by ludf.id asc";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string title = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();
                        string uID = selectQuery.Table("query").DefaultView[i].Row["device_family_id"].ToString();
                        UMObj umObj = new UMObj(uID, title, string.Empty, true, i);
                        res.Add(Int32.Parse(uID));
                        lst.Add(umObj);
                    }
                }

            }
        }
        finally
        {
            if (selectQuery != null)
            {
                selectQuery.Finish();
                selectQuery = null;
            }
        }

        Session["device_families"] = lst;


        return res;
    }

    protected void BuildLimitationDeviceFamilies(int limitID, int groupID, ref List<UMObj> allFamiliesList,
        ref List<UMObj> limitFamiliesList, ref List<UMObj> complementLimitFamiliesList)
    {
        List<int> res = new List<int>();
        List<UMObj> lst = new List<UMObj>();
        DataSet ds = TvmDAL.Get_DeviceFamiliesLimitationsData(groupID, limitID);
        if (ds != null && ds.Tables != null && ds.Tables.Count == 3)
        {
            DataTable allFamilies = ds.Tables[0];
            DataTable limitFamilies = ds.Tables[1];
            DataTable complementLimitFamilies = ds.Tables[2];
            if (allFamilies != null && allFamilies.Rows != null && allFamilies.Rows.Count > 0)
            {
                allFamiliesList = new List<UMObj>(allFamilies.Rows.Count);
                for (int i = 0; i < allFamilies.Rows.Count; i++)
                {
                    string sID = ODBCWrapper.Utils.GetSafeStr(allFamilies.Rows[i]["ID"]);
                    string sTitle = ODBCWrapper.Utils.GetSafeStr(allFamilies.Rows[i]["NAME"]);
                    allFamiliesList.Add(new UMObj(sID, sTitle, string.Empty, false, 0));
                }
            }
            else
            {
                allFamiliesList = new List<UMObj>(0);
            }

            if (limitFamilies != null && limitFamilies.Rows != null && limitFamilies.Rows.Count > 0)
            {
                limitFamiliesList = new List<UMObj>(limitFamilies.Rows.Count);
                for (int i = 0; i < limitFamilies.Rows.Count; i++)
                {
                    string sID = ODBCWrapper.Utils.GetSafeStr(limitFamilies.Rows[i]["DEVICE_FAMILY_ID"]);
                    string sName = ODBCWrapper.Utils.GetSafeStr(limitFamilies.Rows[i]["NAME"]);
                    limitFamiliesList.Add(new UMObj(sID, sName, string.Empty, false, 0));
                }
            }
            else
            {
                limitFamiliesList = new List<UMObj>(0);
            }

            if (complementLimitFamilies != null && complementLimitFamilies.Rows != null && complementLimitFamilies.Rows.Count > 0)
            {
                complementLimitFamiliesList = new List<UMObj>(complementLimitFamilies.Rows.Count);
                for (int i = 0; i < complementLimitFamilies.Rows.Count; i++)
                {
                    string sID = ODBCWrapper.Utils.GetSafeStr(complementLimitFamilies.Rows[i]["DEVICE_FAMILY_ID"]);
                    string sName = ODBCWrapper.Utils.GetSafeStr(complementLimitFamilies.Rows[i]["NAME"]);
                    complementLimitFamiliesList.Add(new UMObj(sID, sName, string.Empty, false, 0));
                }
            }
            else
            {
                complementLimitFamiliesList = new List<UMObj>(0);
            }

            Session["device_families"] = limitFamiliesList;

        }
        else
        {
            Session["device_families"] = new List<UMObj>(0);
        }

    }

    public string changeItemStatus(string sID, string sAction, string index)
    {
        string retVal = string.Empty;
        if (Session["device_families"] != null && Session["device_families"] is List<UMObj>)
        {
            List<UMObj> umObjList = Session["device_families"] as List<UMObj>;
            string action = sAction.ToLower();
            switch (action)
            {
                case "remove":
                    {
                        for (int i = 0; i < umObjList.Count; i++)
                        {
                            UMObj obj = umObjList[i];
                            if (obj.m_id.Equals(sID))
                            {
                                umObjList.Remove(obj);
                                break;
                            }
                        }
                        Session["device_families"] = umObjList;
                        break;
                    }
                case "add":
                    {
                        UMObj obj = new UMObj(sID, string.Empty, string.Empty, true, int.Parse(index));
                        int newOrder = int.Parse(index);

                        foreach (UMObj umObj in umObjList)
                        {
                            int oldOrder = umObj.m_orderNum;

                            if (oldOrder >= newOrder)
                            {
                                umObj.m_orderNum++;
                            }

                        }
                        umObjList.Insert(int.Parse(index), obj);
                        umObjList.Sort();
                        Session["device_families"] = umObjList;
                        break;
                    }
            }

            // delete from cache this DLM object    
            if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0)
            {
                int limitID = 0;
                int.TryParse(Session["limit_id"].ToString(), out limitID);
                // delete from cache this DLM object    
                DomainsWS.module p = new DomainsWS.module();

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = GetWSURL("domains_ws");
                if (sWSURL != "")
                    p.Url = sWSURL;
                try
                {
                    DomainsWS.Status resp = p.RemoveDLM(sWSUserName, sWSPass, limitID);
                    log.Debug("RemoveDLM - " + string.Format("Dlm:{0}, res:{1}", limitID, resp.Code));
                }
                catch (Exception ex)
                {
                    log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", limitID, ex.Message, ex.StackTrace), ex);
                }
            }

        }
        return retVal;
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }
}
