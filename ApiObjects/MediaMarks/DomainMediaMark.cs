using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.MediaMarks
{
    [Serializable]
    public class DomainMediaMark
    {
        [JsonProperty("devices")]
        public List<UserMediaMark> devices;

        [NonSerialized]
        public int domainID;

        public override string ToString()
        {
            return String.Concat("d", domainID);
        }
    }
}
