using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using TVinciShared;


public partial class adm_edit_group_permissions : System.Web.UI.Page
{

    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(1, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);
            if (Request.QueryString["selectedIndex"] != null && Request.QueryString["groupName"] != null)
            {
                int selectedIndex;
                if (int.TryParse(Request.QueryString["selectedIndex"], out selectedIndex))
                {
                    Session["selectedGroupName"] = Request.QueryString["groupName"];
                    GetAllGroupsList(selectedIndex);
                }
            }
            else
            {
                GetAllGroupsList();
            }
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ":";
        Response.Write(sRet + " Edit Group Permissions");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    private void GetAllGroupsList(int selectedIndex = -1)
    {
        if (Session["allGroupsTable"] != null)
        {
            groups.DataSource = (DataTable)Session["allGroupsTable"];
        }
        else
        {
            groups.DataSource = TvmDAL.GetAllGroups();
            Session["allGroupsTable"] = groups.DataSource;
        }
        groups.DataValueField = "moto_text";
        groups.DataTextField = "moto_text";
        groups.DataBind();
        if (selectedIndex > -1)
        {
            groups.SelectedIndex = selectedIndex;
        }
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Group Permissions");
        dualList.Add("SecondListTitle", "Available Permissions");

        object[] resultData = null;

        string groupName = null;
        if (Session["selectedGroupName"] != null)
        {
            groupName = Session["selectedGroupName"].ToString();
        }
        List<object> GroupPermissionsList = new List<object>();
        if (!string.IsNullOrEmpty(groupName))
        {
            DataTable allowedTable = new DataTable();
            DataTable notAllowedTable = TvmDAL.GetAllMenus();
            HashSet<string> groupAllowedPermissionsHashSet = new HashSet<string>();
            HashSet<string> groupNotAllowedPermissionsHashSet = new HashSet<string>();

            foreach (DataRow dr in allowedTable.Rows)
            {
                groupAllowedPermissionsHashSet.Add(dr[0].ToString());
                var data = new
                {
                    ID = dr[0],
                    Title = string.Format("{0}, href: {1}", dr[1], dr[2]),
                    Description = dr[2],
                    InList = true
                };
                GroupPermissionsList.Add(data);
            }

            foreach (DataRow dr in notAllowedTable.Rows)
            {
                groupNotAllowedPermissionsHashSet.Add(dr[0].ToString());
                var data = new
                {
                    ID = dr[0],
                    Title = string.Format("{0}, href: {1}", dr[1], dr[2]),
                    Description = dr[2],
                    InList = false
                };
                GroupPermissionsList.Add(data);
            }


            Session["groupAllowedPermissions"] = groupAllowedPermissionsHashSet;
            Session["groupNotAllowedPermissions"] = groupNotAllowedPermissionsHashSet;

        }

        resultData = new object[GroupPermissionsList.Count];
        resultData = GroupPermissionsList.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_edit_group_permissions.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    public string changeItemStatus(string sMenuID, string sAction)
    {
        string selectedGroupName = null;
        if (Session["selectedGroupName"] != null)
        {
            selectedGroupName = Session["selectedGroupName"].ToString();
            HashSet<string> groupAllowedPermissionsHashSet = new HashSet<string>();
            HashSet<string> groupNotAllowedPermissionsHashSet = new HashSet<string>();
            if (Session["groupAllowedPermissions"] != null && Session["groupNotAllowedPermissions"] != null)
            {
                groupAllowedPermissionsHashSet = (HashSet<string>)Session["groupAllowedPermissions"];
                groupNotAllowedPermissionsHashSet = (HashSet<string>)Session["groupNotAllowedPermissions"];
            }
            int menuID = 0;
            if (int.TryParse(sMenuID, out menuID) && !string.IsNullOrEmpty(selectedGroupName))
            {
                if (groupAllowedPermissionsHashSet.Contains(sMenuID) && TvmDAL.RemoveMenuFromGroup(selectedGroupName, menuID))
                {
                    groupAllowedPermissionsHashSet.Remove(sMenuID);
                    groupNotAllowedPermissionsHashSet.Add(sMenuID);
                }

                else if (groupNotAllowedPermissionsHashSet.Contains(sMenuID) && TvmDAL.AddMenuToGroup(selectedGroupName, menuID))
                {
                    groupNotAllowedPermissionsHashSet.Remove(sMenuID);
                    groupAllowedPermissionsHashSet.Add(sMenuID);
                }
            }

            Session["groupAllowedPermissions"] = groupAllowedPermissionsHashSet;
            Session["groupNotAllowedPermissions"] = groupNotAllowedPermissionsHashSet;
        }

        return "";
    }

}