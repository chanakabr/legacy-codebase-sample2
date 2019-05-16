using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using TVinciShared;

public partial class adm_copy_user_permissions : System.Web.UI.Page
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
            if (Request.QueryString["selectedSourceIndex"] != null && Request.QueryString["sourceUserID"] != null 
                && Request.QueryString["selectedDestinationIndex"] != null && Request.QueryString["destinationUserID"] != null)
            {
                int sourceUserID, selectedSourceIndex, destinationUserID, selectedDestinationIndex;
                if (int.TryParse(Request.QueryString["selectedSourceIndex"], out selectedSourceIndex) && int.TryParse(Request.QueryString["sourceUserID"], out sourceUserID)
                    && int.TryParse(Request.QueryString["selectedDestinationIndex"], out selectedDestinationIndex) && int.TryParse(Request.QueryString["destinationUserID"], out destinationUserID))
                {
                    Session["sourceUserID"] = sourceUserID;
                    Session["destinationUserID"] = destinationUserID;
                    GetAllUsersList(selectedSourceIndex, selectedDestinationIndex);
                    if (CopyUserPermissions(sourceUserID, destinationUserID))
                    {
                        actionResponse.InnerText = string.Format("{0} permissions were copied to {1} successfully {2}", usersSource.Items[selectedSourceIndex].Text, usersDestination.Items[selectedDestinationIndex].Text, DateTime.Now.ToString());
                    }
                    else
                    {
                        actionResponse.InnerText = "Failed while trying to copy permissions";
                    }
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
        Response.Write(sRet + " Copy User Permissions");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    private bool CopyUserPermissions(int sourceUserID, int destinationUserID)
    {        
        if (sourceUserID > 0 && destinationUserID > 0)
        {
            if (sourceUserID == destinationUserID)
            {
                return true;
            }
            else
            {
                return TvmDAL.CopyUserPermissions(sourceUserID, destinationUserID);
            }
        }

        return false;
    }

    private void GetAllUsersList(int selectedSourceIndex = -1, int selectedDestinationIndex = -1)
    {
        if (Session["allUsersTable"] != null)
        {
            usersSource.DataSource = (DataTable)Session["allUsersTable"];
            usersDestination.DataSource = (DataTable)Session["allUsersTable"];
        }
        else
        {
            usersSource.DataSource = TvmDAL.GetAllUsers();
            usersDestination.DataSource = usersSource.DataSource;
            Session["allUsersTable"] = usersSource.DataSource;
        }
        usersSource.DataValueField = "id";
        usersSource.DataTextField = "username";
        usersSource.DataBind();
        usersDestination.DataValueField = "id";
        usersDestination.DataTextField = "username";
        usersDestination.DataBind();

        if (selectedSourceIndex > -1)
        {
            usersSource.SelectedIndex = selectedSourceIndex;
        }
        if (selectedDestinationIndex > -1)
        {
            usersDestination.SelectedIndex = selectedDestinationIndex;
        }
    }

}