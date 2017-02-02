using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_users_list_excel : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_users_list.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_users_list.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            int nTotalRows = 0;
            DataTable dt = BuildUserGapTable(ref nTotalRows);
            dropSectionsList.DataSource = dt;
            dropSectionsList.DataTextField = "txt";
            dropSectionsList.DataValueField = "id";
            dropSectionsList.DataBind();

            if (Request.QueryString["search_free"] != null && !string.IsNullOrEmpty(Request.QueryString["search_free"].ToString()))
            {
                Session["searchFree"] = Request.QueryString["search_free"].ToString();
                txtFree.Text = Session["searchFree"].ToString();
            }
        }
    }

    protected void GetExcel(object sender, EventArgs e)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        DataTable dtUsers = new DataTable();
        Dictionary<int, List<string>> dUserDD = new Dictionary<int, List<string>>();

        int page = dropSectionsList.SelectedIndex;
        string sFreeText = txtFree.Text;

        int gap_between_user_ids = WS_Utils.GetTcmIntValue("gap_between_user_ids");

        log.DebugFormat("GetExcel - Get_UsersListByBulk group:{0}, top:{1}, page:{2}", nGroupID, gap_between_user_ids, page);

        DataSet ds = DAL.UsersDal.Get_UsersListByBulk(nGroupID, sFreeText, gap_between_user_ids, page);

        if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
        {
            dtUsers = ds.Tables[0];

            log.DebugFormat("GetExcel - Get_UsersListByBulk users tabel row count:{0}", dtUsers.Rows.Count);

            if (ds.Tables.Count == 2 && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
            {
                log.DebugFormat("GetExcel - Get_UsersListByBulk dynamic data tabel row count:{0}", ds.Tables[1].Rows.Count);

                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    int nUserID = ODBCWrapper.Utils.GetIntSafeVal(dr, "user_id");
                    string sKey = ODBCWrapper.Utils.GetSafeStr(dr, "DATA_TYPE");
                    string sVal = ODBCWrapper.Utils.GetSafeStr(dr, "DATA_VALUE");

                    if (string.IsNullOrEmpty(sKey) || string.IsNullOrEmpty(sVal))
                    {
                        continue;
                    }

                    string ddvalue = string.Format("<{0}:{1}>", sKey, sVal);

                    if (!dUserDD.ContainsKey(nUserID))
                    {
                        dUserDD.Add(nUserID, new List<string>() { ddvalue });
                    }
                    else
                    {
                        dUserDD[nUserID].Add(ddvalue);
                    }
                }

                for (int i = 0; i < dtUsers.Rows.Count; i++)
                {
                    int nUserID = int.Parse(dtUsers.Rows[i]["id"].ToString());
                    if (dUserDD.ContainsKey(nUserID))
                    {
                        dtUsers.Rows[i]["Dynamic_Data"] = string.Join(" ", dUserDD[nUserID]);
                    }
                }
            }


        }

        GridView gv = new GridView();

        gv.DataSource = dtUsers;
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



    protected void GetTotalRecords()
    {
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery.SetCachedSec(0);
            selectQuery += "SELECT  count (id) as numOfIDs  from  users u (nolock) WHERE STATUS=1 AND USERNAME NOT LIKE '%{Household}%' AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int numOfIDs = int.Parse(selectQuery.Table("query").DefaultView[0].Row["numOfIDs"].ToString());

                    HttpContext.Current.Response.Write(numOfIDs.ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            HttpContext.Current.Response.Write("0");
        }
    }

    protected void GetBulkSize()
    {
        try
        {
            int nBulk = WS_Utils.GetTcmIntValue("gap_between_user_ids");
            HttpContext.Current.Response.Write(nBulk.ToString());
        }
        catch (Exception)
        {
            HttpContext.Current.Response.Write("0");
        }
    }

    private DataTable BuildUserGapTable(ref int nTotalRecord)
    {
        try
        {
            int nBulk = WS_Utils.GetTcmIntValue("gap_between_user_ids");
            int numOfIDs = 0;
            int numOfBulks = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery.SetCachedSec(0);

            selectQuery += "SELECT  count (id) as numOfIDs  from  users u (nolock) WHERE STATUS=1 AND USERNAME NOT LIKE '%{Household}%' AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    numOfIDs = int.Parse(selectQuery.Table("query").DefaultView[0].Row["numOfIDs"].ToString());
                    nTotalRecord = numOfIDs;
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("txt", typeof(string));

            int div = (numOfIDs) / nBulk;
            int mod = (numOfIDs) % nBulk > 0 ? 1 : 0;
            numOfBulks = div + mod;

            for (int i = 0; i < numOfBulks; i++)
            {
                dt.Rows.Add(i + 1, (i * nBulk).ToString() + " - " + ((i + 1) * nBulk).ToString());
            }
            return dt;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            return new DataTable();
        }
    }

}