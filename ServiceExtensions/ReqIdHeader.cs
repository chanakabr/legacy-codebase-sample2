using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace ServiceExtensions
{
    public class ReqIdHeader : SoapHeader
    {
        public string kmon_req_id { get; set; }
    }
}
