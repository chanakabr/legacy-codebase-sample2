using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using TvinciImporter;
using System.Data;
using KLogMonitor;
using System.Reflection;
using TvinciPricing;
using apiWS;

public partial class adm_alcr_ppvs_and_file_types : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_asset_life_cycle_rules.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_asset_life_cycle_rules.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            int ruleId = 0;
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1" && Session["rule_id"] != null
                && !string.IsNullOrEmpty(Session["rule_id"].ToString()) && int.TryParse(Session["rule_id"].ToString(), out ruleId) && ruleId >= 0
                && Session["ppvsAndFileTypesToAdd"] != null && Session["ppvsAndFileTypesToRemove"] != null)
            {                
                LifeCycleFileTypesAndPpvsTransitions ppvsAndFileTypesToAdd = Session["ppvsAndFileTypesToAdd"] as LifeCycleFileTypesAndPpvsTransitions;
                LifeCycleFileTypesAndPpvsTransitions ppvsAndFileTypesToRemove = Session["ppvsAndFileTypesToRemove"] as LifeCycleFileTypesAndPpvsTransitions;
                if (ppvsAndFileTypesToAdd == null || ppvsAndFileTypesToRemove == null)
                {
                    log.ErrorFormat("Failed to get ppvsAndFileTypesToAdd or ppvsAndFileTypesToRemove for rule_id: {0}", ruleId);
                    HttpContext.Current.Session["error_msg"] = "incorrect values while updating rule ppvs and file types";                    
                    return;
                }

                FriendlyAssetLifeCycleRule friendlyAssetLifeCycleRule = new FriendlyAssetLifeCycleRule()
                {
                    Id = ruleId,
                    Actions = new LifeCycleTransitions() { FileTypesAndPpvsToAdd = ppvsAndFileTypesToAdd, FileTypesAndPpvsToRemove = ppvsAndFileTypesToRemove }
                };

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";

                TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "Asset", "api", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");                
                if (!string.IsNullOrEmpty(sWSURL) && !string.IsNullOrEmpty(sWSUserName) && !string.IsNullOrEmpty(sWSPass))
                {
                    apiWS.API client = new apiWS.API();
                    client.Url = sWSURL;
                    if (!client.InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes(sWSUserName, sWSPass, friendlyAssetLifeCycleRule))
                    {
                        log.ErrorFormat("Failed to update asset life cycle rule ppvs and file types for rule_id: {0}", ruleId);
                        HttpContext.Current.Session["error_msg"] = "Failed to update asset life cycle rule ppvs and file types";
                        return;
                    }
                    else
                    {
                        Response.Redirect("adm_asset_life_cycle_rules.aspx");
                    }
                }
                
                return;
            }
            if (Request.QueryString["rule_id"] != null && Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = Request.QueryString["rule_id"].ToString();
                Session["ppvsAndFileTypesToAdd"] = null;
                Session["ppvsAndFileTypesToRemove"] = null;
            }
            else if (Session["rule_id"] == null || string.IsNullOrEmpty(Session["rule_id"].ToString()) || !int.TryParse(Session["rule_id"].ToString(), out ruleId) || ruleId <= 0)
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Asset Scheduling Rules - PPVs And File Types");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }    

    public string initDualObj()
    {
        int ruleId = 0;
        if (Session["rule_id"] == null || string.IsNullOrEmpty(Session["rule_id"].ToString()) || !int.TryParse(Session["rule_id"].ToString(), out ruleId) || ruleId <= 0)
        {            
            return "";
        }

        Dictionary<string, object> dualLists = new Dictionary<string, object>();
        Dictionary<string, object> ppvsToAdd = new Dictionary<string, object>();
        Dictionary<string, object> ppvsToRemove = new Dictionary<string, object>();
        Dictionary<string, object> fileTypesToAdd = new Dictionary<string, object>();
        Dictionary<string, object> fileTypesToRemove = new Dictionary<string, object>();
        Session["ppvsAndFileTypesToAdd"] = null;
        Session["ppvsAndFileTypesToRemove"] = null;

        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";

        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "Asset", "api", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");
        FriendlyAssetLifeCycleRule friendlyAssetLifeCycleRule = null;
        if (!string.IsNullOrEmpty(sWSURL) && !string.IsNullOrEmpty(sWSUserName) && !string.IsNullOrEmpty(sWSPass))
        {
            apiWS.API client = new apiWS.API();
            client.Url = sWSURL;
            FriendlyAssetLifeCycleRuleResponse res = client.GetFriendlyAssetLifeCycleRule(sWSUserName, sWSPass, ruleId);
            if (res != null && res.Status != null && res.Status.Code == 0 && res.Rule != null)
            {
                friendlyAssetLifeCycleRule = res.Rule;
            }
            else
            {
                Session["error_msg"] = "Failed to get asset life cycle rule";
                return "";
            }
        }

        if (friendlyAssetLifeCycleRule == null || friendlyAssetLifeCycleRule.Actions == null || friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToAdd == null
            || friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToRemove == null)
        {            
            return "";
        }

        Session["ppvsAndFileTypesToAdd"] = friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToAdd;
        Session["ppvsAndFileTypesToRemove"] = friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToRemove;

        PPVModule[] availablePpvModules = GetAllPpvModules();
        if (availablePpvModules == null)
        {
            return "";
        }
        
        DataTable availableFileTypes = GetAllFileTypes();
        if (availableFileTypes == null || availableFileTypes.Rows == null)
        {
            return "";
        }

        ppvsToAdd.Add("name", "DualListppvsToAdd");
        ppvsToAdd.Add("FirstListTitle", "PPVs To Add");
        ppvsToAdd.Add("SecondListTitle", "Available PPVs");
        ppvsToAdd.Add("pageName", "adm_alcr_ppvs_and_file_types.aspx");
        ppvsToAdd.Add("withCalendar", false);
        object[] ppvsToAddData = null;
        initPpvs(ruleId, friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToAdd.PpvIds.ToList(), availablePpvModules, ref ppvsToAddData);
        ppvsToAdd.Add("Data", ppvsToAddData);

        fileTypesToAdd.Add("name", "DualListfileTypesToAdd");
        fileTypesToAdd.Add("FirstListTitle", "File Types To Add");
        fileTypesToAdd.Add("SecondListTitle", "Available File Types");
        fileTypesToAdd.Add("pageName", "adm_alcr_ppvs_and_file_types.aspx");
        fileTypesToAdd.Add("withCalendar", false);
        object[] fileTypesToAddData = null;
        initFileTypes(ruleId, friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToAdd.FileTypeIds.ToList(), availableFileTypes, ref fileTypesToAddData);
        fileTypesToAdd.Add("Data", fileTypesToAddData);

        ppvsToRemove.Add("name", "DualListppvsToRemove");
        ppvsToRemove.Add("FirstListTitle", "PPVs To Remove");
        ppvsToRemove.Add("SecondListTitle", "Existing PPVs");
        ppvsToRemove.Add("pageName", "adm_alcr_ppvs_and_file_types.aspx");
        ppvsToRemove.Add("withCalendar", false);
        object[] ppvsToRemoveData = null;
        initPpvs(ruleId, friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToRemove.PpvIds.ToList(), availablePpvModules, ref ppvsToRemoveData);
        ppvsToRemove.Add("Data", ppvsToRemoveData);

        fileTypesToRemove.Add("name", "DualListfileTypesToRemove");
        fileTypesToRemove.Add("FirstListTitle", "File Types To Remove");
        fileTypesToRemove.Add("SecondListTitle", "Existing File Types");
        fileTypesToRemove.Add("pageName", "adm_alcr_ppvs_and_file_types.aspx");
        fileTypesToRemove.Add("withCalendar", false);
        object[] fileTypesToRemoveData = null;
        initFileTypes(ruleId, friendlyAssetLifeCycleRule.Actions.FileTypesAndPpvsToRemove.FileTypeIds.ToList(), availableFileTypes, ref fileTypesToRemoveData);
        fileTypesToRemove.Add("Data", fileTypesToRemoveData);

        dualLists.Add("0", ppvsToAdd);
        dualLists.Add("1", fileTypesToAdd);
        dualLists.Add("2", ppvsToRemove);
        dualLists.Add("3", fileTypesToRemove);
        dualLists.Add("size", dualLists.Count);

        return dualLists.ToJSON();
    }

    private void initPpvs(int ruleId, List<int> existingRulePpvIds, PPVModule[] availablePpvModules, ref object[] resultData)
    {
        resultData = new object[availablePpvModules.Length];
        for (int i = 0; i < availablePpvModules.Length; i++)
        {
            string sID = availablePpvModules[i].m_sObjectCode;
            string sTitle = (!string.IsNullOrEmpty(availablePpvModules[i].m_sObjectVirtualName)) ? availablePpvModules[i].m_sObjectVirtualName : sID;                        
            var data = new
            {
                ID = sID,
                Title = sTitle,
                Description = sTitle,
                InList = existingRulePpvIds.Contains(int.Parse(sID))
            };

            resultData[i] = data;
        }
    }

    private void initFileTypes(int ruleId, List<int> existingRuleFileTypes, DataTable availableFileTypes, ref object[] resultData)
    {
        resultData = new object[availableFileTypes.Rows.Count];
        for (int i = 0; i < availableFileTypes.Rows.Count; i++)
        {
            DataRow dr = availableFileTypes.Rows[i];
            int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            string sTitle = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");                                    
            var data = new
            {
                ID = id,
                Title = sTitle,
                Description = sTitle,
                InList = existingRuleFileTypes.Contains(id)
            };

            resultData[i] = data; 
        }
    }    

    private PPVModule[] GetAllPpvModules()
    {
        TvinciPricing.PPVModule[] ppvModules = null;
        string sWSUserName = "";
        string sWSPass = "";
        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPPVModuleList", "pricing", sIP, ref sWSUserName, ref sWSPass);        
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
        if (string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass) || string.IsNullOrEmpty(sWSURL))
        {
            return ppvModules;
        }

        m.Url = sWSURL;
        ppvModules = m.GetPPVModuleList(sWSUserName, sWSPass, string.Empty, string.Empty, string.Empty);
        return ppvModules;
    }

    private DataTable GetAllFileTypes()
    {
        DataTable dt = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        selectQuery += "select ID, DESCRIPTION from groups_media_type where is_active=1 and status=1 and (is_trailer<>1 or is_trailer is null) and ";
        selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(LoginManager.GetLoginGroupID(), "");
        selectQuery += "and group_id <>" + LoginManager.GetLoginGroupID();
        dt = selectQuery.Execute("query", true);
        selectQuery.Finish();
        selectQuery = null;

        return dt;
    }

    public string changeItemStatus(string sID, string dualListName)
    {
        int itemId = 0;
        if (int.TryParse(sID, out itemId) && itemId > 0 && !string.IsNullOrEmpty(dualListName))
        {
            switch (dualListName)
            {
                case "DualListppvsToAdd":
                    changeItemStatusPpvsToAdd(itemId);
                    break;
                case "DualListfileTypesToAdd":
                    changeItemStatusFileTypesToAdd(itemId);
                    break;
                case "DualListppvsToRemove":
                    changeItemStatusPpvsToRemove(itemId);
                    break;
                case "DualListfileTypesToRemove":
                    changeItemStatusFileTypesToRemove(itemId);
                    break;
                default:
                    break;
            }
        }

        return "";
    }

    private void changeItemStatusPpvsToAdd(int itemId)
    {        
        if (Session["ppvsAndFileTypesToAdd"] != null)
        {
            LifeCycleFileTypesAndPpvsTransitions ppvsAndFileTypesToAdd = Session["ppvsAndFileTypesToAdd"] as LifeCycleFileTypesAndPpvsTransitions;
            if (ppvsAndFileTypesToAdd != null)
            {
                List<int> ppvsToAdd = ppvsAndFileTypesToAdd.PpvIds.ToList();
                if (ppvsToAdd.Contains(itemId))
                {
                    ppvsToAdd.Remove(itemId);
                }
                else
                {
                    ppvsToAdd.Add(itemId);
                }

                ppvsAndFileTypesToAdd.PpvIds = ppvsToAdd.ToArray();
                Session["ppvsAndFileTypesToAdd"] = ppvsAndFileTypesToAdd;
            }
        }
    }

    private void changeItemStatusFileTypesToAdd(int itemId)
    {
        if (Session["ppvsAndFileTypesToAdd"] != null)
        {
            LifeCycleFileTypesAndPpvsTransitions ppvsAndFileTypesToAdd = Session["ppvsAndFileTypesToAdd"] as LifeCycleFileTypesAndPpvsTransitions;
            if (ppvsAndFileTypesToAdd != null)
            {
                List<int> fileTypesToAdd = ppvsAndFileTypesToAdd.FileTypeIds.ToList();
                if (fileTypesToAdd.Contains(itemId))
                {
                    fileTypesToAdd.Remove(itemId);
                }
                else
                {
                    fileTypesToAdd.Add(itemId);
                }

                ppvsAndFileTypesToAdd.FileTypeIds = fileTypesToAdd.ToArray();
                Session["ppvsAndFileTypesToAdd"] = ppvsAndFileTypesToAdd;
            }
        }
    }

    private void changeItemStatusPpvsToRemove(int itemId)
    {
        if (Session["ppvsAndFileTypesToRemove"] != null)
        {
            LifeCycleFileTypesAndPpvsTransitions ppvsAndFileTypesToRemove = Session["ppvsAndFileTypesToRemove"] as LifeCycleFileTypesAndPpvsTransitions;
            if (ppvsAndFileTypesToRemove != null)
            {
                List<int> ppvsToAdd = ppvsAndFileTypesToRemove.PpvIds.ToList();
                if (ppvsToAdd.Contains(itemId))
                {
                    ppvsToAdd.Remove(itemId);
                }
                else
                {
                    ppvsToAdd.Add(itemId);
                }

                ppvsAndFileTypesToRemove.PpvIds = ppvsToAdd.ToArray();
                Session["ppvsAndFileTypesToRemove"] = ppvsAndFileTypesToRemove;
            }
        }     
    }

    private void changeItemStatusFileTypesToRemove(int itemId)
    {
        if (Session["ppvsAndFileTypesToRemove"] != null)
        {
            LifeCycleFileTypesAndPpvsTransitions ppvsAndFileTypesToRemove = Session["ppvsAndFileTypesToRemove"] as LifeCycleFileTypesAndPpvsTransitions;
            if (ppvsAndFileTypesToRemove != null)
            {
                List<int> fileTypesToAdd = ppvsAndFileTypesToRemove.FileTypeIds.ToList();
                if (fileTypesToAdd.Contains(itemId))
                {
                    fileTypesToAdd.Remove(itemId);
                }
                else
                {
                    fileTypesToAdd.Add(itemId);
                }

                ppvsAndFileTypesToRemove.FileTypeIds = fileTypesToAdd.ToArray();
                Session["ppvsAndFileTypesToRemove"] = ppvsAndFileTypesToRemove;
            }
        }
    }

    private void EndOfAction()
    {
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["success_back_page"] != null)
            HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
        else
            HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");

        //if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
        //{
        //    // string sFailure = coll["failure_back_page"].ToString();
        //    if (coll["failure_back_page"] != null)
        //        HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["failure_back_page"].ToString() + "';</script>");
        //    else
        //        HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
        //}
        //else
        //{
        //    if (HttpContext.Current.Request.QueryString["back_n_next"] != null)
        //    {
        //        HttpContext.Current.Session["last_page_html"] = null;
        //        string s = HttpContext.Current.Session["back_n_next"].ToString();
        //        HttpContext.Current.Response.Write("<script>window.document.location.href='" + s.ToString() + "';</script>");
        //        HttpContext.Current.Session["back_n_next"] = null;
        //    }
        //    else
        //    {
        //        if (coll["success_back_page"] != null)
        //            HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
        //        else
        //            HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
        //    }
        //}
    }
    
}