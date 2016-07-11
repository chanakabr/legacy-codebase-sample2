using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;

namespace Catalog.Response
{
    [DataContract]
    public class ExtendedSearchResult: UnifiedSearchResult 
    {
        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }
    }
}
