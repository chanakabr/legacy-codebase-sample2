using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
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

    public class Status
    {
        public Code m_SuccessCode;

        public Status()
        {
            m_SuccessCode = Code.Success;
        }

        public Status(bool succeeded)
        {
            if (succeeded)
            {
                m_SuccessCode = Code.Success;
            }
            else
            {
                m_SuccessCode = Code.Failure;
            }
        }
    }
}
