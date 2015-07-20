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

        public Status(int code = 0, string message = "")
        {
            this.Code = code;
            this.Message = message;
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
    }
}
