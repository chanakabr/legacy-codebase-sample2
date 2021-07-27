using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ApiObjects.Response
{
    [Serializable]
    [DataContract]
    public class Status
    {
        private string message = string.Empty;
        private int code;
        private List<KeyValuePair> args;

        public static Status Ok => new Status(eResponseStatus.OK);
        public static Status Error => new Status(eResponseStatus.Error);
        public static Status ErrorMessage(string message) => new Status(eResponseStatus.Error, message);

        [DataMember]
        [JsonProperty("Code")]
        public int Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;

                // update status message 
                if (string.IsNullOrEmpty(message))
                    message = ((eResponseStatus)value).ToString();
            }
        }

        [DataMember]
        [JsonProperty("Message")]
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }

        [DataMember]
        [JsonProperty("Args")]
        public List<KeyValuePair> Args
        {
            get
            {
                return args;
            }
            set
            {
                args = value;
            }
        }

        public Status()
        {
        }

        public Status(eResponseStatus status, string message = null, List<KeyValuePair> args = null)
        {
            this.Set(status, message, args);

        }

        public Status(int code = 0, string message = "", List<KeyValuePair> args = null)
        {
            this.Set(code, message, args);
        }

        public void Set(int errorCode, string errorMessage = null, List<KeyValuePair> args = null)
        {
            this.code = errorCode;
            this.args = args;

            if (string.IsNullOrEmpty(errorMessage))
            {
                this.message = errorCode.ToString();
            }
            else
            {
                this.message = errorMessage;
            }
        }

        public void Set(eResponseStatus responseStatus, string responseMessage = null, List<KeyValuePair> args = null)
        {
            this.Set((int)responseStatus, string.IsNullOrEmpty(responseMessage) ? responseStatus.ToString() : responseMessage, args);
        }

        public void Set(Status newStatus)
        {
            if (newStatus != null)
            {
                this.code = newStatus.code;
                this.message = newStatus.message;
                this.args = newStatus.args;
            }
        }

        public void AddArg(eResponseStatus key, object value)
        {
            AddArg(((int)key).ToString(), value);
        }

        public void AddArg(string key, object value)
        {
            if (args == null)
            {
                args = new List<KeyValuePair>();
            }

            if (args.Count == 0 || !args.Any(x => x.key == key))
            {
                args.Add(new KeyValuePair(key, value != null ? value.ToString() : string.Empty));
            }
        }

        public bool IsOkStatusCode()
        {
            return code == (int)eResponseStatus.OK;
        }

        /// <summary>
        /// override Status.ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Code:{0}.", code));
            sb.AppendLine(string.Format("Message:{0}.", message));
            sb.AppendLine(string.Format("Args:{0}.", args != null && args.Count > 0 ? string.Join(",", args.Select(x => string.Format("Key:{0}, Value:{1}", x.key, x.value))) : string.Empty));

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var status = (Status)obj;

            return status.Code == Code && status.Message == Message;
        }
    }
}