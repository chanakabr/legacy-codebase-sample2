using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using TVPApiModule.Objects.CRM;

namespace TVPApiServices
{

    public class DummyChargeUserForMediaFileRequest
    {
        [JsonProperty(Required = Required.Always)]
        public double price { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string currency;

        [JsonProperty(Required = Required.Always)]
        public int file_id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ppv_module_code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string user_ip { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string coupon { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string site_guid { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string udid { get; set; }
    }

    public class DummyChargeUserForSubscriptionRequest
    {
        [JsonProperty(Required = Required.Always)]
        public double price { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string currency { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string subscription_id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string coupon_code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string user_ip { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string site_guid { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string extra_parameters { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string udid { get; set; }
    }

    public class GetUserByUsernameRequest
    {
        [JsonProperty(Required = Required.Always)]
        public string user_name { get; set; }
    }

    public class SearchUsersRequest
    {
        [JsonProperty(Required = Required.Always)]
        public string text { get; set; }
    }
    

}

