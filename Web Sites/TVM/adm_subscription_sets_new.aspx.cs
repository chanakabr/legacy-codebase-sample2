using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_subscription_sets_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    private static int maxOrderNum = 0;

    protected string m_sMenu;
    protected string m_sSubMenu;    

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {                
                long setId = DBManipulator.DoTheWork("pricing_connection");
                if (setId > 0 && UpdateSubscriptionsInSet(setId))
                {
                    Session["set_id"] = 0;
                    Session["subscriptionsInSetMap"] = null;
                    Session["availableSubscriptionsMap"] = null;
                    EndOfAction();
                }
                else
                {
                    log.ErrorFormat("Failed Inserting or Updating Set, setId: {0}", setId);
                    HttpContext.Current.Session["error_msg"] = "incorrect values while updating / failed inserting new set";
                }

                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["set_id"] != null && Request.QueryString["set_id"].ToString() != "")
            {
                Session["set_id"] = int.Parse(Request.QueryString["set_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("sets", "group_id", int.Parse(Session["set_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                Session["set_id"] = 0;
                Session["subscriptionsInSetMap"] = null;
                Session["availableSubscriptionsMap"] = null;
            }
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Subscription Sets";
        if (Session["set_id"] != null && Session["set_id"].ToString() != "" && Session["set_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["set_id"] != null && Session["set_id"].ToString() != "" && int.Parse(Session["set_id"].ToString()) != 0)
            t = Session["set_id"];
        string sBack = "adm_subscription_sets.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("sets", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_subscription_sets_new.aspx?submited=1");

        return sTable;
    }

    public string initDualObj()
    {
        long setId = 0;
        if (Session["set_id"] != null)
        {
            long.TryParse(Session["set_id"].ToString(), out setId);
        }

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Subscriptions Included In Set");
        dualList.Add("FirstListWithOrderByButtons", true);
        dualList.Add("SecondListTitle", "Available Subscriptions");

        object[] resultData = null;
        List<object> subscriptionSetsData = new List<object>();
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        DataTable subscriptions = TvmDAL.GetAvailableSubscriptionsBySetId(nLogedInGroupID, setId);
        if (subscriptions != null && subscriptions.Rows != null)
        {
            Dictionary<long, SubscriptionSetWithOrder> subscriptionsInSetMap = new Dictionary<long, SubscriptionSetWithOrder>();
            if (Session["subscriptionsInSetMap"] != null)
            {
                subscriptionsInSetMap = (Dictionary<long, SubscriptionSetWithOrder>)Session["subscriptionsInSetMap"];
            }

            Dictionary<long, SubscriptionSet> availableSubscriptionsMap = new Dictionary<long, SubscriptionSet>();
            foreach (DataRow dr in subscriptions.Rows)
            {
                long subscriptionId = ODBCWrapper.Utils.GetLongSafeVal(dr, "SUBSCRIPTION_ID", 0);
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                string description = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                bool isPartOfSet = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_PART_OF_SET", 0) > 0;
                int priority = isPartOfSet ? ODBCWrapper.Utils.GetIntSafeVal(dr, "PRIORITY", 0) : 0;
                if (subscriptionId > 0 && !string.IsNullOrEmpty(name))
                {
                    SubscriptionSet subscriptionSet = new SubscriptionSet() { ID = subscriptionId, Title = name, Description = !string.IsNullOrEmpty(description) ? description : name};
                    if (isPartOfSet && priority > 0)
                    {
                        SubscriptionSetWithOrder subscriptionSetWithOrder = new SubscriptionSetWithOrder(subscriptionSet);
                        subscriptionSetWithOrder.InList = true;
                        subscriptionSetWithOrder.OrderNum = priority;
                        if (priority > maxOrderNum)
                        {
                            maxOrderNum = priority;
                        }

                        if (!subscriptionsInSetMap.ContainsKey(subscriptionId))
                        {
                            subscriptionsInSetMap.Add(subscriptionId, subscriptionSetWithOrder);                            
                        }                        
                    }
                    else
                    {
                        availableSubscriptionsMap.Add(subscriptionId, subscriptionSet);
                        subscriptionSetsData.Add(subscriptionSet);
                    }                    
                }
            }

            if (subscriptionsInSetMap.Count > 0)
            {
                subscriptionsInSetMap = subscriptionsInSetMap.OrderBy(x => x.Value.OrderNum).ToDictionary(x => x.Key, x => x.Value);
                subscriptionSetsData.AddRange(subscriptionsInSetMap.Values);
            }

            Session["subscriptionsInSetMap"] = subscriptionsInSetMap;
            Session["availableSubscriptionsMap"] = availableSubscriptionsMap;
        }

        resultData = new object[subscriptionSetsData.Count];
        resultData = subscriptionSetsData.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_subscription_sets_new.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    public string changeItemStatus(string id, string sAction)
    {        
        Dictionary<long, SubscriptionSetWithOrder> subscriptionsInSetMap = new Dictionary<long, SubscriptionSetWithOrder>();
        Dictionary<long, SubscriptionSet> availableSubscriptionsMap = new Dictionary<long, SubscriptionSet>();
        if (Session["subscriptionsInSetMap"] != null && Session["availableSubscriptionsMap"] != null)
        {
            subscriptionsInSetMap = (Dictionary<long, SubscriptionSetWithOrder>)Session["subscriptionsInSetMap"];
            availableSubscriptionsMap = (Dictionary<long, SubscriptionSet>)Session["availableSubscriptionsMap"];
        }

        long subscriptionId = 0;
        if (long.TryParse(id, out subscriptionId) & subscriptionId > 0)
        {
            if (subscriptionsInSetMap.ContainsKey(subscriptionId))
            {
                SubscriptionSet temp = new SubscriptionSet(subscriptionsInSetMap[subscriptionId]);
                temp.InList = false;                
                availableSubscriptionsMap.Add(subscriptionId, temp);
                // if current item orderNum equals current maxOrderNum we may need to update, depends on other items
                if (subscriptionsInSetMap[subscriptionId].OrderNum == maxOrderNum)
                {
                    maxOrderNum = subscriptionsInSetMap.Select(x => x.Value.OrderNum).Max();
                }

                subscriptionsInSetMap.Remove(subscriptionId);
            }
            else
            {
                SubscriptionSetWithOrder temp = new SubscriptionSetWithOrder(availableSubscriptionsMap[subscriptionId]);
                // update current maxOrderNum value
                maxOrderNum++;
                temp.OrderNum = maxOrderNum;
                temp.InList = true;
                subscriptionsInSetMap.Add(subscriptionId, temp);
                availableSubscriptionsMap.Remove(subscriptionId);                
            }

            Session["subscriptionsInSetMap"] = subscriptionsInSetMap;
            Session["availableSubscriptionsMap"] = availableSubscriptionsMap;
        }

        return "";
    }

    public string changeItemOrder(string id, string updatedOrderNum)
    {
        long subscriptionId = 0;
        int orderNum = 0;
        if (long.TryParse(id, out subscriptionId) && subscriptionId > 0 && int.TryParse(updatedOrderNum, out orderNum) && orderNum > 0 && Session["subscriptionsInSetMap"] != null)
        {
            Dictionary<long, SubscriptionSetWithOrder> subscriptionsInSetMap = (Dictionary<long, SubscriptionSetWithOrder>)Session["subscriptionsInSetMap"];
            if (subscriptionsInSetMap.ContainsKey(subscriptionId))
            {
                // if new orderNum is bigger than current maxOrderNum, just update current maxOrderNum value
                if (orderNum > maxOrderNum)
                {
                    maxOrderNum = orderNum;
                }
                // if current item orderNum equals current maxOrderNum we may need to update, depends on other items                
                else if (subscriptionsInSetMap[subscriptionId].OrderNum == maxOrderNum)
                {
                    maxOrderNum = subscriptionsInSetMap.Select(x => x.Value.OrderNum).Max();
                }

                subscriptionsInSetMap[subscriptionId].OrderNum = orderNum;
            }

            Session["subscriptionsInSetMap"] = subscriptionsInSetMap;
        }

        return "";
    }

    private bool UpdateSubscriptionsInSet(long setId)
    {
        bool res = false;
        if (setId <= 0 || Session["subscriptionsInSetMap"] == null)
        {
            return res;
        }

        try
        {
            Dictionary<long, SubscriptionSetWithOrder> subscriptionsInSetMap = (Dictionary<long, SubscriptionSetWithOrder>)Session["subscriptionsInSetMap"];
            List<KeyValuePair<long, int>> subscriptionsToUpdate = new List<KeyValuePair<long, int>>();
            foreach (KeyValuePair<long, SubscriptionSetWithOrder> pair in subscriptionsInSetMap)
            {
                subscriptionsToUpdate.Add(new KeyValuePair<long, int>(pair.Key, pair.Value.OrderNum));
            }

            res = TvmDAL.UpdateSubscriptionsInSet(LoginManager.GetLoginGroupID(), setId, subscriptionsToUpdate, LoginManager.GetLoginID());
        }
        catch (Exception ex)
        {
            log.Error(string.Format("Failed UpdateSubscriptionsInSet, setId: {0}", setId), ex);
        }

        return res;
    }

    private void EndOfAction()
    {
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["success_back_page"] != null)
            HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
        else
            HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
    }

}
