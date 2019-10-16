using ApiObjects;
using EventBus.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class InitiateNotificationActionRequest : ServiceEvent
    {
        [JsonProperty("user_action")]
        public eUserMessageAction UserAction
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

        public override string ToString()
        {
            return $"{{{nameof(UserAction)}={UserAction}, {nameof(Udid)}={Udid}, {nameof(pushToken)}={pushToken}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
