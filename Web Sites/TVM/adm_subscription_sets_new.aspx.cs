using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_subscription_sets_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    public static int maxOrderNum = 0;

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
                int updaterId = LoginManager.GetLoginID();
                DBManipulator.DoTheWork("pricing_connection");
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
                Session["set_id"] = 0;
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

        string sTable = theRecord.GetTableHTML("adm_subscription_sets_new.aspx?submited=1");

        return sTable;
    }

    public string initDualObj()
    {
        long setId = 0;
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
            Dictionary<long, SubscriptionSet> subscriptionsInSetMap = new Dictionary<long, SubscriptionSet>();
            if (Session["subscriptionsInSetMap"] != null)
            {
                subscriptionsInSetMap = (Dictionary<long, SubscriptionSet>)Session["subscriptionsInSetMap"];
                subscriptionSetsData.AddRange(subscriptionsInSetMap.Values);
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
                        subscriptionSet.InList = true;
                        subscriptionSet.OrderNum = priority;
                        if (priority > maxOrderNum)
                        {
                            maxOrderNum = priority;
                        }

                        if (!subscriptionsInSetMap.ContainsKey(subscriptionId))
                        {
                            subscriptionsInSetMap.Add(subscriptionId, subscriptionSet);
                        }
                    }
                    else
                    {
                        availableSubscriptionsMap.Add(subscriptionId, subscriptionSet);                        
                    }

                    subscriptionSetsData.Add(subscriptionSet);
                }                
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
        long setId = 0;
        Dictionary<long, SubscriptionSet> subscriptionsInSetMap = new Dictionary<long, SubscriptionSet>();
        Dictionary<long, SubscriptionSet> availableSubscriptionsMap = new Dictionary<long, SubscriptionSet>();
        if (Session["subscriptionsInSetMap"] != null && Session["availableSubscriptionsMap"] != null)
        {
            subscriptionsInSetMap = (Dictionary<long, SubscriptionSet>)Session["subscriptionsInSetMap"];
            availableSubscriptionsMap = (Dictionary<long, SubscriptionSet>)Session["availableSubscriptionsMap"];
        }

        long subscriptionId = 0;
        if (long.TryParse(id, out subscriptionId) & subscriptionId > 0)
        {
            if (subscriptionsInSetMap.ContainsKey(subscriptionId))
            {
                SubscriptionSet temp = new SubscriptionSet(subscriptionsInSetMap[subscriptionId]);
                temp.OrderNum = 0;
                temp.InList = false;
                availableSubscriptionsMap.Add(subscriptionId, temp);
                subscriptionsInSetMap.Remove(subscriptionId);
            }
            else
            {
                SubscriptionSet temp = new SubscriptionSet(availableSubscriptionsMap[subscriptionId]);
                temp.OrderNum = maxOrderNum + 1;
                temp.InList = true;
                subscriptionsInSetMap.Add(subscriptionId, availableSubscriptionsMap[subscriptionId]);
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
        if (long.TryParse(id, out subscriptionId) && subscriptionId > 0 &&
            int.TryParse(updatedOrderNum, out orderNum) && orderNum > 0 &&
            Session["subscriptionsInSetMap"] != null)
        {
            Dictionary<long, SubscriptionSet> subscriptionsInSetMap = (Dictionary<long, SubscriptionSet>)Session["subscriptionsInSetMap"];
            if (subscriptionsInSetMap.ContainsKey(subscriptionId))
            {
                subscriptionsInSetMap[subscriptionId].OrderNum = orderNum;
            }

            Session["subscriptionsInSetMap"] = subscriptionsInSetMap;
        }

        return "";
    }

    public class SubscriptionSet
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool InList { get; set; }
        public int OrderNum { get; set; }

        public SubscriptionSet() 
        {
            this.InList = false;
            this.OrderNum = 0;
        }

        public SubscriptionSet(SubscriptionSet set) 
        {
            this.ID = set.ID;
            this.Title = set.Title;
            this.Description = set.Description;
            this.InList = set.InList;
            this.OrderNum = set.OrderNum;
        }

    }
}
