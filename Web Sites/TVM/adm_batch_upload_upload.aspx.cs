using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using TVinciShared;

public partial class adm_batch_upload_upload : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    static Dictionary<int, Thread> ThreadDict = new Dictionary<int, Thread>();

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

        Int32 nGroupID = LoginManager.GetLoginGroupID();

        if (!ThreadDict.ContainsKey(nGroupID))
        {
            FileUpload1.Visible = true;
            ButtonUpload.Visible = true;
            LblUploadStatus.Visible = false;
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
        Response.Write(PageUtils.GetPreHeader() + ": Batch Upload - Upload Excel File");
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

                    path = ApplicationConfiguration.BatchUpload.Value;
                    //path = System.IO.Path.Combine(path, "batch_upload");
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

                    ParameterizedThreadStart start = new ParameterizedThreadStart(ImportExcel);

                    Thread t = new Thread(start);

                    ThreadDict.Add(LoginManager.GetLoginGroupID(), t);

                    string[] vals = new string[3];
                    vals[0] = nGroupID.ToString();
                    vals[1] = path;
                    vals[2] = uniqueFileName;

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


    protected void ImportExcel(object val)
    {

        LblUploadStatus.ForeColor = System.Drawing.Color.Black;
        LblUploadStatus.Text = "Processing Excel file...";
        LblUploadStatus.Visible = true;

        FileUpload1.Visible = false;
        ButtonUpload.Visible = false;

        string[] vals = (string[])val;

        Int32 nGroupID = int.Parse(vals[0]);
        string path = vals[1];
        string fileName = vals[2];


        ExcelFeeder.Feeder excelFeeder = new ExcelFeeder.Feeder(nGroupID, path, fileName);

        DataTable resultTable = new DataTable("resultTable");

        DataColumn colCoGuid = new DataColumn();
        colCoGuid.DataType = System.Type.GetType("System.String");
        colCoGuid.ColumnName = string.Format("co_guid");
        resultTable.Columns.Add(colCoGuid);

        DataColumn colStatus = new DataColumn();
        colStatus.DataType = System.Type.GetType("System.String");
        colStatus.ColumnName = string.Format("status");
        resultTable.Columns.Add(colStatus);

        DataColumn colMessage = new DataColumn();
        colMessage.DataType = System.Type.GetType("System.String");
        colMessage.ColumnName = string.Format("Message");
        resultTable.Columns.Add(colMessage);

        DataColumn colMid = new DataColumn();
        colMid.DataType = System.Type.GetType("System.String");
        colMid.ColumnName = string.Format("media_id");
        resultTable.Columns.Add(colMid);

        bool res = excelFeeder.ActualWork(ref resultTable);
        /*
        if (res)
        {

            GridView gv = new GridView();

            gv.DataSource = resultTable;
            gv.DataBind();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=ExcelUploaderResults.xls");
            HttpContext.Current.Response.Charset = "UTF-16";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            gv.RenderControl(htmlWrite);
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.End();
        }
        */
        lock (ThreadDict)
        {
            ThreadDict.Remove(nGroupID);
        }

    }
}