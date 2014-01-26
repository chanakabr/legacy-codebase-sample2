using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class DeviceDomain
    {
        public string site_guid;
        public int domain_id;
        public string domain_name;
    }
}
