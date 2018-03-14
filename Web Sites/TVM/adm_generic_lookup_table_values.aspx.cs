using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_generic_lookup_table_values : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected const int ROW_COUNT = 1000;

    static Dictionary<string, Thread> ThreadDict = new Dictionary<string, Thread>();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        Int32 nMenuID = 0;
        m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

        Int32 nGroupID = LoginManager.GetLoginGroupID();
        int nRet = 0;
        if (Request.QueryString["lookup_id"] != null && Request.QueryString["lookup_id"].ToString() != "")
        {
            Session["lookup_id"] = int.Parse(Request.QueryString["lookup_id"].ToString());



        }
        else
            Session["lookup_id"] = 0;


        string key = string.Format("{0}_{1}", nGroupID, Session["lookup_id"]);
        if (!ThreadDict.ContainsKey(key))
        {
            int nRet1 = 0;
            string ld = string.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as 'rowcount', MAX(create_date) as 'lastdate' from lu_generic where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("lookup_type", "=", Session["lookup_id"]);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet1 = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "rowcount", 0);
                    ld = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "lastdate", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            FileUpload1.Visible = true;
            ButtonUpload.Visible = true;
            LblUploadStatus.Visible = true;
            LblUploadStatus.Text = string.Format("Total rows : {0}, Last update date : {1}", nRet1, ld);
        }
        else
        {
            LblUploadStatus.Visible = true;
            FileUpload1.Visible = false;
            ButtonUpload.Visible = false;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Lookup table");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void UploadExcel(object sender, EventArgs e)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        if (FileUpload1.HasFile)
        {
            try
            {

                // Get the name of the file to upload.
                String fileName = FileUpload1.FileName;

                // Allow only files with .xls extensions to be uploaded.
                if (System.IO.Path.HasExtension(fileName) && (System.IO.Path.GetExtension(fileName) == ".xls" || System.IO.Path.GetExtension(fileName) == ".xlsx"))
                {
                    String path = Server.MapPath(string.Empty);

                    path = ApplicationConfiguration.LookupGenericUpload.Value;
                    path = System.IO.Path.Combine(path, nGroupID.ToString());

                    // Determine whether the directory exists.
                    if (!Directory.Exists(path))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                    }

                    string[] s = fileName.Split('.');

                    string uniqueFileName = string.Format("{0}_{1}.{2}", s[0], ImageUtils.GetDateImageName(), s[1]);

                    string savePath = System.IO.Path.Combine(path, uniqueFileName);

                    FileUpload1.SaveAs(savePath);

                    string[] vals = new string[4];
                    vals[0] = nGroupID.ToString();
                    vals[1] = savePath;
                    vals[2] = uniqueFileName;

                    if (Session["lookup_id"] != null && Session["lookup_id"].ToString() != "")
                    {
                        vals[3] = Session["lookup_id"].ToString();
                    }

                    ParameterizedThreadStart start = new ParameterizedThreadStart(ImportExcel);

                    Thread t = new Thread(start);
                    string key = string.Format("{0}_{1}", LoginManager.GetLoginGroupID(), Session["lookup_id"]);
                    ThreadDict.Add(key, t);
                    t.Start(vals);
                }
                else
                {
                    LblUploadStatus.ForeColor = System.Drawing.Color.Red;
                    LblUploadStatus.Text = "File must be Excel type !";
                    LblUploadStatus.Visible = true;
                }
            }
            catch (Exception ex)
            {
                LblUploadStatus.ForeColor = System.Drawing.Color.Red;
                LblUploadStatus.Text = "Failed Upload";
                LblUploadStatus.Visible = true;
                log.Error("", ex);
            }
        }
        else
        {
            LblUploadStatus.ForeColor = System.Drawing.Color.Red;
            LblUploadStatus.Text = "No File To Upload";
            LblUploadStatus.Visible = true;
        }
    }

    private void ImportExcel(object val)
    {
        LblUploadStatus.ForeColor = System.Drawing.Color.Black;
        LblUploadStatus.Text = "Processing Excel file...";
        LblUploadStatus.Visible = true;
        FileUpload1.Visible = false;
        ButtonUpload.Visible = false;

        string[] vals = (string[])val;
        int groupID = int.Parse(vals[0].ToString());
        string savePath = vals[1];
        string uniqueFileName = vals[2];
        int lookupType = int.Parse(vals[3].ToString());

        DataSet ds = GetExcelWorkSheet(savePath, uniqueFileName, 0).Copy();

        if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
        {
            bool haveRows = true;
            int startIndex = 0;
            DateTime dateNow = DateTime.UtcNow;
            while (haveRows)
            {
                DataTable dt = CreateDataTable(ds.Tables[0], startIndex, groupID, lookupType, dateNow, out haveRows);
                if (dt != null)
                {
                    bool success = DAL.TvmDAL.insertValueToLookupTable(dt);
                }
                else
                {
                    haveRows = false;
                }
                startIndex += ROW_COUNT;
            }
            lock (ThreadDict)
            {
                string key = string.Format("{0}_{1}", groupID, lookupType);
                ThreadDict.Remove(key);
            }
        }
    }

    private DataTable CreateDataTable(DataTable tempDT, int startIndex, int groupID, int lookupType, DateTime dateNow, out bool moreRows)
    {
        DataTable resultTable = new DataTable("resultTable"); ;
        try
        {
            resultTable.Columns.Add("key", typeof(string));
            resultTable.Columns.Add("Value", typeof(string));
            resultTable.Columns.Add("group_id", typeof(int));
            resultTable.Columns.Add("lookup_type", typeof(int));
            resultTable.Columns.Add("dateNow", typeof(DateTime));

            moreRows = FillTableWithData(resultTable, tempDT, groupID, lookupType, dateNow, startIndex);
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            moreRows = false;
            return null;
        }


        return resultTable;
    }

    private bool FillTableWithData(DataTable resultTable, DataTable tempDT, int groupID, int lookupType, DateTime dateNow, int startIndex)
    {

        bool moreRows = true;
        for (int i = startIndex; i < startIndex + ROW_COUNT; i++)
        {
            if (tempDT != null && tempDT.Rows != null && tempDT.Rows.Count > i)
            {
                DataRow tRow = tempDT.Rows[i];
                DataRow row = resultTable.NewRow();
                if (!string.IsNullOrEmpty(tRow[0].ToString()) && !string.IsNullOrEmpty(tRow[1].ToString()))
                {
                    row["key"] = tRow[0];
                    row["Value"] = tRow[1];
                    row["group_id"] = groupID;
                    row["lookup_type"] = lookupType;
                    row["dateNow"] = dateNow;
                    resultTable.Rows.Add(row);
                }
            }
            else
            {
                moreRows = false;
                break;
            }
        }
        return moreRows;
    }

    private DataSet GetExcelWorkSheet(string pathName, string fileName, int workSheetNumber)
    {
        try
        {
            OleDbConnection ExcelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=1\"");
            OleDbCommand ExcelCommand = new OleDbCommand();
            ExcelCommand.Connection = ExcelConnection;
            OleDbDataAdapter ExcelAdapter = new OleDbDataAdapter(ExcelCommand);

            ExcelConnection.Open();
            DataTable ExcelSheets = ExcelConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            string SpreadSheetName = "[" + ExcelSheets.Rows[workSheetNumber]["TABLE_NAME"].ToString() + "]";

            DataSet ExcelDataSet = new DataSet();
            ExcelCommand.CommandText = @"SELECT * FROM " + SpreadSheetName;
            ExcelAdapter.Fill(ExcelDataSet);

            ExcelConnection.Close();
            return ExcelDataSet;
        }
        catch (Exception ex)
        {
            log.Error("GetExcelWorkSheet Error - Lookup Table - Error opening Excel file " + ex.Message, ex);
            return null;
        }
    }

    private string openCSV(DataTable dt)
    {

        GridView gv = new GridView();

        gv.DataSource = dt;
        gv.DataBind();
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
        HttpContext.Current.Response.Charset = "UTF-8";
        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        gv.RenderControl(htmlWrite);
        HttpContext.Current.Response.Write(stringWrite.ToString());
        HttpContext.Current.Response.End();
        return "";
    }

    protected void ExportExcel(object sender, EventArgs e)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        if (Session["lookup_id"] != null && Session["lookup_id"].ToString() != "")
        {
            try
            {
                DataTable resultTable = new DataTable();

                //Create "Basic" Column for every single value
                DataColumn keyBasic = new DataColumn();
                keyBasic.DataType = System.Type.GetType("System.String");
                keyBasic.ColumnName = "Key";
                resultTable.Columns.Add(keyBasic);

                DataColumn valBasic = new DataColumn();
                valBasic.DataType = System.Type.GetType("System.String");
                valBasic.ColumnName = "Value";
                resultTable.Columns.Add(valBasic);

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select [key], [value] from lu_generic where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("lookup_type", "=", Session["lookup_id"]);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                selectQuery += " order by id ";
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount1; i++)
                    {
                        string key = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "key", i);
                        string val = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "value", i);

                        DataRow row = resultTable.NewRow();
                        row[0] = key;
                        row[1] = val;
                        resultTable.Rows.Add(row);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                string style = @"<style> td { mso-number-format:\@; text-align:left; } </style> ";

                GridView gv = new GridView();

                gv.DataSource = resultTable;
                gv.DataBind();

                Int32 nCount = gv.HeaderRow.Cells.Count;
                string color = string.Empty;

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
                HttpContext.Current.Response.Charset = "UTF-16";
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";

                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

                gv.RenderControl(htmlWrite);
                HttpContext.Current.Response.Write(style);
                HttpContext.Current.Response.Write(stringWrite.ToString());
                HttpContext.Current.Response.End();


            }
            catch (Exception ex)
            {
                log.Error("", ex);
                LblUploadStatus.ForeColor = System.Drawing.Color.Red;
                LblUploadStatus.Text = "Failed To Export Data";
                LblUploadStatus.Visible = true;
            }
        }
        else
        {
            LblUploadStatus.ForeColor = System.Drawing.Color.Red;
            LblUploadStatus.Text = "No File To Upload";
            LblUploadStatus.Visible = true;
        }
    }



}
