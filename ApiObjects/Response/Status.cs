using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace ApiObjects.Response
{
    [Serializable]
    public class Status
    {
        public int Code { get; set; }

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
