using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class DomainResponseObject
    {
        public Domain m_oDomain;
        public DomainResponseStatus m_oDomainResponseStatus;

        public DomainResponseObject()
        {

        }


        public DomainResponseObject(Domain oDomain, DomainResponseStatus oDomainResponseStatus)
        {
            m_oDomain = oDomain;
            m_oDomainResponseStatus = oDomainResponseStatus;
        }
    }
}
