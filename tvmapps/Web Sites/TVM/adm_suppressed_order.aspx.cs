using Phx.Lib.Log;
using System;
using System.Reflection;
using TVinciShared;
using System.Linq;

public partial class adm_suppressed_order : System.Web.UI.Page
{

    protected string m_sMenu;
    protected string m_sSubMenu;
    const string metaIndexes = "meta_index";


    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media_types.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_media_types.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        
        Int32 nMenuID = 0;

        m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
        
        if (!IsPostBack)
        {
            // in case session wasn't nullify
            ClearSession();

            if (!string.IsNullOrEmpty(Request.QueryString[metaIndexes]))
            {
                Session[metaIndexes] = Request.QueryString[metaIndexes].ToString();
            }
            else
            {
                var groupId = LoginManager.GetLoginGroupID();
                var _list = this.GetListIndexes(groupId);
                Session[metaIndexes] = _list;
                txtName.Text = _list.ToString().Trim();
            }
        }
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        var groupID = LoginManager.GetLoginGroupID();
        string name = txtName.Text.Trim();

        var success = this.UpdateIndexes(groupID, name);
        if (success)
            ClearSession();

        Page_Load(sender, e);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Meta Suppressed Order");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }


    private void ClearSession()
    {
        // in case session wasn't nullify
        Session[metaIndexes] = null;
    }

    string GetListIndexes(int groupId)
    {
        var items = Core.Api.api.GetMediaSuppressedIndexes(groupId);
        if (!items.IsOkStatusCode() || items == null || !items.Object.Any())
            return string.Empty;

        return string.Join(", ", items.Object);
    }

    bool UpdateIndexes(int groupId, string newList)
    {
        var hs = newList.Split(',').Select(x => x.Trim()).ToHashSet();
        var response = Core.Api.api.UpdateMediaSuppressedIndexes(groupId, hs);
        return response != null && response.Object;
    }
}
