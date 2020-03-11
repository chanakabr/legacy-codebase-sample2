using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Text;
using System.IO;
using System.Net;

public partial class adm_fr_reports : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_fr_reports.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_fr_reports.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + " Financial Reports");
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sGroupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "group_name", nGroupID, "CONNECTION_STRING").ToString();
        
        theTable += "select fr.id, fr.start_date as 'From', fr.end_date as 'To', fr.file_name as 'Filename', fr.create_date as 'Create Date',";
        theTable += " isNull(fe.NAME, '" + sGroupName + "') as 'Right Holder Name',";
        theTable += " 'Report Type' = CASE report_type WHEN 1 THEN 'Financial' WHEN 2 THEN 'BreakDown' WHEN 4 THEN 'Zip' ELSE '-' END";
        theTable += "from fr_reports as fr left outer join fr_financial_entities as fe on rh_entity_id=fe.id where ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("fr.group_id", "=", nGroupID);
        theTable.SetConnectionKey("CONNECTION_STRING");
        if (Session["RightHolder"] != null)
        {
            Int32 nRHEntityID = int.Parse(Session["RightHolder"].ToString());
            if (nRHEntityID > 0)
            {
                theTable += " and ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("fr.RH_ENTITY_ID", "=", nRHEntityID);
            }
        }

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("Filename");

        DataTableLinkColumn linkColumn = new DataTableLinkColumn("javascript:download_report", "Download File", "");
        linkColumn.AddQueryStringValue("FileName", "field=Filename");

        theTable.AddLinkColumn(linkColumn);

    }
    /*Download Report  from FTP server to local, deal with 2 diffrent files
     XML and ZIP */
    public string DownloadReport(string fileName)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        string sFTPUN = string.Empty;
        string sFTPPass = string.Empty;
        string sFTP = string.Empty;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select pics_ftp_username, pics_ftp_password, reports_ftp from groups where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oFTPUN = selectQuery.Table("query").DefaultView[0].Row["pics_ftp_username"];
                object oFTPPass = selectQuery.Table("query").DefaultView[0].Row["pics_ftp_password"];
                object oFTP = selectQuery.Table("query").DefaultView[0].Row["reports_ftp"];

                if (oFTPUN != null && oFTPUN != DBNull.Value)
                    sFTPUN = oFTPUN.ToString();
                if (oFTPPass != null && oFTPPass != DBNull.Value)
                    sFTPPass = oFTPPass.ToString();
                if (oFTP != null && oFTP != DBNull.Value)
                    sFTP = oFTP.ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        string filePath = System.IO.Path.Combine(sFTP, fileName);

        string sContentType = "";

        //deal with download a zip file (include 2 xml report files)
        if (fileName.Contains("zip"))
        {
            sContentType = "octet-stream";

            byte[] buf = null;
            using (WebClient client = new WebClient())
            {
                if (sFTP.StartsWith("ftp://"))
                {
                    sFTP = sFTP.Substring(6);
                }

                sFTP = string.Format("ftp://{0}:{1}@{2}", sFTPUN, sFTPPass.Replace("@", "%40"), sFTP);

                filePath = System.IO.Path.Combine(sFTP, fileName);

                buf = client.DownloadData(filePath);
            }


            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.ContentType = string.Format("application/{0}", sContentType);

            HttpContext.Current.Response.BinaryWrite(buf);
            HttpContext.Current.Response.End();
        }
        //deal with xml report file 
        else
        {

            sContentType = "text/xml";

            if (filePath.Contains("xls"))
            {
                sContentType = "vnd.ms-excel";
            }

            FTPUploader t = new FTPUploader(filePath, sFTP, sFTPUN, sFTPPass);
            string res = t.Download();

            StringBuilder sRet = new StringBuilder();

            sRet.Append(res);

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.ContentType = string.Format("application/{0}", sContentType);

            using (StringWriter sw = new StringWriter(sRet))
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    //  render the htmlwriter into the response
                    HttpContext.Current.Response.Write(sw.ToString());
                    HttpContext.Current.Response.End();
                }
            }
        }

        return "Report Downloaded";
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_fr_reports.aspx";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}