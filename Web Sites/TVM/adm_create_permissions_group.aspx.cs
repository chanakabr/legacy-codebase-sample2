using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using TVinciShared;

public partial class adm_create_permissions_group : System.Web.UI.Page
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
            if (Request.QueryString["selectedIndex"] != null && Request.QueryString["userID"] != null && Request.QueryString["NewGroupName"] != null)
            {
                int userID, selectedIndex;
                string newGroupName = Request.QueryString["NewGroupName"];
                if (int.TryParse(Request.QueryString["selectedIndex"], out selectedIndex) && int.TryParse(Request.QueryString["userID"], out userID))
                {
                    Session["selectedUserID"] = userID;
                    GetAllUsersList(selectedIndex);
                    if (CreateNewGroupForUser(newGroupName, userID))
                    {
                        actionResponse.InnerText = string.Format("group {0} created and added {1} to group successfully {2}", newGroupName, users.Items[selectedIndex].Text, DateTime.Now.ToString());
                    }
                    else
                    {
                        actionResponse.InnerText = "Failed while trying to create new group";
                    }
                }
                else
                {

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
        Response.Write(sRet + " New Permissions Group");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    private bool CreateNewGroupForUser(string newGroupName, int userID)
    {
        if (!string.IsNullOrEmpty(newGroupName) && userID > 0)
        {
            return TvmDAL.AddUserToGroup(newGroupName, userID);
        }

        return false;
    }

    private void GetAllUsersList(int selectedIndex = -1)
    {
        if (Session["newGroupUsersTable"] != null)
        {
            users.DataSource = (DataTable)Session["newGroupUsersTable"];
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

}