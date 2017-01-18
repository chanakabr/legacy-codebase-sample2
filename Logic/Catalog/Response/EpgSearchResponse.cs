using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Response
{
    [DataContract]
    public class EpgSearchResponse : BaseResponse
    {
        [DataMember]
        public List<SearchResult> m_nEpgIds;

        public EpgSearchResponse() : base ()
        {
            m_nEpgIds = new List<SearchResult>();
        }
    }
}


