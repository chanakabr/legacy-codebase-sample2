using System;
using System.Web.Script.Serialization;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.ComponentModel;

public partial class Gateways_tvinci_init : System.Web.UI.Page
{
    //protected string m_sSettingXMLPath = @"D:\ode\Projects\TVPProAPIs\WS_TVPApi\Gateways\TvinciSettings.xml";
    protected string m_sSettingXMLPath = @"~/TvinciSettings.xml";

    protected override void OnInit(EventArgs e)
    {
        gvUsers.RowDeleting += new GridViewDeleteEventHandler(gvUsers_RowDeleting);
        btnAdd.Command += new CommandEventHandler(btnAdd_Command);
        btnSave.Command += new CommandEventHandler(btnSave_Command);
        btnPreview.Command += new CommandEventHandler(btnPreview_Command);

        m_sSettingXMLPath = Server.MapPath(m_sSettingXMLPath);

        base.OnInit(e);
    }

    void btnPreview_Command(object sender, CommandEventArgs e)
    {
        Response.Redirect("./tvinci_init.aspx", true);
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        InitConfig initConfig = new InitConfig();

        XmlSerializer x = new XmlSerializer(initConfig.GetType());
        using (StreamReader reader = new StreamReader(m_sSettingXMLPath))
        {
            object o = x.Deserialize(reader);
            if (o != null)
            {
                initConfig = (InitConfig)o;
            }
        }

        string sEditMode = Request.QueryString["Edit"];
        if (!string.IsNullOrEmpty(sEditMode))
        {
            if (!IsPostBack)
            {
                tbApiPass.Text = initConfig.ApiPass;
                tbApiUser.Text = initConfig.ApiUser;
                tbGatewayURL.Text = initConfig.GatewayURL;
                tbHomePageChannelID.Text = initConfig.HomePageChannelID;
                tbLogoURL.Text = initConfig.LogoURL;
                tbPlatform.Text = initConfig.Platform;
                tbRootCateroryID.Text = initConfig.RootCateroryID;
            }
            //BindingList<InitConfig.User> users 
            gvUsers.DataSource = initConfig.Users;

            this.DataBind();
        }
        else
        {
            Response.Clear();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            String jsonStr = serializer.Serialize(initConfig);
            Response.Write(jsonStr);
            Response.End();
        }

    }

    void btnSave_Command(object sender, CommandEventArgs e)
    {
        SaveData();
    }

    void btnAdd_Command(object sender, CommandEventArgs e)
    {
        if (gvUsers.DataSource == null) gvUsers.DataSource = new BindingList<InitConfig.User>();
        (gvUsers.DataSource as BindingList<InitConfig.User>).Add(new InitConfig.User(tbUsername.Text, tbPassword.Text));
        gvUsers.DataBind();

        SaveData();
    }

    void gvUsers_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        (gvUsers.DataSource as BindingList<InitConfig.User>).RemoveAt(e.RowIndex);
        gvUsers.DataBind();

        SaveData();
    }

    public void SaveData()
    {
        try
        {
            InitConfig initConfig = new InitConfig();
            initConfig.ApiPass = tbApiPass.Text;
            initConfig.ApiUser = tbApiUser.Text;
            initConfig.GatewayURL = tbGatewayURL.Text;
            initConfig.HomePageChannelID = tbHomePageChannelID.Text;
            initConfig.LogoURL = tbLogoURL.Text;
            initConfig.Platform = tbPlatform.Text;
            initConfig.RootCateroryID = tbRootCateroryID.Text;

            initConfig.Users = (gvUsers.DataSource as BindingList<InitConfig.User>);

            XmlSerializer serial = new XmlSerializer(initConfig.GetType());

            using (StreamWriter sr = new StreamWriter(m_sSettingXMLPath))
            {
                serial.Serialize(sr, initConfig);
            }
        }
        catch (Exception ex) { Response.Write(ex.ToString()); }
    }

    [Serializable]
    public class InitConfig
    {
        public string LogoURL = string.Empty;
        public string HomePageChannelID = string.Empty;
        public string RootCateroryID = string.Empty;
        public string GatewayURL = string.Empty;
        public string ApiUser = string.Empty;
        public string ApiPass = string.Empty;
        public string Platform = string.Empty;
        public BindingList<User> Users = new BindingList<User>();

        public InitConfig()
        {
            //Users.Add(new User("avidan@tvinci.com", "eliron27"));
        }

        [Serializable]
        public class User
        {

            public string UserName { set; get; }
            public string Password { set; get; }

            public User()
            {
                
            }

            public User(string userName, string password)
            {
                this.UserName = userName;
                this.Password = password;
            }
        }


    }

}