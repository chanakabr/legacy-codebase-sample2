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
using System.Text.RegularExpressions;
using System.Text;

public partial class adm_comments_filter_tester_popup : System.Web.UI.Page
{

    static Regex regex = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_comments_filter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_comments_filter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (Request.QueryString["regex"] != null && Request.QueryString["regex"].ToString().Trim() == "0")
        {
            regex = null;
            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();

            Int32 nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "parent_group_id", nOwnerGroupID).ToString());

            if (nParentGroupID != 1)
            {
                nOwnerGroupID = nParentGroupID;
            }

            object oPat = ODBCWrapper.Utils.GetTableSingleVal("group_language_filters", "Expression", "group_id", "=", nOwnerGroupID);
            if (oPat != null && oPat != DBNull.Value && !string.IsNullOrEmpty(oPat.ToString()))
            {
                string pattern = oPat.ToString();
                regex = new Regex(pattern, RegexOptions.IgnoreCase);
            }
        }


        if (!IsPostBack)
        {
            //Int32 nGroupID = LoginManager.GetLoginGroupID();
            //string pattern = GetRegExpression(nGroupID);
            
        }
    }
 
    protected void Button1_Click(object sender, EventArgs e)
    {
        string input = TextBox1.Text;
        string output = input;

        if (regex != null)
        {
            output = regex.Replace(input, "****");
        }

        TextBox2.Text = output;
    }
}
