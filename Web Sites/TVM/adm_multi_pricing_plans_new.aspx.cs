using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using TvinciImporter;
using System.Data;
using KLogMonitor;
using System.Reflection;

public partial class adm_multi_pricing_plans_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected List<UMObj> m_usageModules;
    private const string OLD_MPP_NAME_SESSION_KEY = "OldMPPName";

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 6);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        string sMainLang = "";
        string sMainCode3 = "";
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {

                Int32 nSuscriptionID = DBManipulator.DoTheWork("pricing_connection");
                List<int> newSutIDS = Session["sub_user_types"] as List<int>;

                UpdateUsageModules(nSuscriptionID, LoginManager.GetLoginGroupID());
                UpdateUserTypes(nSuscriptionID, LoginManager.GetLoginGroupID(), newSutIDS);
                Session["subscription_id"] = nSuscriptionID.ToString();

                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                string sCode3 = Session["lang_code"].ToString();
                Int32 nSubscriptionDesc = 0;
                Int32 nSubscriptionName = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from subscription_descriptions where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nSubscriptionDesc = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery2 += "select id from subscription_names where is_active=1 and status=1 and ";
                selectQuery2.SetConnectionKey("pricing_connection");
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                if (selectQuery2.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery2.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nSubscriptionName = int.Parse(selectQuery2.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery2.Finish();
                selectQuery2 = null;

                Int32 nIter = 1;
                string sLang = "";

                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang, ref sMainCode3))
                    nIter = 0;
                string sName = "";
                sName = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;
                string sDesc = "";
                sDesc = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;


                if (nSubscriptionDesc != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscription_descriptions");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionDesc);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscription_descriptions");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }

                if (nSubscriptionName != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscription_names");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sName);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionName);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscription_names");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sName);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }

                try
                {
                    Notifiers.BaseSubscriptionNotifier t = null;
                    Notifiers.Utils.GetBaseSubscriptionsNotifierImpl(ref t, LoginManager.GetLoginGroupID(), "pricing_connection");
                    if (t != null)
                        t.NotifyChange(nSuscriptionID.ToString());
                    //return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + nSuscriptionID.ToString() + " : " + ex.Message, ex);
                }
                if (nSuscriptionID != 0)
                {
                    log.Debug("MultiUM - Subscription " + nSuscriptionID.ToString() + " Found");
                    string priceCode = string.Empty;
                    string firstUsageModuleCode = string.Empty;
                    int nExtDisountID = 0;
                    ODBCWrapper.DataSetSelectQuery umSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                    umSelectQuery.SetConnectionKey("pricing_connection");
                    umSelectQuery.SetCachedSec(0);
                    umSelectQuery += " select um.id, um.pricing_id, um.ext_discount_id from subscriptions_usage_modules sum, usage_modules um where sum.is_Active = 1 and sum.status = 1 and um.status = 1 and um.is_Active = 1 and um.id = sum.usage_module_id and ";
                    umSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("sum.subscription_id", "=", nSuscriptionID);
                    umSelectQuery += " order by sum.order_num asc";
                    if (umSelectQuery.Execute("query", true) != null)
                    {
                        log.Debug("MultiUM - Enter query");

                        int count = umSelectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            priceCode = umSelectQuery.Table("query").DefaultView[0].Row["pricing_id"].ToString();
                            log.Debug("MultiUM - Found price code " + priceCode.ToString());
                            firstUsageModuleCode = umSelectQuery.Table("query").DefaultView[0].Row["id"].ToString();
                            nExtDisountID = int.Parse(umSelectQuery.Table("query").DefaultView[0].Row["ext_discount_id"].ToString());
                            log.Debug("MultiUM - Found usage Module " + firstUsageModuleCode);
                        }
                    }
                    else
                    {
                        log.Debug("MultiUM - Not Enter query");
                    }
                    umSelectQuery.Finish();
                    umSelectQuery = null;
                    if (!string.IsNullOrEmpty(priceCode))
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions");
                        updateQuery.SetConnectionKey("pricing_connection");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("USAGE_MODULE_CODE", "=", firstUsageModuleCode);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_USAGE_MODULE_CODE", "=", firstUsageModuleCode);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_PRICE_CODE", "=", priceCode);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Ext_discount_module", "=", nExtDisountID);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nSuscriptionID);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                    string sSubName = "";
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1.SetConnectionKey("pricing_connection");
                    selectQuery1 += "select * from subscriptions where ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nSuscriptionID);
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            sSubName = selectQuery1.Table("query").DefaultView[0].Row["NAME"].ToString();
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;

                    if (sSubName.Length > 0)
                    {
                        int idToUpdateInLucene = DBManipulator.BuildOrUpdateFictivicMedia("Package", sSubName, nSuscriptionID, LoginManager.GetLoginGroupID(), Session[OLD_MPP_NAME_SESSION_KEY] != null ? Session[OLD_MPP_NAME_SESSION_KEY].ToString() : string.Empty);
                        UpdateMediaUserTypes(idToUpdateInLucene, newSutIDS);
                        if (Session[OLD_MPP_NAME_SESSION_KEY] != null && Session[OLD_MPP_NAME_SESSION_KEY].ToString().Length > 0) // when updating media need to update in lucene as well. when creating the lucene update occurs on adm_media_new.aspx.cs
                        {
                            ImporterImpl.UpdateRecordInLucene(LoginManager.GetLoginGroupID(), idToUpdateInLucene);
                        }
                    }
                }

                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
            {
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
                Session["lang_code"] = ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "code3", int.Parse(Session["lang_id"].ToString()));
            }
            else
            {
                Session["lang_id"] = GetMainLang(ref sMainLang, ref sMainCode3);
                Session["lang_code"] = sMainCode3;
            }
            if (Request.QueryString["subscription_id"] != null &&
                Request.QueryString["subscription_id"].ToString() != "")
            {
                Session["subscription_id"] = int.Parse(Request.QueryString["subscription_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["subscription_id"] = 0;
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nGroupID);
        }

    }

    protected void UpdateUsageModules(int subID, int groupID)
    {
        if (Session["sub_usage_modules"] != null && Session["sub_usage_modules"] is List<UMObj>)
        {
            List<int> tempIDs = new List<int>();
            List<UMObj> newUMs = Session["sub_usage_modules"] as List<UMObj>;
            List<int> oldUMs = BuildSubscriptionUMs(subID, groupID, true);
            foreach (UMObj newObj in newUMs)
            {
                int newID = int.Parse(newObj.m_id);
                tempIDs.Add(newID);
                if (oldUMs.Contains(newID))
                {
                    UpdateUsageModule(newID, subID, newObj.m_orderNum);
                }
                else
                {
                    InsertUsageModule(newID, subID, newObj.m_orderNum, groupID);
                }

            }
            foreach (int oldID in oldUMs)
            {
                if (!tempIDs.Contains(oldID))
                {
                    RemoveUsageModule(oldID, subID);
                }
            }
        }
    }

    protected void UpdateUserTypes(int subID, int groupID, List<int> newSutIDS)
    {
        if (Session["sub_user_types"] != null && Session["sub_user_types"] is List<int>)
        {
            List<int> tempSutIDs = new List<int>();
            List<int> oldSutIDS = BuildSubscriptionUserTypes(subID, groupID, true);

            foreach (int newSutID in newSutIDS)
            {
                tempSutIDs.Add(newSutID);
                if (oldSutIDS.Contains(newSutID))
                {
                    UpdateUserType(subID, newSutID);
                }
                else
                {
                    InsertUserType(subID, newSutID, groupID);
                }
            }

            foreach (int oldSutID in oldSutIDS)
            {
                if (!tempSutIDs.Contains(oldSutID))
                {
                    RemoveUserType(subID, oldSutID);
                }
            }

        }
    }

    protected void UpdateMediaUserTypes(int mediaID, List<int> newSutIDS)
    {
        if (mediaID != 0 && Session["sub_user_types"] != null && Session["sub_user_types"] is List<int>)
        {
            object mediaUserTypesIDs = null;

            if (newSutIDS != null && newSutIDS.Count > 0)
            {
                mediaUserTypesIDs = string.Join(";", newSutIDS.Select(n => n.ToString()).ToArray());

            }
            else
            {
                mediaUserTypesIDs = DBNull.Value;
            }

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_types", "=", mediaUserTypesIDs);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", mediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
    }

    protected void RemoveUsageModule(int umID, int subID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_usage_modules");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", subID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", umID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected void UpdateUsageModule(int umID, int subID, int order_num)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_usage_modules");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("order_num", "=", order_num);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", subID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", umID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected void InsertUsageModule(int umID, int subID, int order_num, int groupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_usage_modules");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", subID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", umID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("order_num", "=", order_num);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void InsertUserType(int subID, int utID, int groupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_user_types");
        insertQuery.SetConnectionKey("pricing_connection");

        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", subID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_type_id", "=", utID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void RemoveUserType(int subID, int utID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_user_types");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", subID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_type_id", "=", utID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected void UpdateUserType(int subID, int utID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_user_types");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", subID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_type_id", "=", utID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected List<int> BuildSubscriptionUMs(int subID, int groupID, bool alsoUnActive)
    {
        List<int> retVal = new List<int>();
        List<UMObj> umList = new List<UMObj>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select um.id, um.name, sum.order_num from usage_modules um, subscriptions_usage_modules sum where sum.usage_module_id = um.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sum.subscription_id", "=", subID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("um.group_id", "=", groupID);
        if (!alsoUnActive)
        {
            selectQuery += " and sum.is_active = 1 and sum.status = 1";
        }
        selectQuery += "order by sum.order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    string title = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();
                    string description = string.Empty;
                    string uID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    int orderNum = int.Parse(selectQuery.Table("query").DefaultView[i].Row["order_num"].ToString());
                    UMObj umObj = new UMObj(uID, title, description, true, orderNum);
                    umList.Add(umObj);
                    retVal.Add(int.Parse(uID));
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        Session["sub_usage_modules"] = umList;
        return retVal;
    }

    protected List<int> BuildSubscriptionUserTypes(int subID, int groupID, bool alsoUnActive)
    {
        List<int> sutList = new List<int>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select sut.user_type_id from subscriptions_user_types sut(nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sut.subscription_id", "=", subID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sut.group_id", "=", groupID);
        if (!alsoUnActive)
        {
            selectQuery += " and sut.is_active = 1 and sut.status = 1";
        }

        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int sutID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[i].Row["user_type_id"]);
                    sutList.Add(sutID);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        Session["sub_user_types"] = sutList;
        return sutList;
    }

    protected bool IsUsageModuleBelong(Int32 nUsageID)
    {
        try
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from subscriptions_usage_modules where is_active=1 and status=1 and ";
            Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", int.Parse(Session["subscription_id"].ToString()));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", nUsageID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    bRet = true;
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

    public string changeItemStatus(string sID, string dualListName)
    {
        // userType dualList cChanged then call changeItemStatusUserTypes
        if (dualListName.ToUpper() == "DualListUserTypes".ToUpper())
        {
            changeItemStatusUserTypes(sID);
        }
        // pricePlans dualList changed then call changeItemStatusPricePlans
        else
        {
            changeItemStatusPricePlans(sID);
        }

        return "";
    }

    public void changeItemStatusPricePlans(string sID)
    {
        if (Session["sub_usage_modules"] != null && Session["sub_usage_modules"] is List<UMObj>)
        {
            List<UMObj> umObjList = Session["sub_usage_modules"] as List<UMObj>;
            bool isReorderNeeded = false;
            int currentOrderNum = -1;
            for (int i = 0; i < umObjList.Count; i++)
            {
                UMObj obj = umObjList[i];
                if (obj.m_id.Equals(sID))
                {
                    currentOrderNum = obj.m_orderNum;
                    umObjList.Remove(obj);
                    isReorderNeeded = true;
                    break;
                }
            }
            if (isReorderNeeded)
            {
                foreach (UMObj obj in umObjList)
                {
                    if (obj.m_orderNum > currentOrderNum)
                    {
                        obj.m_orderNum--;
                    }
                }
            }
            else
            {
                UMObj obj = new UMObj(sID, string.Empty, string.Empty, true, umObjList.Count);
                int newOrder = umObjList.Count;
                umObjList.Insert(umObjList.Count, obj);
                umObjList.Sort();
            }

            Session["sub_usage_modules"] = umObjList;
        }
    }

    public void changeItemStatusUserTypes(string sID)
    {
        if (Session["sub_user_types"] != null && Session["sub_user_types"] is List<int>)
        {
            List<int> sutList = Session["sub_user_types"] as List<int>;
            int sutIDClient = 0;
            bool isAddNeeded = true;
            if (int.TryParse(sID, out sutIDClient) && sutList.Remove(sutIDClient))
            {
                isAddNeeded = false;
            }

            if (isAddNeeded)
            {
                sutList.Add(sutIDClient);
            }

            Session["sub_user_types"] = sutList;
        }
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualLists = new Dictionary<string, object>();
        Dictionary<string, object> userTypes = new Dictionary<string, object>();
        Dictionary<string, object> pricingPlans = new Dictionary<string, object>();

        userTypes.Add("name", "DualListUserTypes");
        userTypes.Add("FirstListTitle", "User Types");
        userTypes.Add("SecondListTitle", "Available User Types");
        userTypes.Add("pageName", "adm_multi_pricing_plans_new.aspx");
        userTypes.Add("withCalendar", false);
        object[] userTypesData = null;
        initUserTypes(ref userTypesData);
        userTypes.Add("Data", userTypesData);

        pricingPlans.Add("name", "DualListPricePlans");
        pricingPlans.Add("FirstListTitle", "Pricing Plans");
        pricingPlans.Add("SecondListTitle", "Available Pricing Plans");
        pricingPlans.Add("pageName", "adm_multi_pricing_plans_new.aspx");
        pricingPlans.Add("withCalendar", false);
        object[] pricePlansData = null;
        initPricingPlans(ref pricePlansData);
        pricingPlans.Add("Data", pricePlansData);

        dualLists.Add("0", userTypes);
        dualLists.Add("1", pricingPlans);
        dualLists.Add("size", dualLists.Count);

        return dualLists.ToJSON();
    }

    public void initPricingPlans(ref object[] resultData)
    {
        List<object> pricePlans = new List<object>();
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from usage_modules where is_active=1 and status=1 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 2);
        List<int> subIMsIDs = null;
        if (Session["subscription_id"] != null && !string.IsNullOrEmpty(Session["subscription_id"].ToString()))
        {
            subIMsIDs = BuildSubscriptionUMs(int.Parse(Session["subscription_id"].ToString()), nCommerceGroupID, false);
        }
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sGroupID = selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString();
                string sTitle = "";
                if (selectQuery.Table("query").DefaultView[i].Row["NAME"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["NAME"] != DBNull.Value)
                    sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                string sDescription = "";
                if (subIMsIDs == null || (subIMsIDs != null && !subIMsIDs.Contains(int.Parse(sID))))
                {
                    var data = new
                    {
                        ID = sID,
                        Title = sTitle,
                        Description = sDescription,
                        InList = false
                    };
                    pricePlans.Add(data);
                }
            }
            if (Session["subscription_id"] != null && Session["sub_usage_modules"] != null)
            {
                int subID = int.Parse(Session["subscription_id"].ToString());
                List<UMObj> umObjList = Session["sub_usage_modules"] as List<UMObj>;
                foreach (UMObj obj in umObjList)
                {
                    var data = new
                    {
                        ID = obj.m_id,
                        Title = obj.m_title,
                        Description = obj.m_description,
                        InList = true
                    };
                    pricePlans.Add(data);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        resultData = new object[pricePlans.Count];
        resultData = pricePlans.ToArray();
    }

    public void initUserTypes(ref object[] resultData)
    {
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        string sTitle = "";
        int nID = 0;
        List<int> userTypesIDs = null;
        Dictionary<int, string> dictUserTypes = new Dictionary<int, string>();
        List<object> userTypes = new List<object>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("users_connection");
        selectQuery += "select ID,Description from users_types(nolock) where is_active=1 and status=1 and ";
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nLogedInGroupID, "");

        if (Session["subscription_id"] != null && !string.IsNullOrEmpty(Session["subscription_id"].ToString()))
        {
            userTypesIDs = BuildSubscriptionUserTypes(int.Parse(Session["subscription_id"].ToString()), nLogedInGroupID, false);
        }

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                nID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[i].Row["ID"]);
                sTitle = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[i].Row["Description"]);
                dictUserTypes.Add(nID, sTitle);

                if (userTypesIDs == null || (userTypesIDs != null && !userTypesIDs.Contains(nID)))
                {
                    var data = new
                    {
                        ID = nID,
                        Title = sTitle,
                        Description = sTitle,
                        InList = false
                    };
                    userTypes.Add(data);
                }
            }
            if (Session["subscription_id"] != null && Session["sub_user_types"] != null)
            {
                List<int> sutList = Session["sub_user_types"] as List<int>;
                foreach (int sutID in sutList)
                {
                    sTitle = dictUserTypes[sutID];
                    var data = new
                    {
                        ID = sutID,
                        Title = sTitle,
                        Description = sTitle,
                        InList = true
                    };
                    userTypes.Add(data);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        resultData = new object[userTypes.Count];
        resultData = userTypes.ToArray();

    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            string sMainLang = "";
            string sCode3 = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang, ref sCode3);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_subscriptions_new.aspx?subscription_id=" + Session["subscription_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
            else
                sTemp += "javascript:void(0);";
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sOnOff = "on";
                    if (nLangID != int.Parse(Session["lang_id"].ToString()))
                        sOnOff = "off";
                    sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
                    if (nLangID != int.Parse(Session["lang_id"].ToString()))
                    {
                        sTemp += "adm_subscriptions_new.aspx?subscription_id=" + Session["subscription_id"].ToString() + "&lang_id=" + nLangID.ToString();
                    }
                    else
                        sTemp += "javascript:void(0);";
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                if (nCount1 == 0)
                    sTemp = "";
            }
            selectQuery1.Finish();
            selectQuery1 = null;

            return sTemp;
        }
        catch
        {
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    static protected Int32 GetMainLang(ref string sMainLang, ref string sCode)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.NAME,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                sCode = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nLangID;
    }

    static protected string GetMainLang()
    {
        string sMainLang = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sMainLang;
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Subscription: ";
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "" && Session["subscription_id"].ToString() != "0")
        {
            object sSubName = ODBCWrapper.Utils.GetTableSingleVal("subscription_names", "description", "language_code3", "=", GetMainLang(), "pricing_connection");
            if (sSubName != null && sSubName != DBNull.Value)
                sRet += "(" + sSubName.ToString() + ")";
            sRet += " - Edit";
        }
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }

    protected string GetCurrentValue(string sField, string sTable, Int32 nsubscription_idID, string sLangCode, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nsubscription_idID);
        selectQuery += " and is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sLangCode);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oRet = selectQuery.Table("query").DefaultView[0].Row[sField];
                if (oRet != null && oRet != DBNull.Value)
                    sRet = oRet.ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    protected System.Data.DataTable GetBaseDT()
    {
        System.Data.DataTable dT = new System.Data.DataTable();
        Int32 n = 0;
        string s = "";
        dT.Columns.Add(PageUtils.GetColumn("ID", n));
        dT.Columns.Add(PageUtils.GetColumn("txt", s));
        return dT.Copy();
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null;
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "" && int.Parse(Session["subscription_id"].ToString()) != 0)
            t = Session["subscription_id"];

        string sBack = "adm_multi_pricing_plans.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("subscriptions", "adm_table_pager", sBack, "", "ID", t, string.Empty, "");
        theRecord.SetConnectionKey("pricing_connection");
        string sMainLang = "";
        string sMainCode = "";
        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang, ref sMainCode))
        {
            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
            theRecord.AddRecord(dr_domain);
        }

        DataRecordLongTextField dr_Title = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Title.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "0")
        {
            dr_Title.SetValue(GetCurrentValue("description", "subscription_names", int.Parse(Session["subscription_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Title.SetValue("");
        theRecord.AddRecord(dr_Title);

        DataRecordLongTextField dr_Description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Description.Initialize("Description", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "0")
        {
            dr_Description.SetValue(GetCurrentValue("description", "subscription_descriptions", int.Parse(Session["subscription_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Description.SetValue("");
        theRecord.AddRecord(dr_Description);

        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang, ref sMainCode))
        {
            DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
            dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
            dr_start_date.SetDefault(DateTime.Now);
            theRecord.AddRecord(dr_start_date);

            DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
            dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
            dr_end_date.SetDefault(new DateTime(2099, 1, 1));
            theRecord.AddRecord(dr_end_date);

        }

        string sWSUserName = "";
        string sWSPass = "";
        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPriceCodeList", "pricing", sIP, ref sWSUserName, ref sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = GetWSURL();
        if (sWSURL != "")
            m.Url = sWSURL;

        // discount
        DataRecordDropDownField dr_disc = new DataRecordDropDownField("coupons_groups", "code", "id", "", null, 60, true);
        string sQuery = "select code as txt,id as id from pricing..discount_codes where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID();
        dr_disc.SetSelectsQuery(sQuery);
        dr_disc.Initialize("Discounts (Internal Item)", "adm_table_header_nbg", "FormInput", "DISCOUNT_MODULE_CODE", false);
        string deafultVal = "---";
        dr_disc.SetDefaultVal(deafultVal);
        theRecord.AddRecord(dr_disc);

        // coupons
        DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("coupons_groups", "code", "id", "", null, 60, true);
        sQuery = "select code as txt,id as id from pricing..coupons_groups where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID();
        dr_coupons_group.SetSelectsQuery(sQuery);
        dr_coupons_group.Initialize("Coupon Group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_CODE", false);
        dr_coupons_group.SetDefaultVal(deafultVal);
        theRecord.AddRecord(dr_coupons_group);


        DataRecordShortTextField dr_Product_Code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Product_Code.Initialize("Product Code", "adm_table_header_nbg", "FormInput", "Product_Code", false);
        theRecord.AddRecord(dr_Product_Code);


        DataRecordBoolField dr_is_recurring = new DataRecordBoolField(true);
        dr_is_recurring.Initialize("Is subscription Renewed", "adm_table_header_nbg", "FormInput", "IS_RECURRING", false);
        theRecord.AddRecord(dr_is_recurring);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordDropDownField dr_preview_module = new DataRecordDropDownField("preview_modules", "Name", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from pricing..preview_modules where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID();
        dr_preview_module.SetSelectsQuery(sQuery);
        dr_preview_module.Initialize("Preview Module", "adm_table_header_nbg", "FormInput", "PREVIEW_MODULE_ID", false);
        deafultVal = "No Preview Module";
        dr_preview_module.SetDefaultVal(deafultVal);
        dr_preview_module.SetNoSelectStr(deafultVal);
        theRecord.AddRecord(dr_preview_module);

        DataTable DomainLimitationModulesDT = GetDomainLimitationModulesDT();

        DataRecordDropDownField dr_domain_limitation_module = new DataRecordDropDownField("groups_device_limitation_modules", "Name", "id", string.Empty, null, 60, true);
        dr_domain_limitation_module.SetSelectsDT(DomainLimitationModulesDT);
        dr_domain_limitation_module.Initialize("Domain Limitation Module", "adm_table_header_nbg", "FormInput", "device_limit_id", false);
        dr_domain_limitation_module.SetFieldType("string");
        dr_domain_limitation_module.SetDefaultVal(GetMppDlm(t));
        dr_domain_limitation_module.SetNoSelectStr("No Domain Limitation Module");

        theRecord.AddRecord(dr_domain_limitation_module);

        DataRecordShortIntField dr_grace_period_minutes = new DataRecordShortIntField(true, 9, 9, 0);
        dr_grace_period_minutes.Initialize("Grace Period (Minutes)", "adm_table_header_nbg", "FormInput", "GRACE_PERIOD_MINUTES", false);
        dr_grace_period_minutes.SetDefault(0);
        theRecord.AddRecord(dr_grace_period_minutes);

        m.Dispose();

        string sTable = theRecord.GetTableHTML("adm_multi_pricing_plans_new.aspx?submited=1", true);
        writeSubscriptionNameToSession(sTable);
        return sTable;
    }

    private string GetMppDlm(object subscriptionID)
    {
        string dlm = string.Empty;
        int subID = 0;
        if (subscriptionID != null && int.TryParse(subscriptionID.ToString(), out subID))
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select gdlm.NAME from groups_device_limitation_modules gdlm inner join Pricing..subscriptions s on gdlm.id = s.device_limit_id where gdlm.status = 1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("s.id", "=", subID);
            if (selectQuery.Execute("query", true) != null)
            {
                dlm = selectQuery.Table("query").Rows[0][0].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        return dlm;
    }

    private DataTable GetDomainLimitationModulesDT()
    {
        DataTable dt = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ID, NAME as txt from groups_device_limitation_modules where status=1 and is_active=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            dt = selectQuery.Table("query");
        }
        selectQuery.Finish();
        selectQuery = null;
        return dt;
    }

    private void writeSubscriptionNameToSession(string sTable)
    {
        Session[OLD_MPP_NAME_SESSION_KEY] = null;
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString().Length > 0)
        {
            int nSubscriptionID = int.Parse(Session["subscription_id"].ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select name from subscriptions where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                    Session[OLD_MPP_NAME_SESSION_KEY] = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (Session[OLD_MPP_NAME_SESSION_KEY] == null)
            Session[OLD_MPP_NAME_SESSION_KEY] = string.Empty;

    }


    
}