using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using TVinciShared;


public partial class adm_edit_group_members : System.Web.UI.Page
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
                    Session["selectedGroupToEdit"] = Request.QueryString["groupName"];
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
        if (Session["allGroupsDT"] != null)
        {
            groups.DataSource = (DataTable)Session["allGroupsDT"];
        }
        else
        {
            groups.DataSource = TvmDAL.GetAllGroups();
            Session["allGroupsDT"] = groups.DataSource;
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
        dualList.Add("FirstListTitle", "Group Members");
        dualList.Add("SecondListTitle", "Other Users");

        object[] resultData = null;

        string groupName = null;
        if (Session["selectedGroupToEdit"] != null)
        {
            groupName = Session["selectedGroupToEdit"].ToString();
        }
        List<object> GroupMembersList = new List<object>();
        if (!string.IsNullOrEmpty(groupName))
        {
            DataTable membersTable = TvmDAL.GetAllUsersInGroup(groupName);
            DataTable otherUsersTable = TvmDAL.GetAllUsersNotInGroup(groupName);
            HashSet<string> groupMembersHashSet = new HashSet<string>();
            HashSet<string> otherUsersHashSet = new HashSet<string>();

            foreach (DataRow dr in membersTable.Rows)
            {
                groupMembersHashSet.Add(dr[0].ToString());
                var data = new
                {
                    ID = dr[0],
                    Title = dr[1],
                    Description = dr[1],
                    InList = true
                };
                GroupMembersList.Add(data);
            }

            foreach (DataRow dr in otherUsersTable.Rows)
            {
                otherUsersHashSet.Add(dr[0].ToString());
                var data = new
                {
                    ID = dr[0],
                    Title = dr[1],
                    Description = dr[1],
                    InList = false
                };
                GroupMembersList.Add(data);
            }


            Session["groupMembers"] = groupMembersHashSet;
            Session["otherUsers"] = otherUsersHashSet;

        }

        resultData = new object[GroupMembersList.Count];
        resultData = GroupMembersList.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_edit_group_members.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    public string changeItemStatus(string sUserID, string sAction)
    {
        string selectedGroupName = null;
        if (Session["selectedGroupToEdit"] != null)
        {
            selectedGroupName = Session["selectedGroupToEdit"].ToString();
            HashSet<string> groupMembersHashSet = new HashSet<string>();
            HashSet<string> otherUsersHashSet = new HashSet<string>();
            if (Session["groupMembers"] != null && Session["otherUsers"] != null)
            {
                groupMembersHashSet = (HashSet<string>)Session["groupMembers"];
                otherUsersHashSet = (HashSet<string>)Session["otherUsers"];
            }
            int UserID = 0;
            if (int.TryParse(sUserID, out UserID) && !string.IsNullOrEmpty(selectedGroupName))
            {
                if (groupMembersHashSet.Contains(sUserID) && TvmDAL.RemoveUserFromGroup(UserID))
                {
                    groupMembersHashSet.Remove(sUserID);
                    otherUsersHashSet.Add(sUserID);
                }

                else if (otherUsersHashSet.Contains(sUserID) && TvmDAL.AddUserToGroup(selectedGroupName, UserID))
                {
                    otherUsersHashSet.Remove(sUserID);
                    groupMembersHashSet.Add(sUserID);
                }
            }

            Session["groupMembers"] = groupMembersHashSet;
            Session["otherUsers"] = otherUsersHashSet;
        }

        return "";
    }

}