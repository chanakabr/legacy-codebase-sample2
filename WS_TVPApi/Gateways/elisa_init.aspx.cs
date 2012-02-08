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
        public string LogoURL = "http://tvinci.cdnetworks.net/tvinci_logo.jpg";
        public string HomePageChannelID = "327149";
        public string RootCateroryID = "1065";
        public string GatewayURL = "http://173.231.146.34:9003/tvpapi/gateways/jsongateway.aspx";
        public string ApiUser = "tvpapi_125";
        public string ApiPass = "11111";
        public string Platform = "iPad";
        public ArrayList Users = new ArrayList();

        public InitConfig()
        {
            Users.Add(new User("demo@tvinci.com", "123456"));
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