using System;
using TVinciShared;

public partial class adm_media_finacial_contracts : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

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
        Response.Write(sRet + " : Financial contracts ");
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

    protected void InsertMediaFilesContractFamilyID(Int32 nContractFamilyID, Int32 nMEdiaFileID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("fr_media_files_contract_families");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMEdiaFileID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nContractFamilyID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nGroupID;
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateMediaFilesContractFamilyID(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("fr_media_files_contract_families");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetMediaFilesContractFamilyID(Int32 nContractFamilyID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,status from fr_media_files_contract_families where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", int.Parse(Session["media_file_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nContractFamilyID);
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
        Int32 nStatus = 0;
        Int32 nMediaFilesContractFamilyID = GetMediaFilesContractFamilyID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nMediaFilesContractFamilyID != 0)
        {
            if (nStatus == 0)
                UpdateMediaFilesContractFamilyID(nMediaFilesContractFamilyID, 1);
            else
                UpdateMediaFilesContractFamilyID(nMediaFilesContractFamilyID, 0);
        }
        else
        {
            InsertMediaFilesContractFamilyID(int.Parse(sID), int.Parse(Session["media_file_id"].ToString()), nLogedInGroupID);
        }

        return "";
    }

    protected bool IsContractFamilyBelongToMediaFile(Int32 nMediaFileID, Int32 nFinancContractEntityID)
    {
        bool bRet = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from fr_media_files_contract_families where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nFinancContractEntityID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                bRet = true;
        }
        selectQuery.Finish();
        selectQuery = null;
        return bRet;
    }

    public void GetMeidaID()
    {
        Response.Write(Session["media_id"].ToString());
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

        string sRet = "";
        sRet += "Current Financial Contracts Families";
        sRet += "~~|~~";
        sRet += "Available Financial Contracts Families";
        sRet += "~~|~~";

        string sIP = "1.1.1.1";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from fr_financial_entities where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        selectQuery += " and PARENT_ENTITY_ID<>0";
        selectQuery += "order by PARENT_ENTITY_ID";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            sRet += "<root>";
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                Int32 nParentID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["PARENT_ENTITY_ID"].ToString());
                string sParentName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "NAME", nParentID).ToString();
                string sTitle = sParentName + " - " + selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                string sDescription = selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();
                if (IsContractFamilyBelongToMediaFile(int.Parse(Session["media_file_id"].ToString()) , int.Parse(sID)) == true)
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sDescription + "\" inList=\"true\" />";
                else
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sDescription + "\" inList=\"false\" />";
            }
            sRet += "</root>";
        }
        selectQuery.Finish();
        selectQuery = null;
        
        return sRet;
    }
}
