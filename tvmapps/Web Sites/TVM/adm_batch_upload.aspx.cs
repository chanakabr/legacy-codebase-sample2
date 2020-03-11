using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using ExcelGenerator;
using ExcelFeeder;
using System.IO;
using System.Threading;

public partial class adm_batch_upload : System.Web.UI.Page
{
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

        LblGeneratorStatus.Visible = false;

    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Batch Upload - Generate");
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        string Str = TextBoxNumOfFiles.Text.Trim();

        Int32 Num;

        bool isNum = Int32.TryParse(Str, out Num);

        if (!isNum)
        {

            LblGeneratorStatus.ForeColor = System.Drawing.Color.Red;
            LblGeneratorStatus.Text = "Number of files must be a number";
            LblGeneratorStatus.Visible = true;

            return;
            
        }
        

        ExcelGenerator.ExcelGenerator excelGenerator = new ExcelGenerator.ExcelGenerator(nGroupID, Num);

        DataTable resultTable = excelGenerator.GetExcelTable(5);

        string style = @"<style> td { mso-number-format:\@; text-align:left; } </style> ";

        GridView gv = new GridView();

        gv.DataSource = resultTable;
        gv.DataBind();

        //gv.HeaderRow.BackColor = System.Drawing.ColorTranslator.FromHtml("#000941");
        //gv.HeaderRow.ForeColor = System.Drawing.Color.White;

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
        HttpContext.Current.Response.Charset = "UTF-16";
        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";

        System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        gv.RenderControl(htmlWrite);
        HttpContext.Current.Response.Write(style);
        HttpContext.Current.Response.Write(stringWrite.ToString());
        HttpContext.Current.Response.End();
    }

}