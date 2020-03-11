using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog
{
    [DataContract]
    public class AdProvider
    {
        [DataMember]
        public int ProviderID;
        [DataMember]
        public string ProviderName;

        public AdProvider()
        {
            ProviderID = 0;
            ProviderName = string.Empty;
        }
    }
}
