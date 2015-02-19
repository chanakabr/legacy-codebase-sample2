using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class Status
    {
        public StatusObjectCode status { get; set; }
        public int code { get; set; }
        public string message { get; set; }

        public Status(StatusObjectCode statusObjectCode = StatusObjectCode.Unknown, int code = 0, string message = "")
        {
            this.status = statusObjectCode;
            this.code = code;
            this.message = message;
        }
    }
}
