using ApiObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InitiateNotificationActionHandler
{
    [Serializable]
    public class InitiateNotificationActionRequest
    {
        [JsonProperty("group_id")]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("user_action")]
        public int UserAction
        {
            get;
            set;
        }

        [JsonProperty("user_id")]
        public int UserId
        {
            get;
            set;
        }

        [JsonProperty("udid")]
        public string Udid
        {
            get;
            set;
        }

        [JsonProperty("push_token")]
        public string pushToken
        {
            get;
            set;
        }
    }
}
