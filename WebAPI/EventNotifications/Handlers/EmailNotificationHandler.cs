using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.EventNotifications
{
    public class EmailNotificationHandler : NotificationEventHandler
    {
        EmailNotificationDefinitions definitions = null;

        public EmailNotificationHandler(string definitionsJson) : base(definitionsJson)
        {
            JObject json = JObject.Parse(definitionsJson);

            definitions = json.ToObject<EmailNotificationDefinitions>();
        }

        internal override void HandleEvent(EventManager.KalturaEvent kalturaEvent, object t)
        {
            // to do
        }
    }

    [Serializable]
    public class EmailNotificationDefinitions
    {
        [JsonProperty("subject")]
        public string Subject
        {
            get;
            set;
        }

        [JsonProperty("body")]
        public string Body
        {
            get;
            set;
        }

        [JsonProperty("from_email")]
        public string FromEmail
        {
            get;
            set;
        }

        [JsonProperty("from_name")]
        public string FromName
        {
            get;
            set;
        }

        [JsonProperty("to")]
        public string To
        {
            get;
            set;
        }

        [JsonProperty("cc")]
        public string Cc
        {
            get;
            set;
        }

        [JsonProperty("bcc")]
        public string Bcc
        {
            get;
            set;
        }

        [JsonProperty("reply_to")]
        public string ReplyTo
        {
            get;
            set;
        }

        [JsonProperty("hostname")]
        public string Hostname
        {
            get;
            set;
        }


    }
}