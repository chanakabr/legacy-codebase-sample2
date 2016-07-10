using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects
{
    [DataContract(Namespace = "", Name = "Feeder")]
    public class IngestRequest
    {
        [DataMember(Name = "userName", Order = 0)]
        public string UserName { get; set; }

        [DataMember(Name = "passWord", Order = 1)]
        public string Password { get; set; }

        [DataMember(Name = "data", Order = 2)]
        public string Data { get; set; }
    }
}
