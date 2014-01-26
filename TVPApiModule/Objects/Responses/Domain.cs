using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Domain
    {
        public string name { get; set; }

        public string description { get; set; }

        public string coGuid { get; set; }

        public int domainID { get; set; }

        public int groupID { get; set; }

        public int limit { get; set; }

        public int deviceLimit { get; set; }

        public int userLimit { get; set; }

        public int concurrentLimit { get; set; }

        public int status { get; set; }

        public int isActive { get; set; }

        public int[] usersIDs { get; set; }

        public DeviceContainer[] deviceFamilies { get; set; }

        public int[] masterGUIDs { get; set; }

        public DomainStatus domainStatus { get; set; }

        public int frequencyFlag { get; set; }

        public System.DateTime nextActionFreq { get; set; }
    }
}
