using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;

namespace Core.Catalog.Response
{
    [DataContract]
    [KnownType(typeof(RecordingSearchResult))]
    public class UnifiedSearchResult : BaseObject
    {

    }

    [DataContract]
    public class RecordingSearchResult : UnifiedSearchResult
    {
        [DataMember]
        public string EpgId
        {
            get;
            set;
        }
    }
}
