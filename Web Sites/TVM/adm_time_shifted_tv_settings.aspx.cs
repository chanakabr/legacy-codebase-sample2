using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using KLogMonitor;
using TVinciShared;
using System.Data;

public partial class adm_time_shifted_tv_settings : System.Web.UI.Page
{

    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {


    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Time Shifted TV Settings");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object fieldIndexValue = null;

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        int groupID = LoginManager.GetLoginGroupID();

        // check if to insert a new record to the table or update an existing one
        int idFromTable = DAL.TvmDAL.GetTimeShiftedTVSettingsID(groupID);
        int nParentGroupID = DAL.UtilsDal.GetParentGroupID(groupID);

        string sTable = string.Empty;
 
        if (idFromTable > 0)
        {
            fieldIndexValue = idFromTable;
        }

        string sBack = "adm_time_shifted_tv_settings.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_parameters", "adm_table_pager", sBack, "", "ID", fieldIndexValue, sBack, "");
        theRecord.SetConnectionKey("users_connection_string");

        DataRecordCheckBoxField dr_catchUp = new DataRecordCheckBoxField(true);
        dr_catchUp.Initialize("Enable Catch-up", "adm_table_header_nbg", "FormInput", "allow_catchup", false);
        theRecord.AddRecord(dr_catchUp);

        DataRecordCheckBoxField dr_cdvr = new DataRecordCheckBoxField(true);
        dr_catchUp.Initialize("Enable C-DVR", "adm_table_header_nbg", "FormInput", "allow_cdvr", false);
        theRecord.AddRecord(dr_catchUp);


        sTable = theRecord.GetTableHTML("adm_time_shifted_tv_settings.aspx?submited=1");

        return sTable;
    }

    private bool IsParentGroup(int groupID)
    {
        bool res = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select * from F_Get_GroupsParent(" + groupID.ToString() + ")";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "PARENT_GROUP_ID");
                    if (parentGroupID == 1)
                    {
                        res = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            res = false;
        }
        return res;
    }
    
}