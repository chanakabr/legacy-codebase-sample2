using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Response
{

    [DataContract]
    public class MediaIdsResponse : BaseResponse
    {
        [DataMember]
        public List<SearchResult> m_nMediaIds;

        
        public MediaIdsResponse()           
        {
            m_nMediaIds = new List<SearchResult>();
        }       
    }
}
