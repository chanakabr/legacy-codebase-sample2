using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TVPApiServices
{

    [DataContract]
    public class OrderObj
    {
        [DataMember]
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy m_eOrderBy { get; set; }
        [DataMember]
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir m_eOrderDir { get; set; }
        [DataMember]
        public string m_sOrderValue { get; set; }
    }
}