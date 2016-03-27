using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    [Serializable]
    public class InitiateNotificationActionRequest
    {
        [JsonProperty("sWSUserName")]
        public string WsUserName;

        [JsonProperty("sWSPassword")]
        public string WsPassword;

        [JsonProperty("userAction")]
        public eUserMessageAction UserAction;

        [JsonProperty("userId")]
        public string UserID;

        [JsonProperty("udid")]
        public string UDID;

        [JsonProperty("pushToken")]
        public string PushToken;

        public InitiateNotificationActionRequest(string wsUserName, string wsPassword, eUserMessageAction userAction, string userID, string udid, string pushToken)
        {
            WsUserName = wsUserName;
            WsPassword = wsPassword;
            UserAction = userAction;
            UserID = userID;
            UDID = udid;
            PushToken = pushToken;
        }
    }
}
