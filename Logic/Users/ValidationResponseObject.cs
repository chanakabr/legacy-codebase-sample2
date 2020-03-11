using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Users
{
    [DataContract]
    public class ValidationResponseObject
    {
        [DataMember]
        public long m_lDomainID;
        [DataMember]
        public DomainResponseStatus m_eStatus;

        public ValidationResponseObject()
        {
            this.m_eStatus = DomainResponseStatus.UnKnown;
        }

        public ValidationResponseObject(DomainResponseStatus status, long lDomainID)
        {
            this.m_eStatus = status;
            this.m_lDomainID = lDomainID;
        }
    }
}
