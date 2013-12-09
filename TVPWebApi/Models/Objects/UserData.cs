using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models
{

    public class UserData
    {
        public UserBasicData basic_data { get; set; }

        public UserDynamicData dynamic_data { get; set; }

        //public string site_guid { get; set; }

        //public int domain_id { get; set; }

        //public bool is_domain_master { get; set; }

        //public UserState state { get; set; }

        //public int sso_operator_id { get; set; }
    }

    public enum UserState
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        Activated,

        /// <remarks/>
        SingleSignIn,

        /// <remarks/>
        DoubleSignIn,

        /// <remarks/>
        LoggedOut,
    }

    public class UserBasicData
    {
        public string user_name { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string email { get; set; }

        public string address { get; set; }

        public string city { get; set; }

        public State state { get; set; }

        public Country country { get; set; }

        public string zip { get; set; }

        public string phone { get; set; }

        public string facebook_id { get; set; }

        public string facebook_image { get; set; }

        public bool is_facebook_image_permitted { get; set; }

        public string affiliate_code { get; set; }

        public string co_guid { get; set; }

        public string external_token { get; set; }

        public string facebook_token { get; set; }

        public UserType user_type { get; set; }
    }

    public class State
    {
        public int object_id { get; set; }

        public string state_name { get; set; }

        public string state_code { get; set; }

        public Country country { get; set; }
    }

    public class Country
    {
        public int object_id { get; set; }

        public string country_name { get; set; }

        public string country_code { get; set; }
    }

    public class UserType
    {
        public int? id { get; set; }

        public string description { get; set; }

        public bool is_default { get; set; }
    }

    public class UserDynamicData
    {
        public UserDynamicDataContainer[] user_data;
    }

    public class UserDynamicDataContainer
    {
        public string data_type { get; set; }

        public string value { get; set; }
    }
}