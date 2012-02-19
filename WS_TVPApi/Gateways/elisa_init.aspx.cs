using System;
using System.Web.Script.Serialization;
using System.Collections;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        String jsonStr = serializer.Serialize(new InitConfig());
        Response.Write(jsonStr);
    }

    class InitConfig
    {
        public string LogoURL = "http://ibc.cdngc.net/CTV/CTV_logo_Motorola.png";
        public string HomePageChannelID = "327200";
        public string RootCateroryID = "1142";
        public string GatewayURL = "http://173.231.146.34:9003/tvpapi/gateways/jsongateway.aspx";
        public string ApiUser = "tvpapi_125";
        public string ApiPass = "11111";
        public string Platform = "ConnectedTV";
        public ArrayList Users = new ArrayList();

        public InitConfig()
        {
            Users.Add(new User("demo@tvinci.com", "123456"));
        }

        class User
        {

            public string UserName;
            public string Password;

            public User(string userName, string password)
            {
                this.UserName = userName;
                this.Password = password;
            }
        }

        
    }

}