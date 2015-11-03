using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Data;
using System.Text;
using System.IO;
using System.Data.OleDb;
using KLogMonitor;
using System.Reflection;

public partial class adm_tvp_translations_batch : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        Int32 nMenuID = 0;
        m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

        if (Request.QueryString["platform"] != null &&
               Request.QueryString["platform"].ToString() != "")
        {
            Session["platform"] = Request.QueryString["platform"].ToString();
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Translations");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetExcel(object sender, EventArgs e)
    {
        int nGroupID = LoginManager.GetLoginGroupID();

        DataTable resultTable = new DataTable("resultTable");

        KeyValuePair<int, string[]>[] sLangs = GetLangs(nGroupID);

        StringBuilder sb = new StringBuilder();

        foreach (KeyValuePair<int, string[]> kvp in sLangs)
        {
            string sLang = kvp.Value[0];

            sb.AppendFormat(", isnull((select OriginalText from TranslationMetadata where STATUS=1 and LANGUAGE_ID={0} and TranslationID=t.ID), '') as '{1}'",
                kvp.Key.ToString(), sLang);
        }

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select t.ID as 'TOKEN_ID' , t.TitleID as 'TOKEN' " + sb.ToString() + " from Translation t where t.STATUS=1";
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;

            resultTable = selectQuery.Table("query");
        }
        selectQuery.Finish();
        selectQuery = null;


        GetExcelTable(resultTable);
    }

    private DataSet GetExcelWorkSheet(string pathName, string fileName, int workSheetNumber)
    {
        try
        {
            OleDbConnection ExcelConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + @"\" + fileName + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"");
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
            log.Error("Excel Feeder Error - Error opening Excel file " + ex.Message, ex);
            return null;
        }
    }

    private void GetExcelTable(DataTable resultTable)
    {
        string style = @"<style> td { mso-number-format:\@; text-align:left; } </style> ";

        GridView gv = new GridView();

        gv.DataSource = resultTable;
        gv.DataBind();

        //gv.HeaderRow.BackColor = System.Drawing.ColorTranslator.FromHtml("#000941");
        //gv.HeaderRow.ForeColor = System.Drawing.Color.White;
        gv.HeaderRow.Font.Bold = true;

        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=Translations.xls");
        HttpContext.Current.Response.Charset = "UTF-8";
        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        Response.Write("\uFEFF");

        System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        gv.RenderControl(htmlWrite);
        HttpContext.Current.Response.Write(style);
        HttpContext.Current.Response.Write(stringWrite.ToString());
        HttpContext.Current.Response.End();
    }

    protected void UploadExcel(object sender, EventArgs e)
    {
        int nGroupID = LoginManager.GetLoginGroupID(); ;

        if (FileUpload1.HasFile)
        {
            // Get the name of the file to upload.
            String fileName = FileUpload1.FileName;

            // Allow only files with .xls extensions to be uploaded.
            if (System.IO.Path.HasExtension(fileName) && (System.IO.Path.GetExtension(fileName) == ".xls" || System.IO.Path.GetExtension(fileName) == ".xlsx"))
            {

                String path = Server.MapPath(string.Empty);
                path = System.IO.Path.Combine(path, "translation_upload");
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

                ImportExcel(nGroupID, path, uniqueFileName);
            }
            else
            {
                /*
                LblUploadStatus.ForeColor = System.Drawing.Color.Red;
                LblUploadStatus.Text = "File must be Excel type !";
                LblUploadStatus.Visible = true;
                */
            }

        }
        else
        {
            /*
            LblUploadStatus.ForeColor = System.Drawing.Color.Red;
            LblUploadStatus.Text = "No File To Upload";
            LblUploadStatus.Visible = true;
            */
        }
    }

    protected void ImportExcel(int nGroupID, string path, string fileName)
    {
        KeyValuePair<int, string[]>[] sLangs = GetLangs(nGroupID);

        //FileUpload1.Visible = false;
        //ButtonUpload.Visible = false;

        DataSet ds = GetExcelWorkSheet(path, fileName, 0).Copy();

        int nCount = ds.Tables[0].DefaultView.Count;
        DataTable dt = ds.Tables[0].Copy();

        int nColCount = dt.Columns.Count;

        for (int i = 0; i < nCount; i++)
        {
            int nTrnslateID = int.Parse(dt.DefaultView[i].Row[0].ToString());

            foreach (KeyValuePair<int, string[]> kvp in sLangs)
            {
                string sLang = kvp.Value[0];
                string sCulture = kvp.Value[1];

                if (dt.Columns.Contains(sLang))
                {
                    string val = dt.DefaultView[i].Row[sLang].ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        HandleTranslations(nGroupID, nTrnslateID, kvp.Key, val, sCulture);
                    }
                }
            }
        }
    }


    private void HandleTranslations(int nGroupID, int nTokenID, int nLangID, string newVal, string sCulture)
    {
        int nTID = 0;
        string oldVal = string.Empty;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id, OriginalText from TranslationMetadata where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TranslationID", "=", nTokenID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_ID", "=", nLangID);
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nTID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                oldVal = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "OriginalText", 0);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        if (nTID > 0)
        {
            if (oldVal != newVal)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("TranslationMetadata");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Text", "=", newVal);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OriginalText", "=", newVal);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTID);
                updateQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }
        else
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("TranslationMetadata");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TranslationID", "=", nTokenID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("language_ID", "=", nLangID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Text", "=", newVal);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Culture", "=", sCulture);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OriginalText", "=", newVal);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);

            insertQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }
    }

    private KeyValuePair<int, string[]>[] GetLangs(int nGroupID)
    {
        List<KeyValuePair<int, string[]>> sLangs = new List<KeyValuePair<int, string[]>>();

        int nLangID = 0;
        string sLang = string.Empty;
        string sCultore = string.Empty;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id, name, culture from lu_languages where status = 1 ";
        selectQuery += "order by isDefault desc";
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                nLangID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                sLang = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "name", i);
                sCultore = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "culture", i);

                string[] arr = new string[2];
                arr[0] = sLang;
                arr[1] = sCultore;

                KeyValuePair<int, string[]> kvp = new KeyValuePair<int, string[]>(nLangID, arr);
                sLangs.Add(kvp);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return sLangs.ToArray<KeyValuePair<int, string[]>>();
    }
}