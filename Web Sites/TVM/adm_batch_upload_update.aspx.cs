using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using TVinciShared;
using System.Collections;
using System.Data;
using System.Diagnostics;
using KLogMonitor;
using System.Reflection;

public partial class adm_batch_upload_update : System.Web.UI.Page
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

        if (!IsPostBack)
        {
            Session["channels_ids"] = null;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Batch Upload - Update");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
    }

    public string changeItemStatus(string sID, string sAction)
    {
        List<string> channels = new List<string>();

        if (Session["channels_ids"] != null)
        {
            channels = (List<string>)Session["channels_ids"];
        }
        if (!ExsitsItList(channels, sID))
        {
            channels.Add(sID);
        }
        else
        {
            channels.Remove(sID);
        }

        Session["channels_ids"] = channels;

        return "";
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Channels To Export");
        dualList.Add("SecondListTitle", "Available Channels");

        object[] resultData = null;
        List<object> allChannels = new List<object>();

        List<string> channels = new List<string>();

        if (Session["channels_ids"] != null)
        {
            channels = (List<string>)Session["channels_ids"];
        }

        ODBCWrapper.DataSetSelectQuery selectQuery = null;
        try
        {
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ID, ADMIN_NAME from channels where status=1 and channel_type<>3 and watcher_id=0 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nLogedInGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ID", i);
                    string sTitle = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "ADMIN_NAME", i);

                    var data = new
                    {
                        ID = sID,
                        Title = sTitle,
                        Description = sTitle,
                        InList = ExsitsItList(channels, sID)
                    };
                    allChannels.Add(data);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            resultData = new object[allChannels.Count];
            resultData = allChannels.ToArray();

            dualList.Add("Data", resultData);
            dualList.Add("pageName", "adm_batch_upload_update.aspx");
            dualList.Add("withCalendar", false);

        }
        finally
        {
            if (selectQuery != null)
            {
                selectQuery.Finish();
                selectQuery = null;
            }
        }

        return dualList.ToJSON();
    }

    protected void GetExcel(object sender, EventArgs e)
    {

        if (Session["channels_ids"] == null)
            return;

        Int32 nGroupID = LoginManager.GetLoginGroupID();

        Int32[] nMedias = GetAllMediasID();

        if (nMedias == null || nMedias.Length == 0)
            return;

        ExcelGenerator.ExcelGenerator excelGenerator = new ExcelGenerator.ExcelGenerator(nGroupID, 0);

        DataTable resultTable = excelGenerator.GetExcelTableEdit(nMedias);

        string style = @"<style> td { mso-number-format:\@; text-align:left; } </style> ";

        GridView gv = new GridView();

        gv.DataSource = resultTable;
        gv.DataBind();

        Int32 nCount = gv.HeaderRow.Cells.Count;
        string color = string.Empty;

        for (int i = 0; i < gv.HeaderRow.Cells.Count; i++)
        {
            string val = gv.HeaderRow.Cells[i].Text;

            gv.HeaderRow.Cells[i].BackColor = System.Drawing.ColorTranslator.FromHtml("#000941");

            if (val.Contains(ExcelGenerator.CellType.BASIC.ToString()))
            {
                color = "#7FC474";
            }
            else if (val.Contains(ExcelGenerator.CellType.STRING.ToString()))
            {
                color = "#C4BA74";
            }
            else if (val.Contains(ExcelGenerator.CellType.DOUBLE.ToString()))
            {
                color = "#A7C474";
            }
            else if (val.Contains(ExcelGenerator.CellType.BOOLEAN.ToString()))
            {
                color = "#C49174";
            }
            else if (val.Contains(ExcelGenerator.CellType.TAG.ToString()))
            {
                color = "#C4747F";
            }
            else if (val.Contains(ExcelGenerator.CellType.FILE.ToString()))
            {
                string sName = resultTable.Columns[i].ColumnName;

                string[] seperator = { "<e>" };
                string[] splited = sName.Split(seperator, StringSplitOptions.None);

                int index = int.Parse(splited[1].Substring(0, splited[1].IndexOf("</e>")));

                color = "#7499C4";

                if (index % 2 == 0)
                {
                    color = "#87cefa";
                }
                else
                {
                    color = "#4682b4";
                }
            }

            gv.Rows[0].Cells[i].BackColor = System.Drawing.ColorTranslator.FromHtml(color);
            gv.Rows[0].Cells[i].ForeColor = System.Drawing.Color.White;

        }

        gv.Rows[0].Font.Bold = true;


        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
        HttpContext.Current.Response.Charset = "";
        // addition to solve the Hebrew - gibberish issue
        HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Unicode;
        Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
        //till here
        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        gv.RenderControl(htmlWrite);

        HttpContext.Current.Response.Write(style);
        HttpContext.Current.Response.Write(stringWrite.ToString());
        HttpContext.Current.Response.End();
    }

    private Int32[] GetAllMediasID()
    {
        // cast all channels ke to list 
        List<string> sChannelsIDS = (List<string>)Session["channels_ids"];

        int[] mediaIDs = null;

        if (sChannelsIDS != null && sChannelsIDS.Count > 0)
        {
            List<int> ChannelsIDS = sChannelsIDS.Select(x => int.Parse(x)).ToList();
            mediaIDs = GetMediaIdsFromCatalog(ChannelsIDS);
        }
        return mediaIDs;
    }

    private int[] GetMediaIdsFromCatalog(List<int> ChannelsIDS)
    {
        try
        {
            int[] assetIds;
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
            TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "Channel", "api", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");
            if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass))
            {
                return null;
            }

            apiWS.API client = new apiWS.API();
            client.Url = sWSURL;

            assetIds = client.GetChannelsAssetsIDs(sWSUserName, sWSPass, ChannelsIDS.ToArray(), null, false, string.Empty, false, true);
            return assetIds;
        }
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
            return null;
        }

    }

    private bool ExsitsItList(List<string> list, string val)
    {
        if (list != null)
        {
            foreach (string id in list)
            {
                if (id.Equals(val))
                    return true;
            }
        }

        return false;
    }
}
