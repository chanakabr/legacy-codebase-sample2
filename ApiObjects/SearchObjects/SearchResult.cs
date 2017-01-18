using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public int assetID { get; set; }
        [DataMember]
        public DateTime UpdateDate { get; set; }
    }
}
