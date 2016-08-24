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
using System.Collections.Generic;
using DAL;
using System.Reflection;
using KLogMonitor;

public partial class adm_categories_channels : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_categories.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["category_id"] != null)
                Session["category_id"] = Request.QueryString["category_id"].ToString();
            else
                Session["category_id"] = null;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public void GetBackButton()
    {
        string sBack = "";
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && Session["category_id"].ToString() != "0")
        {
            sBack = "adm_categories.aspx?category_id=" + PageUtils.GetTableSingleVal("categories", "parent_category_id", int.Parse(Session["category_id"].ToString()));
        }
        if (sBack != "")
        {
            string sRet = "<tr><td id=\"back_btn\" onclick='window.document.location.href=\"" + sBack + "\";'><a href=\"#back_btn\" class=\"btn_back\"></a></td></tr>";
            Response.Write(sRet);
        }
    }

    protected string GetWhereAmIStr()
    {
        Int32 nCategoryID = 0;
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && Session["category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["category_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = 0;

        while (nCategoryID != nLast)
        {
            Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("categories", "parent_category_id", nCategoryID).ToString());
            string sHeader = PageUtils.GetTableSingleVal("categories", "CATEGORY_NAME", nCategoryID).ToString();
            if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?category_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            else
                sRet = sHeader;
            bFirst = false;
            nCategoryID = nParentID;
        }
        if (sRet != "")
            sRet = "Categories Channels: <span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?category_id=0';\">Root </span><span class=\"arrow\">&raquo; </span>" + sRet;
        else
            sRet = "Categories Channels: Root";
        return sRet;

    }    

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": " + GetWhereAmIStr());
    }

    public string changeItemStatus(string id, string sAction)
    {
        long categoryId = 0;
        if (Session["category_id"] == null || Session["category_id"].ToString() == "" || !long.TryParse(Session["category_id"].ToString(), out categoryId) || categoryId <= 0)
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        HashSet<long> includedChannelsHashset = new HashSet<long>();
        HashSet<long> availableChannelsHashset = new HashSet<long>();
        if (Session["includedChannels"] != null && Session["availableChannels"] != null)
        {
            includedChannelsHashset = (HashSet<long>)Session["includedChannels"];
            availableChannelsHashset = (HashSet<long>)Session["availableChannels"];
        }
        
        long channelId;
        if (long.TryParse(id, out channelId))
        {
            int updaterId = LoginManager.GetLoginID();
            if (includedChannelsHashset.Contains(channelId))
            {
                if (!TvmDAL.RemoveChannelFromCategory(updaterId, categoryId, channelId))
                {
                    log.ErrorFormat("Failed removing channel {0} from category {1}", channelId, categoryId);
                }
            }
            else
            {
                int groupId = LoginManager.GetLoginGroupID();                
                if (!TvmDAL.InsertChannelToCategory(groupId, updaterId, categoryId, channelId))
                {
                    log.ErrorFormat("Failed inserting channel {0} to category {1}", channelId, categoryId);
                }
            }
        }

        return "";
    }

    public string changeItemOrder(string id, string updatedOrderNum)
    {
        long categoryId = 0;
        if (Session["category_id"] == null || Session["category_id"].ToString() == "" || !long.TryParse(Session["category_id"].ToString(), out categoryId) || categoryId <= 0)
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }
        
        long channelId;
        int orderNum;
        if (long.TryParse(id, out channelId) && int.TryParse(updatedOrderNum, out orderNum))
        {
            int updaterId = LoginManager.GetLoginID();
            if (!TvmDAL.UpdateChannelOrderNumInCategory(updaterId, categoryId, channelId, orderNum))
            {
                log.ErrorFormat("Failed updating channel {0} orderNum value to: {1} on category {2}", channelId, orderNum, categoryId);
            }
        }

        return "";
    }

    private void updateChannelOrderNumInCategory(long categoryId, long channelId)
    {
        throw new NotImplementedException();
    }

    public string initDualObj()
    {
        long categoryId = 0;
        if (Session["category_id"] == null || Session["category_id"].ToString() == "" || !long.TryParse(Session["category_id"].ToString(), out categoryId) || categoryId <= 0)
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Channels included in category");
        dualList.Add("FirstListWithOrderByButtons", true);
        dualList.Add("SecondListTitle", "Available Channels");

        object[] resultData = null;
        List<object> categoryChannelsData = new List<object>();
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        DataSet ds = TvmDAL.GetCategoriesPossibleChannels(nLogedInGroupID, categoryId);
        HashSet<long> includedChannelsHashset = new HashSet<long>();
        HashSet<long> availableChannelsHashset = new HashSet<long>();        
        if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
        {
            DataTable availableChannels = ds.Tables[0];
            DataTable channelsIncludedInCategory = ds.Tables[1];
            List<KeyValuePair<long, int>> categoryChannelsMap = new List<KeyValuePair<long,int>>();
            HashSet<long> channelsInCategory = new HashSet<long>();
            Dictionary<long, CategoryChannel> channelsToOrder = new Dictionary<long,CategoryChannel>();
            if (channelsIncludedInCategory != null && channelsIncludedInCategory.Rows != null)
            {
                foreach (DataRow dr in channelsIncludedInCategory.Rows)
                {
                    long channelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "CHANNEL_ID", 0);
                    int orderNum = ODBCWrapper.Utils.GetIntSafeVal(dr, "ORDER_NUM");
                    if (channelId > 0 && !channelsInCategory.Contains(channelId))
                    {
                        categoryChannelsMap.Add(new KeyValuePair<long, int>(channelId, orderNum));
                        channelsInCategory.Add(channelId);
                    }
                }                
            }
            
            if (availableChannels != null && availableChannels.Rows != null)
            {
                foreach (DataRow dr in availableChannels.Rows)
                {
                    long channelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    string groupName = ODBCWrapper.Utils.GetSafeStr(dr, "GROUP_NAME");
                    string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                    string adminName = ODBCWrapper.Utils.GetSafeStr(dr, "ADMIN_NAME");

                    string title = adminName;
                    if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(name))
                    {
                        title = string.Format("{0} - {1}", channelId, name);
                    }

                    title += "(" + groupName + ")";
                    if (channelsInCategory.Contains(channelId))
                    {
                        CategoryChannel categoryChannel = new CategoryChannel()
                        {
                            ID = channelId,
                            Title = title,
                            Description = title,
                            InList = true,
                            OrderNum = 0
                        };

                        channelsToOrder.Add(channelId, categoryChannel);
                        includedChannelsHashset.Add(channelId);
                    }
                    else
                    {
                        
                        var data = new
                        {
                            ID = channelId.ToString(),
                            Title = title,
                            Description = title,
                            InList = false
                        };

                        categoryChannelsData.Add(data);
                        availableChannelsHashset.Add(channelId);
                    }
                }

                foreach (KeyValuePair<long, int> pair in categoryChannelsMap)
                {
                    CategoryChannel categoryChannel = channelsToOrder[pair.Key];
                    categoryChannel.OrderNum = pair.Value;                    
                    categoryChannelsData.Add(categoryChannel);
                }

                Session["includedChannels"] = includedChannelsHashset;
                Session["availableChannels"] = availableChannelsHashset;
            }
        }

        resultData = new object[categoryChannelsData.Count];
        resultData = categoryChannelsData.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_categories_channels.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    public class CategoryChannel
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool InList { get; set; }
        public int OrderNum { get; set; }

        public CategoryChannel() { }
    }
}
