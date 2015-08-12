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
        public List<LastPosition> m_lPositions;

        [DataMember]
        public Status Status;


        public DomainLastPositionResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }

    [DataContract]
    public class LastPosition
    {
        [DataMember]
        public int m_nUserID;

        [DataMember]
        public eUserType m_eUserType;

        [DataMember]
        public int m_nLocation;

        public LastPosition()
        {
        }

        public LastPosition(int nUserID, eUserType eUserType, int nLocation)
        {
            this.m_nUserID = nUserID;
            this.m_eUserType = eUserType;
            this.m_nLocation = nLocation;
        }
    }
}
