using System;
using System.Web;
using System.Text.RegularExpressions;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for LoginManager
    /// </summary>
    public class LoginManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public enum PAGE_PERMISION_TYPE
        {
            VIEW = 0,
            EDIT = 1,
            NEW = 2,
            REMOVE = 3,
            PUBLISH = 4
        }

        public LoginManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static Int32 GetLoginID()
        {
            Int32 nAcctID = 0;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Session.Get("Login") != null)
                    nAcctID = int.Parse(HttpContext.Current.Session.Get("Login").ToString());
                else
                    nAcctID = 0;
            }
            catch
            {
                nAcctID = 0;
            }
            return nAcctID;
        }

        public static Int32 GetLoginGroupID()
        {
            Int32 nGroupID = 0;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Session.Get("LoginGroup") != null)
                    nGroupID = int.Parse(HttpContext.Current.Session.Get("LoginGroup").ToString());
                else
                    nGroupID = 0;
            }
            catch
            {
                nGroupID = 0;
            }
            return nGroupID;
        }

        public static bool CheckLogin()
        {
            try
            {
                //string sURL = HttpContext.Current.Request.GetUrl().ToString().ToLower();
                //if (sURL.IndexOf("404;") != -1)
                //sURL = sURL.Substring(sURL.IndexOf("404;") + 4);
                //HttpContext.Current.Session.Get("RequestedURL"] = sURL;
                Int32 nAcctID = GetLoginID();
                if (nAcctID == 0)
                    return false;
                Int32 nAdminLoginID = GetMaxFromAdminLogin(true, true);
                if (nAdminLoginID == 0)
                    return false;

                if (nAdminLoginID != 0)
                {
                    ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                    directQuery += "update admin_login set last_action_date=getdate() where ";
                    directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nAdminLoginID);
                    directQuery.Execute();
                    directQuery.Finish();
                    directQuery = null;
                }
                //HttpContext.Current.Session.Get("RequestedURL"] = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetCurrentPageURL()
        {
            string sURL = HttpContext.Current.Request.GetFilePath().ToString();
            Int32 nStart = sURL.LastIndexOf('/') + 1;
            Int32 nEnd = sURL.Length;
            string sPage = sURL.Substring(nStart, nEnd - nStart);
            return sPage;
        }

        public static bool IsPagePermitted()
        {
            return IsPagePermitted(GetCurrentPageURL());
        }

        public static bool IsPagePermitted(string sPageURL)
        {
            return IsActionPermittedOnPage(sPageURL, PAGE_PERMISION_TYPE.VIEW);
        }

        public static bool IsActionPermittedOnPage(PAGE_PERMISION_TYPE actionType)
        {
            return IsActionPermittedOnPage(GetCurrentPageURL(), actionType);
        }

        public static bool CheckParentPermitted(Int32 nAcctID, ref Int32 nParent, string sField)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(3600);
            selectQuery += "select aap." + sField + ", am.PARENT_MENU_ID from admin_accounts_permissions aap,admin_menu am where aap.MENU_ID=am.id and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.MENU_ID", "=", nParent);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.ACCOUNT_ID", "=", nAcctID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nPermit = int.Parse(selectQuery.Table("query").DefaultView[0].Row[sField].ToString());
                    if (nPermit == 1)
                    {
                        nParent = int.Parse(selectQuery.Table("query").DefaultView[0].Row["PARENT_MENU_ID"].ToString());
                        bRet = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }

        public static bool IsActionPermittedOnPage(string sPageURL, PAGE_PERMISION_TYPE actionType)
        {
            try
            {
                if (sPageURL.IndexOf("?") != -1)
                    sPageURL = sPageURL.Substring(0, sPageURL.IndexOf("?"));
                bool bRet = false;
                Int32 nAcctID = GetLoginID();
                if (nAcctID == 0)
                    return false;
                string sFieldName = "";
                if (actionType == PAGE_PERMISION_TYPE.EDIT)
                    sFieldName = "EDIT_PERMIT";
                if (actionType == PAGE_PERMISION_TYPE.NEW)
                    sFieldName = "NEW_PERMIT";
                if (actionType == PAGE_PERMISION_TYPE.PUBLISH)
                    sFieldName = "PUBLISH_PERMIT";
                if (actionType == PAGE_PERMISION_TYPE.REMOVE)
                    sFieldName = "REMOVE_PERMIT";
                if (actionType == PAGE_PERMISION_TYPE.VIEW)
                    sFieldName = "View_PERMIT";

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetCachedSec(3600);
                selectQuery += "select ";
                selectQuery += sFieldName;
                selectQuery += " as co,am.PARENT_MENU_ID from admin_menu am,admin_accounts_permissions aap where aap.MENU_ID=am.id and aap.view_permit=1 and am.parent_menu_id<>0 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("aap.ACCOUNT_ID", "=", nAcctID);
                string sLike = "like ('" + sPageURL + "%') ";
                selectQuery += " and am.MENU_HREF " + sLike;
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("am.MENU_HREF", "=", sPageURL);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        Int32 nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                        if (nCo == 1)
                        {
                            Int32 nParent = int.Parse(selectQuery.Table("query").DefaultView[0].Row["PARENT_MENU_ID"].ToString());
                            bRet = true;
                            while (nParent != 0)
                            {
                                bRet = CheckParentPermitted(nAcctID, ref nParent, "View_PERMIT");
                                if (bRet == false)
                                    break;
                            }
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                return bRet;
            }
            catch
            {
                return false;
            }
        }

        public static string GetLoginName()
        {
            string sUserName = "";
            if (HttpContext.Current.Session.Get("username") != null)
                sUserName = HttpContext.Current.Session.Get("username").ToString();
            return sUserName;
        }
        public static string GetLoginGroupName()
        {
            string sGroupName = "";
            if (HttpContext.Current.Session.Get("groupname") != null)
                sGroupName = HttpContext.Current.Session.Get("groupname").ToString();
            return sGroupName;
        }

        public static string GetLoginName(Int32 nAccountID)
        {
            string sRet = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select username from accounts where account_type=2 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nAccountID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sRet = selectQuery.Table("query").DefaultView[0].Row["username"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRet;
        }

        static protected Int32 GetMaxFromAdminLogin(bool bSessinCheck, bool bAcctCheck)
        {
            Int32 nID = 0;
            Int32 nAcctID = GetLoginID();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select max(id) as max_id from admin_login where logout=0 ";
            if (bAcctCheck == true)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", nAcctID);
            }
            if (bSessinCheck == true)
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("session_id", "=", HttpContext.Current.Session.GetSessionID().ToString());
            }
            else
            {
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("session_id", "<>", HttpContext.Current.Session.GetSessionID().ToString());
            }
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    if (selectQuery.Table("query").DefaultView[0].Row["max_id"] != DBNull.Value)
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["max_id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nID;
        }

        static protected void LogoutAllOther()
        {
            Int32 nAcctID = GetLoginID();
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("admin_login");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("last_action_date", "=", DateTime.UtcNow);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("logout", "=", 2);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", nAcctID);
            updateQuery += "and id not in (select id from admin_login where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("session_id", "=", HttpContext.Current.Session.GetSessionID().ToString());
            updateQuery += ")";
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        public static void LogoutFromSite(string sFileToTransferTo)
        {
            string sBaseURL = "http://admin.tvinci.com";
            if (!string.IsNullOrEmpty(ApplicationConfiguration.TVMBaseUrl.Value))
            {
                sBaseURL = ApplicationConfiguration.TVMBaseUrl.Value;
            }

            try
            {
                Int32 nAcctID = GetLoginID();
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("admin_login");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("last_action_date", "=", DateTime.UtcNow);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("logout", "=", 1);
                updateQuery += "where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", nAcctID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                HttpContext.Current.Session.RemoveAll();
                if (HttpContext.Current.Request.GetUrl() != null && HttpContext.Current.Request.GetUrl().PathAndQuery.IndexOf("logout") == -1 &&
                    HttpContext.Current.Request.GetUrl().PathAndQuery.IndexOf("login") == -1)
                    HttpContext.Current.Session.Set("LOGOUT_FROM_PAGE", HttpContext.Current.Request.GetUrl().PathAndQuery);
                HttpContext.Current.Response.Write("<script>document.location.href='" + sBaseURL + sFileToTransferTo + "';</script>");
            }
            catch
            {
                HttpContext.Current.Session.Abandon();
                HttpContext.Current.Response.Write("<script>document.location.href='" + sBaseURL + sFileToTransferTo + "';</script>");
            }
        }

        static bool SetLoginValues(string sUserName, string sPassword, ref string sErrMessage)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(3600);
            selectQuery += "select a.id,a.username,a.PASSWORD,a.FAIL_COUNT,a.LAST_FAIL_DATE,a.group_id,g.GROUP_NAME,a.RH_ENTITY_ID from groups g,accounts a where g.id=a.group_id and a.is_active=1 and g.is_active=1 and a.status=1 and g.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("a.username", "=", sUserName);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    Int32 nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    string sGroupName = selectQuery.Table("query").DefaultView[0].Row["group_name"].ToString();
                    Int32 nFailCount = 0;
                    if (selectQuery.Table("query").DefaultView[0].Row["FAIL_COUNT"] != DBNull.Value)
                        nFailCount = int.Parse(selectQuery.Table("query").DefaultView[0].Row["FAIL_COUNT"].ToString());
                    DateTime lastFail = new DateTime(1, 1, 1);
                    if (selectQuery.Table("query").DefaultView[0].Row["LAST_FAIL_DATE"] != DBNull.Value)
                        lastFail = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["LAST_FAIL_DATE"]);
                    string sAccountPass = selectQuery.Table("query").DefaultView[0].Row["password"].ToString();

                    object oRHEntityID = selectQuery.Table("query").DefaultView[0].Row["RH_ENTITY_ID"];
                    Int32 nRHEntityID = 0;
                    if (oRHEntityID != null && oRHEntityID != DBNull.Value)
                    {
                        nRHEntityID = int.Parse(oRHEntityID.ToString());
                    }

                    if (sAccountPass == sPassword)
                    {
                        if (lastFail > DateTime.UtcNow.AddHours(-2))
                        {
                            if (nFailCount >= 3)
                            {
                                HttpContext.Current.Session.Set("Login","");
                                HttpContext.Current.Session.Set("LoginGroup", "");
                                HttpContext.Current.Session.Set("username", "");
                                HttpContext.Current.Session.Set("groupname", "");
                                HttpContext.Current.Session.Remove("Login");
                                HttpContext.Current.Session.Remove("LoginGroup");
                                HttpContext.Current.Session.Remove("username");
                                HttpContext.Current.Session.Remove("groupname");
                                log.Debug("ACOUNT_LOCK - UN: " + sUserName + " || Pass: " + sPassword);
                                sErrMessage = "ACOUNT_LOCK";
                                selectQuery.Finish();
                                selectQuery = null;
                                return bRet;
                            }
                        }

                        HttpContext.Current.Session.Set("Login", nCO);
                        HttpContext.Current.Session.Set("LoginGroup", nGroupID);
                        HttpContext.Current.Session.Set("username", selectQuery.Table("query").DefaultView[0].Row["username"].ToString());
                        HttpContext.Current.Session.Set("groupname", sGroupName);

                        if (nRHEntityID > 0)
                        {
                            HttpContext.Current.Session.Set("RightHolder", nRHEntityID);
                        }

                        bRet = true;
                        sErrMessage = "";

                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery += "update accounts set FAIL_COUNT=0,";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FAIL_DATE", "=", DBNull.Value);
                        directQuery += "where ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCO);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                    }
                    else
                    {
                        HttpContext.Current.Session.Set("Login", "");
                        HttpContext.Current.Session.Set("LoginGroup", "");
                        HttpContext.Current.Session.Set("username", "");
                        HttpContext.Current.Session.Set("groupname", "");
                        HttpContext.Current.Session.Remove("Login");
                        HttpContext.Current.Session.Remove("LoginGroup");
                        HttpContext.Current.Session.Remove("username");
                        HttpContext.Current.Session.Remove("groupname");
                        log.Debug("WRONG_PASSWORD - UN: " + sUserName + " || Pass: " + sPassword);
                        sErrMessage = "WRONG_USERNAME_PASS";

                        ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                        directQuery += "update accounts set FAIL_COUNT=FAIL_COUNT+1,";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FAIL_DATE", "=", DateTime.UtcNow);
                        directQuery += "where fail_count<3 and ";
                        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCO);
                        directQuery.Execute();
                        directQuery.Finish();
                        directQuery = null;
                    }
                }
                else
                {
                    HttpContext.Current.Session.Set("Login", "");
                    HttpContext.Current.Session.Set("LoginGroup", "");
                    HttpContext.Current.Session.Set("username", "");
                    HttpContext.Current.Session.Set("groupname", "");
                    HttpContext.Current.Session.Remove("Login");
                    HttpContext.Current.Session.Remove("LoginGroup");
                    HttpContext.Current.Session.Remove("username");
                    HttpContext.Current.Session.Remove("groupname");
                    log.Debug("WRONG_USERNAME - UN: " + sUserName + " || Pass: " + sPassword);
                    sErrMessage = "WRONG_USERNAME_PASS";
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }

        public static bool CreateNewAccount(string sUserName, string sPassword, Int32 nGroupID)
        {
            return CreateNewAccount(sUserName, sPassword, nGroupID, false, 0);
        }

        public static bool CreateNewAccount(string sUserName, string sPassword, Int32 nGroupID, bool bIsRightHolder, Int32 nRightHolderEntityID)
        {
            Int32 nCo = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from accounts where account_type=2 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("username", "=", sUserName);
            selectQuery += "and status<>2 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCo = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            if (nCo > 0)
                return false;

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("accounts");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("username", "=", sUserName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPassword);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_TYPE", "=", 2);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 3);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RH_ENTITY_ID", "=", nRightHolderEntityID);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            Int32 nAccountID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery2 += "select id from accounts where";
            selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("username", "=", sUserName);
            selectQuery2 += "and";
            selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPassword);
            selectQuery2 += "and";
            selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_TYPE", "=", 2);
            selectQuery2 += "and";
            selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 3);
            selectQuery2 += "and";
            selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery2 += "and";
            selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("RH_ENTITY_ID", "=", nRightHolderEntityID);
            if (selectQuery2.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery2.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nAccountID = int.Parse(selectQuery2.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery2.Finish();
            selectQuery2 = null;

            if (nAccountID > 0)
            {
                /*
                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select id,parent_menu_id from admin_menu ";
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nMenuID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["ID"].ToString());
                        Int32 nParentMenuID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["PARENT_MENU_ID"].ToString());
                        ODBCWrapper.InsertQuery insertQuery1 = new ODBCWrapper.InsertQuery("admin_accounts_permissions");
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", nAccountID);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", nMenuID);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("VIEW_PERMIT", "=", 1);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("EDIT_PERMIT", "=", 1);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("NEW_PERMIT", "=", 1);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("REMOVE_PERMIT", "=", 1);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PUBLISH_PERMIT", "=", 1);
                        insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", GetLoginID());
                        insertQuery1.Execute();
                        insertQuery1.Finish();
                        insertQuery1 = null;
                    }
                }
                selectQuery1.Finish();
                selectQuery1 = null;
                */

                if (bIsRightHolder)
                {
                    ODBCWrapper.InsertQuery insertQuery1 = new ODBCWrapper.InsertQuery("admin_accounts_permissions");
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", nAccountID);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", 97);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("VIEW_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("EDIT_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("NEW_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("REMOVE_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PUBLISH_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", GetLoginID());
                    insertQuery1.Execute();
                    insertQuery1.Finish();
                    insertQuery1 = null;

                    insertQuery1 = new ODBCWrapper.InsertQuery("admin_accounts_permissions");
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", nAccountID);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", 149);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("VIEW_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("EDIT_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("NEW_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("REMOVE_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PUBLISH_PERMIT", "=", 1);
                    insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", GetLoginID());
                    insertQuery1.Execute();
                    insertQuery1.Finish();
                    insertQuery1 = null;
                }
                else
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1 += "select * from admin_accounts_permissions where  ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", GetLoginID());
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            Int32 nMenuID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["menu_ID"].ToString());
                            Int32 nVIEW_PERMIT = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["VIEW_PERMIT"].ToString());
                            Int32 nEDIT_PERMIT = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["EDIT_PERMIT"].ToString());
                            Int32 nNEW_PERMIT = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["NEW_PERMIT"].ToString());
                            Int32 nREMOVE_PERMIT = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["REMOVE_PERMIT"].ToString());
                            Int32 nPUBLISH_PERMIT = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["PUBLISH_PERMIT"].ToString());

                            ODBCWrapper.InsertQuery insertQuery1 = new ODBCWrapper.InsertQuery("admin_accounts_permissions");
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", nAccountID);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", nMenuID);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("VIEW_PERMIT", "=", nVIEW_PERMIT);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("EDIT_PERMIT", "=", nEDIT_PERMIT);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("NEW_PERMIT", "=", nNEW_PERMIT);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("REMOVE_PERMIT", "=", nREMOVE_PERMIT);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("PUBLISH_PERMIT", "=", nPUBLISH_PERMIT);
                            insertQuery1 += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", GetLoginID());
                            insertQuery1.Execute();
                            insertQuery1.Finish();
                            insertQuery1 = null;
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;
                }
                return true;
            }
            return false;
        }

        public static bool IsPasswordStrong(string password)
        {
            return Regex.IsMatch(password, @"(?!^[0-9]*$)(?!^[a-zA-Z]*$)^([a-zA-Z0-9]{8,})$");
        }

        public static bool ChangeUserPassword(Int32 ncurrID, string sOldPassword, string sNewPassword)
        {
            try
            {
                Int32 nID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from accounts where account_type=2 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", ncurrID);
                if (sOldPassword != "")
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("password", "=", sOldPassword);
                }
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nID == 0)
                    return false;
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("accounts");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("password", "=", sNewPassword);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool LoginToSite(string sUserName, string sPassword, ref string sErrMessage)
        {
            bool bOtherLogin = false;
            Int32 nAdminLoginID = 0;
            bool bret = SetLoginValues(sUserName, sPassword, ref sErrMessage);
            if (bret == false)
                return false;
            Int32 nMaxID = GetMaxFromAdminLogin(false, true);
            DateTime dLastLogin = DateTime.UtcNow;
            DateTime dCurrentTime = DateTime.UtcNow;
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select GETDATE() as current_t,ID,LAST_ACTION_DATE from admin_login where ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMaxID);

            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nAdminLoginID = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["ID"].ToString());
                    dLastLogin = (DateTime)(selectQuery1.Table("query").DefaultView[0].Row["LAST_ACTION_DATE"]);
                    dCurrentTime = (DateTime)(selectQuery1.Table("query").DefaultView[0].Row["current_t"]);
                }
            }
            selectQuery1.Finish();
            selectQuery1 = null;
            if (nAdminLoginID != 0)
            {
                if (dLastLogin > dCurrentTime.AddMinutes(-1))
                {
                    bOtherLogin = true;
                }
            }
            if (bOtherLogin == true)
            {
                HttpContext.Current.Session.Set("Login", "");
                HttpContext.Current.Session.Set("LoginGroup", "");
                HttpContext.Current.Session.Set("username", "");
                HttpContext.Current.Session.Set("groupname", "");

                HttpContext.Current.Session.Remove("Login");
                HttpContext.Current.Session.Remove("LoginGroup");
                HttpContext.Current.Session.Remove("username");
                HttpContext.Current.Session.Remove("groupname");
                log.Debug("ACOUNT_LOGOUT_LOCK - UN: " + sUserName + " || Pass: " + sPassword);
                sErrMessage = "ACOUNT_LOGOUT_LOCK";
                return false;
            }
            if (bret == true)
            {
                LogoutAllOther();
                Int32 nAcctID = GetLoginID();

                Int32 nGroupID = LoginManager.GetLoginGroupID();
                //Int32 nAllIps = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "OPEN_ALL_IP", nGroupID).ToString());
                //bool bAllowedIP = true;
                //if (nAllIps == 0)
                //{

                string sIP = PageUtils.GetCallerIP();

                if (!ApplicationConfiguration.TVMSkipLoginIPCheck.Value)
                {
                    bool bAllowedIP = false;

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetCachedSec(0);
                    selectQuery += "select * from groups_ips(nolock) where ADMIN_OPEN=1 and status=1 and is_active=1 and (end_date is null or end_date>getdate()) and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RTRIM(LTRIM(LOWER(IP)))", "=", sIP.ToString().ToLower());
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                            bAllowedIP = true;
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    //}
                    if (bAllowedIP == false)
                    {
                        log.Debug("IP_NOT_ALLOWED - UN: " + sUserName + " || Pass: " + sPassword + " || IP: " + sIP);
                        sErrMessage = "IP_NOT_ALLOWED";
                        return false;
                    }
                }

                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("admin_login");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("last_action_date", "=", DateTime.UtcNow);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("session_id", "=", HttpContext.Current.Session.GetSessionID().ToString());
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("account_id", "=", nAcctID);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}