using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using TVinciShared;


public partial class adm_edit_user_permissions : System.Web.UI.Page
{
    
    protected string m_sMenu;
    protected string m_sSubMenu;    
    public int testIndex;

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
            if (Request.QueryString["selectedIndex"] != null && Request.QueryString["userID"] != null)
            {
                int userID, selectedIndex;
                if (int.TryParse(Request.QueryString["selectedIndex"], out selectedIndex) && int.TryParse(Request.QueryString["userID"], out userID))
                {
                    Session["selectedUserID"] = userID;
                    GetAllUsersList(selectedIndex);
                }
            }
            else
            {
                GetAllUsersList();
            }
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ":";
        Response.Write(sRet + " Edit User Permissions");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    private void GetAllUsersList(int selectedIndex = -1)
    {
        if (Session["allUsersTable"] != null)
        {
            users.DataSource = (DataTable)Session["allUsersTable"];
        }
        else
        {
            users.DataSource = TvmDAL.GetAllUsers();
            Session["allUsersTable"] = users.DataSource;
        }        
        users.DataValueField = "id";
        users.DataTextField = "username";
        users.DataBind();
        if (selectedIndex > -1)
        {
            users.SelectedIndex = selectedIndex;
        }
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "User Permissions");
        dualList.Add("SecondListTitle", "Available Permissions");        

        object[] resultData = null;

        int selectedUserID = 0;
        if (Session["selectedUserID"] != null)
        {
            selectedUserID = int.Parse(Session["selectedUserID"].ToString());
        }
        List<object> userPermissionsList = new List<object>();
        if (selectedUserID > 0)
        {
            DataTable allowedTable = TvmDAL.GetUsersAllowedMenus(selectedUserID);
            DataTable notAllowedTable = TvmDAL.GetUsersNotAllowedMenus(selectedUserID);
            HashSet<string> userAllowedPermissionsHashSet = new HashSet<string>();
            HashSet<string> userNotAllowedPermissionsHashSet = new HashSet<string>();

            foreach (DataRow dr in allowedTable.Rows)
            {
                userAllowedPermissionsHashSet.Add(dr[0].ToString());
                var data = new
                {
                    ID = dr[0],
                    Title = string.Format("{0}, href: {1}", dr[1], dr[2]),
                    Description = dr[2],
                    InList = true
                };
                userPermissionsList.Add(data);
            }

            foreach (DataRow dr in notAllowedTable.Rows)
            {
                userNotAllowedPermissionsHashSet.Add(dr[0].ToString());
                var data = new
                {
                    ID = dr[0],
                    Title = string.Format("{0}, href: {1}", dr[1], dr[2]),
                    Description = dr[2],
                    InList = false
                };
                userPermissionsList.Add(data);
            }


            Session["userAllowedPermissions"] = userAllowedPermissionsHashSet;
            Session["userNotAllowedPermissions"] = userNotAllowedPermissionsHashSet;

        }

        resultData = new object[userPermissionsList.Count];
        resultData = userPermissionsList.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_edit_user_permissions.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }       

    public string changeItemStatus(string sMenuID, string sAction)
    {
        int selectedUserID = 0;
        if (Session["selectedUserID"] != null)
        {
            selectedUserID = int.Parse(Session["selectedUserID"].ToString());
            HashSet<string> userAllowedPermissionsHashSet = new HashSet<string>();
            HashSet<string> userNotAllowedPermissionsHashSet = new HashSet<string>();
            if (Session["userAllowedPermissions"] != null && Session["userNotAllowedPermissions"] != null)
            {
                userAllowedPermissionsHashSet = (HashSet<string>)Session["userAllowedPermissions"];
                userNotAllowedPermissionsHashSet = (HashSet<string>)Session["userNotAllowedPermissions"];
            }
            int menuID = 0;
            if (int.TryParse(sMenuID, out menuID))
            {
                if (userAllowedPermissionsHashSet.Contains(sMenuID) && TvmDAL.RemoveMenuFromUser(selectedUserID, menuID))
                {
                    userAllowedPermissionsHashSet.Remove(sMenuID);
                    userNotAllowedPermissionsHashSet.Add(sMenuID);
                }

                else if (userNotAllowedPermissionsHashSet.Contains(sMenuID) && TvmDAL.AddMenuToUser(selectedUserID, menuID))
                {
                    userNotAllowedPermissionsHashSet.Remove(sMenuID);
                    userAllowedPermissionsHashSet.Add(sMenuID);
                }
            }

            Session["userAllowedPermissions"] = userAllowedPermissionsHashSet;
            Session["userNotAllowedPermissions"] = userNotAllowedPermissionsHashSet;
        }

        return "";
    }        
   
}