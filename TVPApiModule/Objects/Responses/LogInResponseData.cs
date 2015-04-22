using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class LogInResponseData
    {
        public string SiteGuid { get; set; }
        public int DomainID { get; set; }
        public eResponseStatus LoginStatus { get; set; }
        public User UserData { get; set; }
    }
}
