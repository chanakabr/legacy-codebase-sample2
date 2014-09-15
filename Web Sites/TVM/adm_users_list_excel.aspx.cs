using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_users_list_excel : System.Web.UI.Page
{
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
        if (Request.QueryString["search_free"] != null && !string.IsNullOrEmpty(Request.QueryString["search_free"].ToString()))
        {
            Session["searchFree"] = Request.QueryString["search_free"].ToString();
        }
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {               
                GetTableCSV();
                return;
            }
        }

    }

    public string GetTableCSV()
    {
 
        DataTable dtUsers = new DataTable();
        Dictionary<int, string> dUserDD = new Dictionary<int, string>();
        
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sFreeText = string.Empty;
        int gap_between_user_ids = WS_Utils.GetTcmIntValue("gap_between_user_ids");
        int page = 0;
        if (Request.Form["0_val"] != null && !string.IsNullOrEmpty(Request.Form["0_val"].ToString())) // bulk for ids 
        {
            page = int.Parse(Request.Form["0_val"].ToString());
        }
        if (Request.Form["1_val"] != null && !string.IsNullOrEmpty(Request.Form["1_val"].ToString())) // Free text for username 
        {
            sFreeText = Request.Form["1_val"].ToString();
        }

        DataSet ds = DAL.UsersDal.Get_UsersListByBulk(nGroupID, sFreeText, gap_between_user_ids, page);

        if (ds != null && ds.Tables != null && ds.Tables.Count >= 1)
        {
            dtUsers = ds.Tables[0];

            if (ds.Tables.Count == 2)
            {
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    int nUserID = ODBCWrapper.Utils.GetIntSafeVal(dr, "user_id");
                    string sKey = ODBCWrapper.Utils.GetSafeStr(dr, "DATA_TYPE");
                    string sVal = ODBCWrapper.Utils.GetSafeStr(dr, "DATA_VALUE");
                    if (!dUserDD.ContainsKey(nUserID))
                    {
                        dUserDD.Add(nUserID, string.Empty);
                    }
                    dUserDD[nUserID] += string.Format("<{0}:{1}> ", sKey, sVal);                    
                }

                for (int i = 0; i < dtUsers.Rows.Count; i++)
                {
                    int nUserID = int.Parse(dtUsers.Rows[i]["id"].ToString());
                    if (dUserDD.ContainsKey(nUserID))
                    {
                        dtUsers.Rows[i]["Dynamic_Data"] = dUserDD[nUserID];
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

        return "";
    }


    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object t = null;
        DBRecordWebEditor theRecord = new DBRecordWebEditor("users", "adm_table_pager", "adm_users_list_excel.aspx", "", "ID", t, "javascript:window.close();", "");
        theRecord.SetConnectionKey("users_connection");
             
        // get the default gap from confuguration
        DataTable dt = BuildUserGapTable();        
        DataRecordDropDownField dr_use_list = new DataRecordDropDownField("", "txt", "ID", string.Empty, string.Empty, 60, true);
        dr_use_list.SetSelectsDT(dt);
        dr_use_list.Initialize("user gap list", "adm_table_header_nbg", "FormInput", "gapList", false);
        dr_use_list.SetDefault(0);
        theRecord.AddRecord(dr_use_list);

        DataRecordShortTextField dr_free = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_free.Initialize("free", "adm_table_header_nbg", "FormInput", "username", false);
        if (Session["searchFree"] != null && !string.IsNullOrEmpty(Session["searchFree"].ToString()))
        {
            dr_free.SetValue(Session["searchFree"].ToString());
        }
        theRecord.AddRecord(dr_free);        

        string sTable = theRecord.GetTableHTML("adm_users_list_excel.aspx?submited=1");

        return sTable;
    }

    private DataTable BuildUserGapTable()
    {
        try
        {
            int nBulk = WS_Utils.GetTcmIntValue("gap_between_user_ids");
            int numOfIDs = 0;
            int numOfBulks = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery.SetCachedSec(0);

            selectQuery += "SELECT  count (id) as numOfIDs  from  users u (nolock) WHERE IS_ACTIVE=1 AND STATUS=1 AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    numOfIDs = int.Parse(selectQuery.Table("query").DefaultView[0].Row["numOfIDs"].ToString());
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
                dt.Rows.Add(i+1, (i * nBulk).ToString() + " - " + ((i + 1) * nBulk).ToString());
            }
            return dt;
        }
        catch (Exception ex)
        {
            return new DataTable();
        }
    }


}