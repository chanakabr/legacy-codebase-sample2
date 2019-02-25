using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;

using System.ComponentModel;

namespace ApiObjects.Response
{
    [Serializable]
    [DataContract]
    public class Status
    {
        private string message = string.Empty;
        private int code;
        private List<KeyValuePair> args;

        public Status(int code = 0, string message = "", List<KeyValuePair> args = null)
        {
            this.Code = code;
            this.Message = message;
            this.args = args;
        }

        public Status()
        {
        }

        [DataMember]
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

        public void Set(int newCode, string newMessage = null)
        {
            this.code = newCode;

            if (string.IsNullOrEmpty(newMessage))
            {
                this.message = newCode.ToString();
            }
            else
            {
                this.message = newMessage;
            }
        }

        public void AddArg(string key, string value)
        {
            if (args == null)
            {
                args = new List<KeyValuePair>();
            }

            args.Add(new KeyValuePair(key, value));
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
    }
}
