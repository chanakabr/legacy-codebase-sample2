using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using DAL;
using System.Collections.Specialized;
using KLogMonitor;
using System.Reflection;

public partial class adm_limitation_modules_new : System.Web.UI.Page
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
                if (IsValidFrequencyValue())
                {

                    int idOfOverridingRule = DBManipulator.DoTheWork();
                    if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0 && Session["device_families"] != null &&
                        Session["device_families"] is List<UMObj> && Session["parent_limit_id"] != null
                        && Session["parent_limit_id"].ToString().Length > 0)
                    {
                        List<UMObj> updatedDeviceFamilyObjs = Session["device_families"] as List<UMObj>;
                        List<int> updatedDeviceFamilyIDs = updatedDeviceFamilyObjs.Select(item => Int32.Parse(item.m_id)).ToList<int>();
                        int limitID = Int32.Parse(Session["limit_id"].ToString());
                        if (limitID == 0)
                            limitID = idOfOverridingRule;
                        int parentLimitID = Int32.Parse(Session["parent_limit_id"].ToString());
                        int groupID = LoginManager.GetLoginGroupID();
                        if (limitID > 0 && updatedDeviceFamilyIDs != null && groupID > 0)
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery = null;
                            try
                            {
                                List<int> currentDeviceFamilyIDs = null;
                                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                                selectQuery += "select device_family_id from device_families_limitation_modules with (nolock) where is_active=1 and [status]=1";
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
                                selectQuery += " and ";
                                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_limitation_module_id", "=", limitID);
                                //selectQuery += " and ";
                                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("limit_module", "=", parentLimitID);
                                if (selectQuery.Execute("query", true) != null)
                                {
                                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                                    currentDeviceFamilyIDs = new List<int>(nCount);
                                    for (int i = 0; i < nCount; i++)
                                    {
                                        currentDeviceFamilyIDs.Add(Int32.Parse(selectQuery.Table("query").DefaultView[i].Row["device_family_id"].ToString()));

                                    } // end for
                                }

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
                                    DomainsWS.Status resp = p.RemoveDLM(sWSUserName, sWSPass, parentLimitID);
                                    log.Debug("RemoveDLM - " + string.Format("Dlm:{0}, res:{1}", parentLimitID, resp.Code));
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", parentLimitID, ex.Message, ex.StackTrace), ex);
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
                    //Session["limit_id"] = null;
                    //Session["parent_limit_id"] = null;
                    return;
                }
                else
                {
                    HttpContext.Current.Session["error_msg"] = "frequency value can be only 0!";
                }
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);

            if (Request.QueryString["limit_id"] != null &&
                    Request.QueryString["limit_id"].ToString().Length > 0 /*&& Request.QueryString["limit_id"].ToString().Trim() != "0"*/)
            {
                Session["limit_id"] = int.Parse(Request.QueryString["limit_id"].ToString());
                Int32 nOwnerGroupID = 0;
                try
                {
                    nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("device_families_limitation_modules", "group_id", int.Parse(Session["limit_id"].ToString())).ToString());
                }
                catch (Exception ex)
                {
                    log.Error(string.Empty, ex);
                }
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && !PageUtils.IsTvinciUser())
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                //if (Session["limit_id"] == null || Session["limit_id"].ToString().Length == 0)
                //{
                Session["limit_id"] = 0;
                //}
            }

            if (Request.QueryString["parent_limit_id"] != null && Request.QueryString["parent_limit_id"].ToString().Length > 0)
            {
                Session["parent_limit_id"] = Int32.Parse(Request.QueryString["parent_limit_id"]);
            }
            else
            {
                //if (Session["parent_limit_id"] == null || Session["parent_limit_id"].ToString().Length == 0)
                //{
                Session["parent_limit_id"] = 0;
                //}
            }


        }
    }


    // validates the Value field is 0, only in case the selected Limit Type is Frequency.
    // uses "hard coded" values, in case of updating the page / values, the method MUST be updated as well.
    private bool IsValidFrequencyValue()
    {
        NameValueCollection coll = HttpContext.Current.Request.Form;
        string ruleType = coll["1_val"];
        string ruleVal = coll["2_val"];
        if (ruleType == "3" && ruleVal != "0")
            return false;
        else
            return true;

    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    private void UpdateDeviceFamilies(int groupID, int limitID,
        List<int> updatedDeviceFamilyIDs, List<int> currentDeviceFamilyIDs)
    {
        bool temp = false;
        for (int i = 0; i < updatedDeviceFamilyIDs.Count; i++)
        {
            if (!currentDeviceFamilyIDs.Contains(updatedDeviceFamilyIDs[i]))
            {
                temp = TvmDAL.Insert_DeviceFamilyToLimitationModule(groupID, updatedDeviceFamilyIDs[i], limitID);
            }
        }

        for (int j = 0; j < currentDeviceFamilyIDs.Count; j++)
        {
            if (!updatedDeviceFamilyIDs.Contains(currentDeviceFamilyIDs[j]))
            {
                temp = TvmDAL.Update_DeviceFamilyToLimitationID(groupID, limitID, currentDeviceFamilyIDs[j], true);
            }
        }
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Override Limitation";
        if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0 /* && Session["limit_id"].ToString().Trim() != "0" */
            && Session["parent_limit_id"] != null && Session["parent_limit_id"].ToString().Length > 0 &&
            Session["parent_limit_id"].ToString().Trim() != "0")
        {
            sRet += " - Edit";
        }
        else
        {
            sRet += " - New";
        }
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
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
        string sBack = "adm_limitation_modules.aspx?search_save=1";

        int nGroupID = LoginManager.GetLoginGroupID();
        int parentLimitModuleID = 0;
        parentLimitModuleID = Int32.Parse(Session["parent_limit_id"].ToString());
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_device_families_limitation_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Description", true);
        theRecord.AddRecord(dr_Name);

        DataRecordDropDownField dr_limit_type = new DataRecordDropDownField("lu_device_limitation_modules", "Description", "ID", string.Empty, string.Empty, 60, true);
        string sQuery = "select Description as txt,ID from lu_device_limitation_modules with (nolock) where status=1 order by id asc";
        dr_limit_type.SetSelectsQuery(sQuery);
        dr_limit_type.Initialize("Limit Type", "adm_table_header_nbg", "FormInput", "Type", true);
        theRecord.AddRecord(dr_limit_type);

        DataRecordShortIntField dr_limit_val = new DataRecordShortIntField(true, 9, 9);
        dr_limit_val.Initialize("Value [0 for unlimited]", "adm_table_header_nbg", "FormInput", "Value", true);
        theRecord.AddRecord(dr_limit_val);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(nGroupID.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_parent_limit_id = new DataRecordShortIntField(false, 9, 9);
        dr_parent_limit_id.Initialize("parent_limit_module_id", "adm_table_header_nbg", "FormInput", "parent_limit_module_id", false);
        dr_parent_limit_id.SetValue(parentLimitModuleID.ToString());
        theRecord.AddRecord(dr_parent_limit_id);

        string sTable = theRecord.GetTableHTML("adm_limitation_modules_new.aspx?submited=1");

        return sTable;
    }

    public string initDualObj()
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = null;
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

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("CONNECTION_STRING");
            selectQuery += "select gdf.device_family_id, ldf.Name from groups_device_families gdf with (nolock) ";
            selectQuery += "inner join lu_DeviceFamily ldf with (nolock) on ldf.id=gdf.device_family_id ";
            selectQuery += "where is_active=1 and [status]=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gdf.group_id", "=", nLogedInGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gdf.limit_module_id", "=", Int32.Parse(Session["parent_limit_id"].ToString()));
            List<int> limitModulesIDs = null;
            if (Session["limit_id"] != null && Session["limit_id"].ToString().Length > 0)
            {
                limitID = Int32.Parse(Session["limit_id"].ToString());
                limitModulesIDs = BuildLimitationDeviceFamilies(limitID, nLogedInGroupID);
            }

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = selectQuery.Table("query").DefaultView[i].Row["device_family_id"].ToString();
                    string sTitle = string.Empty;
                    if (selectQuery.Table("query").DefaultView[i].Row["NAME"] != null &&
                        selectQuery.Table("query").DefaultView[i].Row["NAME"] != DBNull.Value)
                        sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                    string sDescription = string.Empty;
                    if (limitModulesIDs == null || (limitModulesIDs != null && !limitModulesIDs.Contains(int.Parse(sID))))
                    {
                        sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTitle, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "\" inList=\"false\" />";
                    }
                } // end for

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
            selectQuery += "select dflm.device_family_id, ldf.Name from device_families_limitation_modules dflm with (nolock) ";
            selectQuery += "inner join lu_DeviceFamily ldf with (nolock) on ldf.ID=dflm.device_family_id ";
            selectQuery += " where dflm.is_active=1 and dflm.[status]=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dflm.group_id", "=", groupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dflm.device_limitation_module_id", "=", limitID);
            selectQuery += " order by dflm.device_family_id asc";
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

            if (Session["parent_limit_id"] != null && Session["parent_limit_id"].ToString().Length > 0)
            {
                int limitID = 0;
                int.TryParse(Session["parent_limit_id"].ToString(), out limitID);

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
}
