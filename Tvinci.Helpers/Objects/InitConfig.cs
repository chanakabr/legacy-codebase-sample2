using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;



namespace Tvinci.Helpers.Objects
{


    /// <summary>
    /// Summary description for InitConfig
    /// </summary>
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




    }



}