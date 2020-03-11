using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

public partial class ClearPermitted : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    protected void ClearButtonClick(object sender, EventArgs e)
    {
        string userTxt = UNTxt.Text;
        string passTxt = PassTxt.Text;
        int siteGuid = 0;
        ODBCWrapper.DataSetSelectQuery usersSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        usersSelectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
        usersSelectQuery += " select id from users where group_id = 125 and is_active = 1 and status = 1 and ";
        usersSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("username", "=", userTxt);
        usersSelectQuery += " and ";
        usersSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("password", "=", passTxt);
        
        if (usersSelectQuery.Execute("query", true) != null)
        {
            int userCount = usersSelectQuery.Table("query").DefaultView.Count;
            if (userCount > 0)
            {
                siteGuid = int.Parse(usersSelectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            }
           
        }
        usersSelectQuery.Finish();
        usersSelectQuery = null;
        
        string itemsStr = ItemIDTxt.Text;
        string subStr = SubscriptionIDTxt.Text;

        if (siteGuid > 0)
        {
            if (PermittedItemsCB.Checked)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", siteGuid);
                if (!string.IsNullOrEmpty(itemsStr) && itemsStr != "0")
                {
                    StringBuilder sb = new StringBuilder();
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += " select id from media_files where is_active = 1 and status = 1 and ";
                    selectQuery += " media_id in (";
                    selectQuery += itemsStr;
                    selectQuery += ")";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                string fileStr = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                                if (i != 0)
                                {
                                    sb.Append(",");
                                }
                                sb.Append(fileStr);
                            }
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    updateQuery += " and ";
                    updateQuery += "media_file_id in (";
                    updateQuery += sb.ToString();
                    updateQuery += ")";
                }
                bool isSuccess = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                if (!isSuccess)
                {
                    ItemsErrorMsg.Value = "Error";
                }
                else
                {
                    ItemsErrorMsg.Value = "None";
                }
                ODBCWrapper.UpdateQuery marksUpdateQuery = new ODBCWrapper.UpdateQuery("users_media_mark");
                marksUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("location_sec", "=", 0);
                marksUpdateQuery += " where ";
                marksUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", siteGuid);
                if (!string.IsNullOrEmpty(itemsStr) && itemsStr != "0")
                {
                    marksUpdateQuery += " and media_id in (";
                    marksUpdateQuery += itemsStr;
                    marksUpdateQuery += ")";
                }
                marksUpdateQuery.Execute();
                marksUpdateQuery = null;
            }
            if (PermittedSubsCB.Checked)
            {
                string subsStr = SubscriptionIDTxt.Text;
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", "=", siteGuid);
                if (!string.IsNullOrEmpty(subsStr) && subsStr != "0")
                {
                    updateQuery += " and subscription_code in (";
                    updateQuery += subsStr;
                    updateQuery += ")";
                }
                bool isSuccess = updateQuery.Execute();
                updateQuery = null;
                if (!isSuccess)
                {
                    SubsErrorMsg.Value = "Error";
                }
                else
                {
                    SubsErrorMsg.Value = "None";
                }
            }
        }
    }
}