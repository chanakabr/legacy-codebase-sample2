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
        [DataMember]
        public int Code { get; set; }

        [DataMember]
        public string Message { get; set; }

        public Status(int code = 0, string message = "")
        {
            this.Code = code;
            this.Message = message;
        }

        public Status()
        {
            this.Message = string.Empty;
        }
    }
}
