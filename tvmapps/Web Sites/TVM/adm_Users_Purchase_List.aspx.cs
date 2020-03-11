using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using TVinciShared;

public partial class adm_Users_Purchase_List : System.Web.UI.Page
{

    public void GetMainMenu()
    {
        Int32 nMenuID = 0;
        Response.Write(TVinciShared.Menu.GetMainMenu(12, true, ref nMenuID));

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        CreateExcel2(93);
    }

    private void CreateExcel(int groupID)
    {
        ODBCWrapper.DataSetSelectQuery usersSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        usersSelectQuery.SetConnectionKey("users_connection");
        usersSelectQuery += "select * from users where ";
        usersSelectQuery += "GROUP_ID = " + groupID.ToString();
        usersSelectQuery += " and status = 1 and IS_ACTIVE = 1";
        DataTable dt = new DataTable();
        Int32 n = 0;
        string s = string.Empty;
        dt.Columns.Add(PageUtils.GetColumn("UserName", s));
        dt.Columns.Add(PageUtils.GetColumn("E-mail", s));
        dt.Columns.Add(PageUtils.GetColumn("SubscriptionsCount", n));
        dt.Columns.Add(PageUtils.GetColumn("PPVCount", n));
        if (usersSelectQuery.Execute("usersQuery", true) != null)
        {
            int usersCount = usersSelectQuery.Table("usersQuery").DefaultView.Count;
            Response.Write("Found " + usersCount.ToString() + " users" + "</br>");
            int ppvCount = 0;
            int subCount = 0;
            for (int i = 0; i < usersCount; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = dt.NewRow();
                int guid = int.Parse(usersSelectQuery.Table("usersQuery").DefaultView[i].Row["ID"].ToString());
                string userName = usersSelectQuery.Table("usersQuery").DefaultView[i].Row["USERNAME"].ToString();
                string email = usersSelectQuery.Table("usersQuery").DefaultView[i].Row["EMAIL_ADD"].ToString();
                Response.Write("i = " + i.ToString() + " User :" + userName + "</br>");
                ODBCWrapper.DataSetSelectQuery ppvSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                ppvSelectQuery.SetConnectionKey("ca_connection");
                ppvSelectQuery += "select * from ppv_purchases where ";
                ppvSelectQuery += "SITE_USER_GUID = " + guid.ToString();
                ppvSelectQuery += " and IS_ACTIVE = 1 and status = 1";
                if (ppvSelectQuery.Execute("ppvQuery", true) != null)
                {
                    ppvCount = ppvSelectQuery.Table("ppvQuery").DefaultView.Count;
                }
                ODBCWrapper.DataSetSelectQuery subSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                subSelectQuery.SetConnectionKey("ca_connection");
                subSelectQuery += "select * from subscriptions_purchases where ";
                subSelectQuery += "SITE_USER_GUID = " + guid.ToString();
                subSelectQuery += " and IS_ACTIVE = 1 and status = 1";
                if (subSelectQuery.Execute("subQuery", true) != null)
                {
                    subCount = subSelectQuery.Table("subQuery").DefaultView.Count;
                }
                tmpRow["UserName"] = userName;
                tmpRow["E-mail"] = email;
                tmpRow["SubscriptionsCount"] = subCount;
                tmpRow["PPVCount"] = ppvCount;
                dt.Rows.InsertAt(tmpRow, dt.Rows.Count);
                dt.AcceptChanges();
                ppvSelectQuery.Finish();
                ppvSelectQuery = null;
                subSelectQuery.Finish();
                subSelectQuery = null;
            }
            
        }
        usersSelectQuery.Finish();
        usersSelectQuery = null;
        
        Response.Write("Before excel");
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
    }

    private void CreateExcel2(int groupID)
    {
        Response.Write(groupID.ToString() + "</br>");
        ODBCWrapper.DataSetSelectQuery usersSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        usersSelectQuery.SetConnectionKey("users_connection");
        usersSelectQuery += "select top 100  * from users where ";
        usersSelectQuery += "GROUP_ID = " + groupID.ToString();
        usersSelectQuery += " and status = 1 and IS_ACTIVE = 1 and (users.ID not in (select site_user_guid from [ConditionalAccess].dbo.subscriptions_purchases)) and (users.ID not in (select site_user_guid from [ConditionalAccess].dbo.ppv_purchases ))";
        DataTable dt = new DataTable();
        Int32 n = 0;
        string s = string.Empty;
        dt.Columns.Add(PageUtils.GetColumn("UserName", s));
        dt.Columns.Add(PageUtils.GetColumn("E-mail", s));

        if (usersSelectQuery.Execute("usersQuery", true) != null)
        {
            int usersCount = usersSelectQuery.Table("usersQuery").DefaultView.Count;
            GridView gv = new GridView();

            gv.DataSource = usersSelectQuery.Table("usersQuery");
            gv.DataBind();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            usersSelectQuery.Finish();
            usersSelectQuery = null;
            gv.RenderControl(htmlWrite);
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.End();
            return;
            Response.Write("Found " + usersCount.ToString() + " users" + "</br>");
            int ppvCount = 0;
            int subCount = 0;
            for (int i = 0; i < usersCount; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = dt.NewRow();
                int guid = int.Parse(usersSelectQuery.Table("usersQuery").DefaultView[i].Row["ID"].ToString());
                string userName = usersSelectQuery.Table("usersQuery").DefaultView[i].Row["USERNAME"].ToString();
                string email = usersSelectQuery.Table("usersQuery").DefaultView[i].Row["EMAIL_ADD"].ToString();

                tmpRow["UserName"] = userName;
                tmpRow["E-mail"] = email;
                dt.Rows.InsertAt(tmpRow, dt.Rows.Count);
                dt.AcceptChanges();
                //ppvSelectQuery.Finish();
                //ppvSelectQuery = null;
                //subSelectQuery.Finish();
                //subSelectQuery = null;
            }

        }


        //Response.Write("Before excel");
        //GridView gv = new GridView();

        //gv.DataSource = dt;
        //gv.DataBind();
        //HttpContext.Current.Response.Clear();
        //HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
        //HttpContext.Current.Response.Charset = "UTF-8";
        //HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        //System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        //HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        //gv.RenderControl(htmlWrite);
        //HttpContext.Current.Response.Write(stringWrite.ToString());
        //HttpContext.Current.Response.End();
    }
}
