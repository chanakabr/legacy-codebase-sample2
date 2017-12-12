using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;


namespace ApiObjects.Response
{
    [Serializable]
    [DataContract]
    public class Status
    {
        private string message = string.Empty;
        private int code;
        private List<KeyValuePair> args;

        public Status(int code = 0, string message = "", List<KeyValuePair>  args= null)
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
    }
}
