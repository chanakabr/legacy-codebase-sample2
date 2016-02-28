using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using System.Globalization;
using KLogMonitor;
using System.Reflection;
using TvinciImporter;
using System.Data;

public partial class adm_media_files_ppvmodules : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_media.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["media_file_id"] != null &&
                Request.QueryString["media_file_id"].ToString() != "")
            {
                Session["media_file_id"] = int.Parse(Request.QueryString["media_file_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "group_id", int.Parse(Session["media_file_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["media_file_id"] == null || Session["media_file_id"].ToString() == "" || Session["media_file_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ":";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("CONNECTION_STRING");
        selectQuery += "select m.name,lmq.description as 'mq_desc',lmt.description as 'lmt_desc' from lu_media_types lmt,lu_media_quality lmq,media m,media_files mf where lmq.id=mf.MEDIA_QUALITY_ID and lmt.id=mf.MEDIA_TYPE_ID and mf.media_id=m.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", int.Parse(Session["media_file_id"].ToString()));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sRet += selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                sRet += "(";
                sRet += selectQuery.Table("query").DefaultView[0].Row["mq_desc"].ToString();
                sRet += " - ";
                sRet += selectQuery.Table("query").DefaultView[0].Row["lmt_desc"].ToString();
                sRet += ")";
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        Response.Write(sRet + " : Pay Per View Modules ");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
    }

    public string GetIPAddress()
    {
        string strHostName = System.Net.Dns.GetHostName();
        System.Net.IPHostEntry ipHostInfo = System.Net.Dns.Resolve(System.Net.Dns.GetHostName());
        System.Net.IPAddress ipAddress = ipHostInfo.AddressList[0];

        return ipAddress.ToString();
    }

    private string GetMainLang(int nGroupID)
    {
        string sMainLang = string.Empty;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select code3 from lu_languages where id in (select LANGUAGE_ID from groups where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGroupID);
        selectQuery += ")";
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "code3", 0);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return sMainLang;
    }

    protected void InsertPPVModulesMediaFilesID(Int32 nPPVModuleID, Int32 nMEdiaFileID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_modules_media_files");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMEdiaFileID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", nPPVModuleID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nGroupID;
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdatePPVModulesMediaFilesID(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_modules_media_files");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected bool UpdatePPVModulesMediaFilesIDDates(Int32 nID, string sStartDate, string sEndDate)
    {
        bool res = false;
        try
        {
            DateTime? dStartDate = string.IsNullOrEmpty(sStartDate) ? null : (DateTime?)(DateTime.ParseExact(sStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
            DateTime? dEndDate = string.IsNullOrEmpty(sEndDate) ? null : (DateTime?)(DateTime.ParseExact(sEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_modules_media_files");
            updateQuery.SetConnectionKey("pricing_connection");
            if (dStartDate.HasValue)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "=", (DateTime)dStartDate);
            }
            else
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "=", DBNull.Value);
            }

            if (dEndDate.HasValue)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", (DateTime)dEndDate);
            }
            else
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "=", DBNull.Value);
            }

            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
            res = updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("Error occurred while trying to execute UpdatePPVModulesMediaFilesIDDates", ex);
        }

        return res;
    }

    protected Int32 GetPPVModulesMediaFilesID(Int32 nPPVModuleID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select id,status from ppv_modules_media_files where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", int.Parse(Session["media_file_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", nPPVModuleID);
        selectQuery += "and";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["STATUS"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    public string changeItemStatus(string sID, string sAction)
    {
        int mediaFileID;
        if (Session["media_file_id"] == null || !int.TryParse(Session["media_file_id"].ToString(), out mediaFileID) || mediaFileID == 0)        
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "group_id", mediaFileID).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nPPVModuleMediaFilesID = GetPPVModulesMediaFilesID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nPPVModuleMediaFilesID != 0)
        {
            if (nStatus == 0)
                UpdatePPVModulesMediaFilesID(nPPVModuleMediaFilesID, 1);
            else
                UpdatePPVModulesMediaFilesID(nPPVModuleMediaFilesID, 0);
        }
        else
        {
            InsertPPVModulesMediaFilesID(int.Parse(sID), mediaFileID, nLogedInGroupID);
        }

        //Update free file index                        
        int mediaID;
        if (int.TryParse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "media_id", mediaFileID).ToString(), out mediaID))
        {
            if (!ImporterImpl.UpdateIndex(new List<int>() { mediaID }, nLogedInGroupID, ApiObjects.eAction.Update))
            {
                log.Error(string.Format("Failed updating index for mediaID: {0}, groupID: {1}", mediaID, nLogedInGroupID));
            }
        }

        try
        {
            Notifiers.BaseMediaNotifier t = null;
            Notifiers.Utils.GetBaseMediaNotifierImpl(ref t, LoginManager.GetLoginGroupID());
            if (t != null)
                t.NotifyChange(Session["media_id"].ToString());
            return "";
        }
        catch (Exception ex)
        {
            log.Error("exception - " + Session["media_id"].ToString() + " : " + ex.Message, ex);
        }

        return "";
    }

    public string changeItemDates(string sID, string sStartDate, string sEndDate)
    {
        int mediaFileID;
        if (Session["media_file_id"] == null || !int.TryParse(Session["media_file_id"].ToString(), out mediaFileID) || mediaFileID == 0)
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }
        
        DataRow mediaFileDetails = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("media_files", "id", mediaFileID.ToString(), new List<string>() { "group_id", "media_id" });        
        int mediaID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("media_files", "media_id", mediaFileID).ToString());
        int nOwnerGroupID = ODBCWrapper.Utils.GetIntSafeVal(mediaFileDetails, "group_id");
        DataRow ppvModuleMediaFileDetails = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("ppv_modules_media_files", "media_file_id", mediaFileID.ToString(), new List<string>() { "group_id", "start_date", "end_date" }, "pricing_connection");
        DateTime? prevStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(ppvModuleMediaFileDetails, "start_date");
        DateTime? prevEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(ppvModuleMediaFileDetails, "end_date");
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nPPVModuleMediaFilesID = GetPPVModulesMediaFilesID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nPPVModuleMediaFilesID != 0)
        {
            if (UpdatePPVModulesMediaFilesIDDates(nPPVModuleMediaFilesID, sStartDate, sEndDate))
            {
                DataRow updatedppvModuleMediaFileDetails = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("ppv_modules_media_files", "media_file_id", mediaFileID.ToString(), new List<string>() { "group_id", "start_date", "end_date" }, "pricing_connection");
                DateTime? updatedStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedppvModuleMediaFileDetails, "start_date");
                DateTime? updatedEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedppvModuleMediaFileDetails, "end_date");
                // check if changes in the start date require future index update call, incase updatedStartDate is in more than 2 years we don't update the index (per Ira's request)
                if (updatedStartDate.HasValue && (updatedStartDate.Value > DateTime.UtcNow && updatedStartDate.Value <= DateTime.UtcNow.AddYears(2))
                    && (!prevStartDate.HasValue || updatedStartDate.Value != prevStartDate.Value))
                {
                    if (!ImporterImpl.InsertFreeItemsIndexUpdate(nLogedInGroupID, ApiObjects.eObjectType.Media, new List<int>() { mediaID }, updatedStartDate.Value))
                    {
                        log.Error(string.Format("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", updatedStartDate.Value, mediaID, nLogedInGroupID));
                    }
                }

                // check if changes in the end date require future index update call, incase updatedEndDate is in more than 2 years we don't update the index (per Ira's request)
                if (updatedEndDate.HasValue && (updatedEndDate > DateTime.UtcNow && updatedEndDate.Value <= DateTime.UtcNow.AddYears(2))
                    && (!prevEndDate.HasValue || updatedEndDate.Value != prevEndDate.Value))
                {
                    if (!ImporterImpl.InsertFreeItemsIndexUpdate(nLogedInGroupID, ApiObjects.eObjectType.Media, new List<int>() { mediaID }, updatedEndDate.Value))
                    {
                        log.Error(string.Format("Failed inserting free items index update for endDate: {0}, mediaID: {1}, groupID: {2}", updatedEndDate.Value, mediaID, nLogedInGroupID));
                    }
                }
            }
        }
        return "";
    }

    public string initDualObj()
    {
        if (Session["media_file_id"] == null || Session["media_file_id"].ToString() == "" || Session["media_file_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "group_id", int.Parse(Session["media_file_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Dictionary<string, object> DualListPPVM = new Dictionary<string, object>();
        DualListPPVM.Add("FirstListTitle", "Current Pay Per View Modules");
        DualListPPVM.Add("SecondListTitle", "Available Pay Per View Modules");
        string sWSUserName = "";
        string sWSPass = "";
        string sIP = "1.1.1.1";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        TVinciShared.WS_Utils.GetWSUNPass(nCommerceGroupID, "GetPPVModuleListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
        log.Debug("Pricing WS - User is : " + sWSUserName + " Pass is : " + sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = GetWSURL();
        if (sWSURL != "")
            m.Url = sWSURL;
        TvinciPricing.PPVModuleContainer[] oModules = m.GetPPVModuleListForAdmin(sWSUserName, sWSPass, int.Parse(Session["media_file_id"].ToString()), string.Empty, string.Empty, string.Empty);
        Int32 nCount = oModules.Length;
        log.Debug("Pricing WS - Count is " + nCount.ToString());
        object[] resultData = new object[nCount];
        string mainLanguage = GetMainLang(nCommerceGroupID);
        for (int i = 0; i < nCount; i++)
        {
            string sID = oModules[i].m_oPPVModule.m_sObjectCode;
            string sTitle = (!string.IsNullOrEmpty(oModules[i].m_oPPVModule.m_sObjectVirtualName)) ? oModules[i].m_oPPVModule.m_sObjectVirtualName : sID;
            bool inList = oModules[i].m_bIsBelong;
            string startDate = (oModules[i].m_dStartDate == null) ? "" : oModules[i].m_dStartDate.Value.ToString("dd/MM/yyyy");
            string endDate = (oModules[i].m_dEndDate == null) ? "" : oModules[i].m_dEndDate.Value.ToString("dd/MM/yyyy");
            string sDescription = "";
            if (oModules[i].m_oPPVModule.m_sDescription != null)
            {
                for (int j = 0; j < oModules[i].m_oPPVModule.m_sDescription.Length; j++)
                {
                    if (oModules[i].m_oPPVModule.m_sDescription[j].m_sLanguageCode3 == mainLanguage)
                    {
                        sDescription += oModules[i].m_oPPVModule.m_sDescription[j].m_sValue;
                    }

                }
            }
            var data = new
            {
                ID = sID,
                Title = sTitle,
                Description = sDescription,
                InList = inList,
                StartDate = startDate,
                EndDate = endDate
            };
            resultData[i] = data;
        }

        DualListPPVM.Add("Data", resultData);
        DualListPPVM.Add("pageName", "adm_media_files_ppvmodules.aspx");
        DualListPPVM.Add("withCalendar", true);
        log.Debug("Pricing WS - " + resultData.ToJSON());
        return DualListPPVM.ToJSON();
    }

    public void GetMeidaID()
    {
        Response.Write(Session["media_id"].ToString());
    }
}
