using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog.Response
{
    [DataContract]
    public class DomainLastPositionResponse : BaseResponse
    {
        [DataMember]
        public string m_sStatus;

        [DataMember]
        public string m_sDescription;

        [DataMember]
        public List<Bookmark> m_lPositions;

        [DataMember]
        public Status Status;


        public DomainLastPositionResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }
}